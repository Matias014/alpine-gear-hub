import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import ConversationsPage from './ConversationsPage'
import { useConversations } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import type { ConversationSummaryResponse } from '../types/chat'
import type { ListingResponse } from '../types/listing'

vi.mock('../hooks/useChat', () => ({ useConversations: vi.fn() }))
vi.mock('../hooks/useListings', () => ({ useListing: vi.fn() }))

function makeConversation(overrides: Partial<ConversationSummaryResponse> = {}): ConversationSummaryResponse {
  return {
    id: 'conversation-1',
    listingId: 'listing-1',
    otherParticipantId: 'seller-1',
    lastMessageBody: 'Is this still available?',
    lastMessageAt: '2026-01-01T00:00:00Z',
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

function renderPage() {
  return render(
    <MemoryRouter>
      <ConversationsPage />
    </MemoryRouter>,
  )
}

describe('ConversationsPage', () => {
  it('shows a loading message while conversations load', () => {
    vi.mocked(useConversations).mockReturnValue({ isLoading: true, isError: false, data: undefined } as unknown as ReturnType<typeof useConversations>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText('Loading conversations…')).toBeInTheDocument()
  })

  it('shows an error message when conversations fail to load', () => {
    vi.mocked(useConversations).mockReturnValue({ isLoading: false, isError: true, data: undefined } as unknown as ReturnType<typeof useConversations>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText("Couldn't load your conversations.")).toBeInTheDocument()
  })

  it('shows an empty state when there are no conversations yet', () => {
    vi.mocked(useConversations).mockReturnValue({ isLoading: false, isError: false, data: [] } as unknown as ReturnType<typeof useConversations>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText('No conversations yet. Message a seller from a listing to start one.')).toBeInTheDocument()
  })

  it('lists conversations with the listing title, last message, and unread badge', () => {
    vi.mocked(useConversations).mockReturnValue({
      isLoading: false,
      isError: false,
      data: [makeConversation({ unreadCount: 3 })],
    } as unknown as ReturnType<typeof useConversations>)
    vi.mocked(useListing).mockReturnValue({ data: makeListing() } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByRole('link')).toHaveAttribute('href', '/messages/conversation-1')
    expect(screen.getByText('Petzl GriGri')).toBeInTheDocument()
    expect(screen.getByText('Is this still available?')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })
})
