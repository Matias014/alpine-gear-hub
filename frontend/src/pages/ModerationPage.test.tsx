import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import ModerationPage from './ModerationPage'
import { useReports, useReviewReport } from '../hooks/useModeration'
import { useListing } from '../hooks/useListings'
import type { ReportResponse } from '../types/moderation'
import type { ListingResponse } from '../types/listing'

vi.mock('../hooks/useModeration', () => ({ useReports: vi.fn(), useReviewReport: vi.fn() }))
vi.mock('../hooks/useListings', () => ({ useListing: vi.fn() }))

function makeReportsPage(items: ReportResponse[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, hasNextPage: false, hasPreviousPage: false }
}

function makeReport(overrides: Partial<ReportResponse> = {}): ReportResponse {
  return {
    id: 'report-1',
    listingId: 'listing-1',
    reportedByUserId: 'buyer-1',
    reason: 'Counterfeit',
    description: 'Looks like a fake.',
    status: 'Pending',
    reviewedByUserId: null,
    reviewedAt: null,
    createdAt: '2026-01-01T00:00:00Z',
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
      <ModerationPage />
    </MemoryRouter>,
  )
}

describe('ModerationPage', () => {
  it('shows a loading message while reports load', () => {
    vi.mocked(useReports).mockReturnValue({ isLoading: true, isError: false, data: undefined } as unknown as ReturnType<typeof useReports>)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error message when reports fail to load', () => {
    vi.mocked(useReports).mockReturnValue({ isLoading: false, isError: true, data: undefined } as unknown as ReturnType<typeof useReports>)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText("Couldn't load reports.")).toBeInTheDocument()
  })

  it('shows an empty state when there are no reports for the selected filter', () => {
    vi.mocked(useReports).mockReturnValue({ isLoading: false, isError: false, data: makeReportsPage([]) } as unknown as ReturnType<typeof useReports>)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: undefined } as unknown as ReturnType<typeof useListing>)

    renderPage()
    expect(screen.getByText('No reports here.')).toBeInTheDocument()
  })

  it('renders a report with its listing title and reason, and switches the status filter', async () => {
    vi.mocked(useReports).mockReturnValue({
      isLoading: false,
      isError: false,
      data: makeReportsPage([makeReport()]),
    } as unknown as ReturnType<typeof useReports>)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: makeListing() } as unknown as ReturnType<typeof useListing>)
    const user = userEvent.setup()

    renderPage()
    expect(screen.getByRole('link', { name: 'Petzl GriGri' })).toBeInTheDocument()
    expect(screen.getByText('Counterfeit item')).toBeInTheDocument()
    expect(useReports).toHaveBeenLastCalledWith('Pending', 1)

    await user.click(screen.getByRole('button', { name: 'Reviewed' }))
    expect(useReports).toHaveBeenLastCalledWith('Reviewed', 1)
  })

  it('reviews a pending report by removing the listing', async () => {
    vi.mocked(useReports).mockReturnValue({
      isLoading: false,
      isError: false,
      data: makeReportsPage([makeReport()]),
    } as unknown as ReturnType<typeof useReports>)
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: makeListing() } as unknown as ReturnType<typeof useListing>)
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Remove listing' }))

    expect(mutateAsync).toHaveBeenCalledWith({ id: 'report-1', resolution: 'Remove' })
  })

  it('reviews a pending report by dismissing it', async () => {
    vi.mocked(useReports).mockReturnValue({
      isLoading: false,
      isError: false,
      data: makeReportsPage([makeReport()]),
    } as unknown as ReturnType<typeof useReports>)
    const mutateAsync = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useReviewReport).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useReviewReport>)
    vi.mocked(useListing).mockReturnValue({ data: makeListing() } as unknown as ReturnType<typeof useListing>)
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Dismiss' }))

    expect(mutateAsync).toHaveBeenCalledWith({ id: 'report-1', resolution: 'Dismiss' })
  })
})
