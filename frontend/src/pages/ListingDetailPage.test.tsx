import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import ListingDetailPage from './ListingDetailPage'
import { useAuth } from '../contexts/AuthContext'
import { useStartConversation } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import type { ListingResponse } from '../types/listing'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))
vi.mock('../hooks/useChat', () => ({ useStartConversation: vi.fn() }))
vi.mock('../hooks/useListings', () => ({ useListing: vi.fn() }))

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

function renderAt(path: string) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/listings/:id" element={<ListingDetailPage />} />
          <Route path="/messages/:id" element={<p>conversation page</p>} />
          <Route path="/login" element={<p>login page</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('ListingDetailPage', () => {
  it('shows a loading message while the listing loads', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, user: null } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: true, isError: false, data: undefined } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useStartConversation>)

    renderAt('/listings/listing-1')
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it("shows a not-found message when the listing can't be loaded", () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, user: null } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: false, isError: true, data: undefined } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useStartConversation>)

    renderAt('/listings/listing-1')
    expect(screen.getByText("This listing couldn't be found.")).toBeInTheDocument()
  })

  it('shows a "manage it" link for the listing owner instead of a message button', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true, user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: false, isError: false, data: makeListing() } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useStartConversation>)

    renderAt('/listings/listing-1')
    expect(screen.getByText('This is your listing.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Manage it' })).toHaveAttribute('href', '/listings/listing-1/edit')
    expect(screen.queryByRole('button', { name: 'Message seller' })).not.toBeInTheDocument()
  })

  it('starts a conversation and navigates to it when a buyer messages the seller', async () => {
    const mutateAsync = vi.fn().mockResolvedValue({ id: 'conversation-1' })
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true, user: { id: 'buyer-1' } } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: false, isError: false, data: makeListing() } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useStartConversation>)
    const user = userEvent.setup()

    renderAt('/listings/listing-1')
    await user.click(screen.getByRole('button', { name: 'Message seller' }))

    expect(mutateAsync).toHaveBeenCalledWith('listing-1')
    expect(await screen.findByText('conversation page')).toBeInTheDocument()
  })

  it('prompts a logged-out visitor to log in instead of showing a message button', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, user: null } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: false, isError: false, data: makeListing() } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useStartConversation>)

    renderAt('/listings/listing-1')
    expect(screen.getByRole('link', { name: 'Log in to message the seller' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Message seller' })).not.toBeInTheDocument()
  })

  it('lets a buyer reveal the report form', async () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true, user: { id: 'buyer-1' } } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListing).mockReturnValue({ isLoading: false, isError: false, data: makeListing() } as unknown as ReturnType<typeof useListing>)
    vi.mocked(useStartConversation).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useStartConversation>)
    const user = userEvent.setup()

    renderAt('/listings/listing-1')
    await user.click(screen.getByRole('button', { name: 'Report this listing' }))

    expect(screen.getByRole('button', { name: 'Submit report' })).toBeInTheDocument()
  })
})
