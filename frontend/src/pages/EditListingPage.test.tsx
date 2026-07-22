import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import EditListingPage from './EditListingPage'
import { useAuth } from '../contexts/AuthContext'
import {
  useChangeListingStatus,
  useDeleteListingImage,
  useListing,
  usePublishListing,
  useUpdateListing,
  useUploadListingImage,
} from '../hooks/useListings'
import type { ListingResponse } from '../types/listing'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))
vi.mock('../hooks/useListings', () => ({
  useListing: vi.fn(),
  useUpdateListing: vi.fn(),
  usePublishListing: vi.fn(),
  useChangeListingStatus: vi.fn(),
  useUploadListingImage: vi.fn(),
  useDeleteListingImage: vi.fn(),
}))
vi.mock('../components/PromotionCheckout', () => ({ PromotionCheckout: () => <div>promotion-checkout-stub</div> }))

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
    status: 'Draft',
    location: 'Chamonix',
    isPromoted: false,
    createdAt: '2026-01-01T00:00:00Z',
    expiresAt: null,
    images: [],
    ...overrides,
  }
}

function noopMutation() {
  return { mutateAsync: vi.fn(), isPending: false }
}

function mockHooks(listing: ListingResponse | undefined, isLoading = false) {
  vi.mocked(useListing).mockReturnValue({ data: listing, isLoading } as unknown as ReturnType<typeof useListing>)
  vi.mocked(useUpdateListing).mockReturnValue(noopMutation() as unknown as ReturnType<typeof useUpdateListing>)
  vi.mocked(usePublishListing).mockReturnValue(noopMutation() as unknown as ReturnType<typeof usePublishListing>)
  vi.mocked(useChangeListingStatus).mockReturnValue(noopMutation() as unknown as ReturnType<typeof useChangeListingStatus>)
  vi.mocked(useUploadListingImage).mockReturnValue(noopMutation() as unknown as ReturnType<typeof useUploadListingImage>)
  vi.mocked(useDeleteListingImage).mockReturnValue(noopMutation() as unknown as ReturnType<typeof useDeleteListingImage>)
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/listings/:id/edit" element={<EditListingPage />} />
        <Route path="/listings" element={<p>listings page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

afterEach(() => {
  vi.restoreAllMocks()
})

describe('EditListingPage', () => {
  it('shows a loading message while the listing loads', () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(undefined, true)

    renderAt('/listings/listing-1/edit')
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it("shows a not-found message when the listing can't be loaded", () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(undefined, false)

    renderAt('/listings/listing-1/edit')
    expect(screen.getByText("This listing couldn't be found.")).toBeInTheDocument()
  })

  it('forbids anyone other than the seller from managing the listing', () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'someone-else' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing())

    renderAt('/listings/listing-1/edit')
    expect(screen.getByText('Only the seller can manage this listing.')).toBeInTheDocument()
  })

  it('publishes a draft listing', async () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing({ status: 'Draft' }))
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(usePublishListing).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof usePublishListing>)
    const user = userEvent.setup()

    renderAt('/listings/listing-1/edit')
    await user.click(screen.getByRole('button', { name: 'Publish' }))

    expect(mutateAsync).toHaveBeenCalled()
  })

  it('shows status actions and the promotion checkout for an active listing', () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing({ status: 'Active' }))

    renderAt('/listings/listing-1/edit')
    expect(screen.getByRole('button', { name: 'Mark as reserved' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Mark as sold' })).toBeInTheDocument()
    expect(screen.getByText('promotion-checkout-stub')).toBeInTheDocument()
  })

  it('removes the listing after confirming, then navigates back to the listings page', async () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing({ status: 'Active' }))
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useChangeListingStatus).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useChangeListingStatus>)
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    const user = userEvent.setup()

    renderAt('/listings/listing-1/edit')
    await user.click(screen.getByRole('button', { name: 'Remove listing' }))

    expect(mutateAsync).toHaveBeenCalledWith('Remove')
    expect(await screen.findByText('listings page')).toBeInTheDocument()
  })

  it('does not remove the listing when the confirmation is cancelled', async () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing({ status: 'Active' }))
    const mutateAsync = vi.fn()
    vi.mocked(useChangeListingStatus).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useChangeListingStatus>)
    vi.spyOn(window, 'confirm').mockReturnValue(false)
    const user = userEvent.setup()

    renderAt('/listings/listing-1/edit')
    await user.click(screen.getByRole('button', { name: 'Remove listing' }))

    expect(mutateAsync).not.toHaveBeenCalled()
  })

  it('saves detail changes once the form becomes dirty', async () => {
    vi.mocked(useAuth).mockReturnValue({ user: { id: 'seller-1' } } as unknown as ReturnType<typeof useAuth>)
    mockHooks(makeListing({ status: 'Draft' }))
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useUpdateListing).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useUpdateListing>)
    const user = userEvent.setup()

    renderAt('/listings/listing-1/edit')
    const saveButton = screen.getByRole('button', { name: 'Save changes' })
    expect(saveButton).toBeDisabled()

    await user.clear(screen.getByLabelText('Title'))
    await user.type(screen.getByLabelText('Title'), 'Petzl GriGri (like new)')
    expect(saveButton).toBeEnabled()

    await user.click(saveButton)
    expect(mutateAsync).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'Petzl GriGri (like new)' }),
    )
  })
})
