import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { ListingCard } from './ListingCard'
import type { ListingSummaryResponse } from '../types/listing'

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

function renderCard(listing: ListingSummaryResponse) {
  return render(
    <MemoryRouter>
      <ListingCard listing={listing} />
    </MemoryRouter>,
  )
}

describe('ListingCard', () => {
  it('links to the listing detail page', () => {
    renderCard(makeListing())
    expect(screen.getByRole('link')).toHaveAttribute('href', '/listings/listing-1')
  })

  it('shows title, price, condition, and location', () => {
    renderCard(makeListing())
    expect(screen.getByText('Petzl GriGri')).toBeInTheDocument()
    expect(screen.getByText('Good')).toBeInTheDocument()
    expect(screen.getByText('Chamonix')).toBeInTheDocument()
  })

  it('shows a "No photo" placeholder when there is no image', () => {
    renderCard(makeListing({ primaryImageUrl: null }))
    expect(screen.getByText('No photo')).toBeInTheDocument()
  })

  it('renders the image when one is provided', () => {
    renderCard(makeListing({ primaryImageUrl: 'https://cdn.example.com/photo.jpg' }))
    expect(screen.getByRole('img', { name: 'Petzl GriGri' })).toHaveAttribute(
      'src',
      'https://cdn.example.com/photo.jpg',
    )
  })

  it('shows a Featured badge only when promoted', () => {
    renderCard(makeListing({ isPromoted: true }))
    expect(screen.getByText('Featured')).toBeInTheDocument()
  })

  it('shows a status badge for non-Active listings but not for Active ones', () => {
    const { rerender } = renderCard(makeListing({ status: 'Active' }))
    expect(screen.queryByText('Active')).not.toBeInTheDocument()

    rerender(
      <MemoryRouter>
        <ListingCard listing={makeListing({ status: 'Sold' })} />
      </MemoryRouter>,
    )
    expect(screen.getByText('Sold')).toBeInTheDocument()
  })
})
