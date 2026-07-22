import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import ConversationPage from './ConversationPage'
import { useAuth } from '../contexts/AuthContext'
import { useConversations, useMarkConversationAsRead, useMessages, useSendMessage } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import type { ConversationSummaryResponse, MessageResponse } from '../types/chat'
import type { ListingResponse } from '../types/listing'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))
vi.mock('../hooks/useChat', () => ({
  useConversations: vi.fn(),
  useMarkConversationAsRead: vi.fn(),
  useMessages: vi.fn(),
  useSendMessage: vi.fn(),
}))
vi.mock('../hooks/useListings', () => ({ useListing: vi.fn() }))

// jsdom doesn't implement scrollIntoView - the page calls it to keep the latest message in view.
Element.prototype.scrollIntoView = vi.fn()

function makeConversation(overrides: Partial<ConversationSummaryResponse> = {}): ConversationSummaryResponse {
  return {
    id: 'conversation-1',
    listingId: 'listing-1',
    otherParticipantId: 'seller-1',
    lastMessageBody: null,
    lastMessageAt: null,
    unreadCount: 0,
    ...overrides,
  }
}

function makeListing(overrides: Partial<ListingResponse> = {}): ListingResponse {
  return {
    id: 'listing-1',
    sellerId: 'seller-1',
    categoryId: 'cat-1',
    categoryName: 'Ropes',
    title: 'Petzl GriGri',
    description: 'Barely used belay device.',
    price: 65,
    currency: 'EUR',
    condition: 'Good',
    status: 'Active',
    location: 'Chamonix',
    isPromoted: false,
    createdAt: '2026-01-01T00:00:00Z',
    expiresAt: null,
    images: [],
    ...overrides,
  }
}

function makeMessage(overrides: Partial<MessageResponse> = {}): MessageResponse {
  return {
    id: 'message-1',
    conversationId: 'conversation-1',
    senderId: 'seller-1',
    body: 'Is this still available?',
    sentAt: new Date().toISOString(),
    readAt: null,
    ...overrides,
  }
}

function mockSharedHooks() {
  vi.mocked(useAuth).mockReturnValue({ user: { id: 'buyer-1' } } as unknown as ReturnType<typeof useAuth>)
  vi.mocked(useConversations).mockReturnValue({ data: [makeConversation()] } as unknown as ReturnType<typeof useConversations>)
  vi.mocked(useListing).mockReturnValue({ data: makeListing() } as unknown as ReturnType<typeof useListing>)
  vi.mocked(useMarkConversationAsRead).mockReturnValue({ mutate: vi.fn() } as unknown as ReturnType<typeof useMarkConversationAsRead>)
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/messages/:id" element={<ConversationPage />} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('ConversationPage', () => {
  it('shows a loading message while messages load', () => {
    mockSharedHooks()
    vi.mocked(useMessages).mockReturnValue({ isLoading: true, data: undefined } as unknown as ReturnType<typeof useMessages>)
    vi.mocked(useSendMessage).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useSendMessage>)

    renderAt('/messages/conversation-1')
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('renders messages aligned by sender and links back to the listing', () => {
    mockSharedHooks()
    vi.mocked(useMessages).mockReturnValue({
      isLoading: false,
      data: [makeMessage({ senderId: 'seller-1', body: 'Is this still available?' }), makeMessage({ id: 'message-2', senderId: 'buyer-1', body: 'Yes, still have it!' })],
    } as unknown as ReturnType<typeof useMessages>)
    vi.mocked(useSendMessage).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useSendMessage>)

    renderAt('/messages/conversation-1')
    expect(screen.getByText('Is this still available?')).toBeInTheDocument()
    expect(screen.getByText('Yes, still have it!')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'About: Petzl GriGri' })).toHaveAttribute('href', '/listings/listing-1')
  })

  it('sends a message and clears the draft input', async () => {
    mockSharedHooks()
    vi.mocked(useMessages).mockReturnValue({ isLoading: false, data: [] } as unknown as ReturnType<typeof useMessages>)
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useSendMessage).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useSendMessage>)
    const user = userEvent.setup()

    renderAt('/messages/conversation-1')
    const input = screen.getByPlaceholderText('Write a message…')
    await user.type(input, 'Would you take 60?')
    await user.click(screen.getByRole('button', { name: 'Send' }))

    expect(mutateAsync).toHaveBeenCalledWith('Would you take 60?')
    expect(input).toHaveValue('')
  })

  it('does not send an empty or whitespace-only message', async () => {
    mockSharedHooks()
    vi.mocked(useMessages).mockReturnValue({ isLoading: false, data: [] } as unknown as ReturnType<typeof useMessages>)
    const mutateAsync = vi.fn()
    vi.mocked(useSendMessage).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useSendMessage>)
    const user = userEvent.setup()

    renderAt('/messages/conversation-1')
    await user.type(screen.getByPlaceholderText('Write a message…'), '   ')

    expect(screen.getByRole('button', { name: 'Send' })).toBeDisabled()
    expect(mutateAsync).not.toHaveBeenCalled()
  })
})
