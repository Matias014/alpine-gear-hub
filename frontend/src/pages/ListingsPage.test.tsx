import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import ListingsPage from './ListingsPage'
import { useCategories, useListings } from '../hooks/useListings'
import type { ListingSummaryResponse, PagedResponse } from '../types/listing'

vi.mock('../hooks/useListings', () => ({ useCategories: vi.fn(), useListings: vi.fn() }))

function makePage(items: ListingSummaryResponse[]): PagedResponse<ListingSummaryResponse> {
  return { items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, hasNextPage: false, hasPreviousPage: false }
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

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <ListingsPage />
    </MemoryRouter>,
  )
}

describe('ListingsPage', () => {
  it('shows a loading message while listings are loading', () => {
    vi.mocked(useCategories).mockReturnValue({ data: [] } as unknown as ReturnType<typeof useCategories>)
    vi.mocked(useListings).mockReturnValue({ isLoading: true, isError: false, data: undefined } as unknown as ReturnType<typeof useListings>)

    renderAt('/listings')
    expect(screen.getByText('Loading listings…')).toBeInTheDocument()
  })

  it('shows an error message when listings fail to load', () => {
    vi.mocked(useCategories).mockReturnValue({ data: [] } as unknown as ReturnType<typeof useCategories>)
    vi.mocked(useListings).mockReturnValue({ isLoading: false, isError: true, data: undefined } as unknown as ReturnType<typeof useListings>)

    renderAt('/listings')
    expect(screen.getByText("Couldn't load listings. Try again shortly.")).toBeInTheDocument()
  })

  it('shows an empty state when no listings match the filters', () => {
    vi.mocked(useCategories).mockReturnValue({ data: [] } as unknown as ReturnType<typeof useCategories>)
    vi.mocked(useListings).mockReturnValue({ isLoading: false, isError: false, data: makePage([]) } as unknown as ReturnType<typeof useListings>)

    renderAt('/listings')
    expect(screen.getByText('No listings match those filters.')).toBeInTheDocument()
  })

  it('renders listing cards and category/condition filters when browsing all gear', () => {
    vi.mocked(useCategories).mockReturnValue({
      data: [{ id: 'cat-1', name: 'Ropes', slug: 'ropes' }],
    } as unknown as ReturnType<typeof useCategories>)
    vi.mocked(useListings).mockReturnValue({
      isLoading: false,
      isError: false,
      data: makePage([makeListing()]),
    } as unknown as ReturnType<typeof useListings>)

    renderAt('/listings')
    expect(screen.getByRole('heading', { name: 'Browse gear' })).toBeInTheDocument()
    expect(screen.getByText('Petzl GriGri')).toBeInTheDocument()
    expect(screen.getByText('Ropes')).toBeInTheDocument()
    expect(screen.getByText('All categories')).toBeInTheDocument()
  })

  it('shows "My listings" and hides category/condition filters when scoped to a seller', () => {
    vi.mocked(useCategories).mockReturnValue({ data: [] } as unknown as ReturnType<typeof useCategories>)
    vi.mocked(useListings).mockReturnValue({
      isLoading: false,
      isError: false,
      data: makePage([makeListing()]),
    } as unknown as ReturnType<typeof useListings>)

    renderAt('/listings?sellerId=seller-1')
    expect(screen.getByRole('heading', { name: 'My listings' })).toBeInTheDocument()
    expect(screen.queryByText('All categories')).not.toBeInTheDocument()
  })
})
