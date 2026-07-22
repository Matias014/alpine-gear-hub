import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import HomePage from './HomePage'
import { useAuth } from '../contexts/AuthContext'
import { useListings } from '../hooks/useListings'
import type { ListingSummaryResponse, PagedResponse } from '../types/listing'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))
vi.mock('../hooks/useListings', () => ({ useListings: vi.fn() }))

function makePage(items: ListingSummaryResponse[]): PagedResponse<ListingSummaryResponse> {
  return { items, page: 1, pageSize: 4, totalCount: items.length, totalPages: 1, hasNextPage: false, hasPreviousPage: false }
}

function makeListing(overrides: Partial<ListingSummaryResponse> = {}): ListingSummaryResponse {
  return {
    id: 'listing-1',
    sellerId: 'seller-1',
    title: 'Petzl GriGri',
    price: 65,
    currency: 'EUR',
    condition: 'Good',
    status: 'Active',
    location: 'Chamonix',
    isPromoted: false,
    primaryImageUrl: null,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

function renderHome() {
  return render(
    <MemoryRouter>
      <HomePage />
    </MemoryRouter>,
  )
}

describe('HomePage', () => {
  it('renders nothing while auth is still loading', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, isLoading: true } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListings).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListings>)

    const { container } = renderHome()
    expect(container).toBeEmptyDOMElement()
  })

  it('shows the guest hero and value props for a logged-out visitor', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, isLoading: false, user: null } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListings).mockReturnValue({ data: makePage([]) } as unknown as ReturnType<typeof useListings>)

    renderHome()
    expect(screen.getByText('Gear you can trust, from climbers who get it.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Start selling' })).toBeInTheDocument()
  })

  it('shows the authenticated hero, quick links, and recent listings for a logged-in user', () => {
    vi.mocked(useAuth).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { id: 'user-1', fullName: 'Jane Climber', role: 'Member' },
    } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListings).mockReturnValue({ data: makePage([makeListing()]) } as unknown as ReturnType<typeof useListings>)

    renderHome()
    expect(screen.getByText('Jane Climber')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Sell gear' })).toBeInTheDocument()
    expect(screen.getByText('Fresh on the marketplace')).toBeInTheDocument()
    expect(screen.getByText('Petzl GriGri')).toBeInTheDocument()
  })

  it('hides the recent-listings section when there are no listings yet', () => {
    vi.mocked(useAuth).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: { id: 'user-1', fullName: 'Jane Climber', role: 'Member' },
    } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(useListings).mockReturnValue({ data: makePage([]) } as unknown as ReturnType<typeof useListings>)

    renderHome()
    expect(screen.queryByText('Fresh on the marketplace')).not.toBeInTheDocument()
  })
})
