import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Pagination } from '../components/Pagination'
import { useListing } from '../hooks/useListings'
import { useReports, useReviewReport } from '../hooks/useModeration'
import { reasonLabels, reportStatusStyles } from '../lib/moderationLabels'
import type { ReportResponse, ReportStatus } from '../types/moderation'

const STATUSES: ReportStatus[] = ['Pending', 'Reviewed', 'Dismissed']

export default function ModerationPage() {
  const [status, setStatus] = useState<ReportStatus | undefined>('Pending')
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useReports(status, page)

  function selectStatus(next: ReportStatus | undefined) {
    setStatus(next)
    setPage(1)
  }

  return (
    <div>
      <h1 className="text-xl font-bold text-gray-900">Moderation queue</h1>

      <div className="mt-4 flex flex-wrap gap-2">
        <FilterButton active={status === undefined} onClick={() => selectStatus(undefined)}>
          All
        </FilterButton>
        {STATUSES.map((s) => (
          <FilterButton key={s} active={status === s} onClick={() => selectStatus(s)}>
            {s}
          </FilterButton>
        ))}
      </div>

      {isLoading && <p className="mt-6 text-sm text-gray-500">Loading…</p>}
      {isError && <p className="mt-6 text-sm text-red-600">Couldn&apos;t load reports.</p>}
      {data && data.items.length === 0 && <p className="mt-6 text-sm text-gray-500">No reports here.</p>}

      <div className="mt-4 space-y-3">
        {data?.items.map((report) => (
          <ReportCard key={report.id} report={report} />
        ))}
      </div>

      {data && (
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          hasNextPage={data.hasNextPage}
          hasPreviousPage={data.hasPreviousPage}
          onPageChange={setPage}
        />
      )}
    </div>
  )
}

function FilterButton({
  active,
  onClick,
  children,
}: {
  active: boolean
  onClick: () => void
  children: string
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-md border px-3 py-1.5 text-sm ${
        active ? 'border-gray-900 bg-gray-900 text-white' : 'border-gray-300 text-gray-700 hover:bg-gray-50'
      }`}
    >
      {children}
    </button>
  )
}

function ReportCard({ report }: { report: ReportResponse }) {
  const { data: listing } = useListing(report.listingId)
  const reviewReport = useReviewReport()
  const [actionError, setActionError] = useState<string | null>(null)

  async function handleReview(resolution: 'Dismiss' | 'Remove') {
    setActionError(null)
    try {
      await reviewReport.mutateAsync({ id: report.id, resolution })
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'That action failed')
    }
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4">
      <div className="flex items-start justify-between gap-2">
        <div>
          {listing ? (
            <Link to={`/listings/${listing.id}`} className="font-medium text-gray-900 hover:underline">
              {listing.title}
            </Link>
          ) : (
            <span className="font-medium text-gray-400">Listing unavailable</span>
          )}
          <p className="text-sm text-gray-500">{reasonLabels[report.reason]}</p>
        </div>
        <span className={`shrink-0 rounded px-2 py-1 text-xs font-medium ${reportStatusStyles[report.status]}`}>
          {report.status}
        </span>
      </div>

      {report.description && <p className="mt-2 text-sm text-gray-700">{report.description}</p>}

      <p className="mt-2 text-xs text-gray-400">Reported {new Date(report.createdAt).toLocaleString()}</p>

      {report.status === 'Pending' && (
        <div className="mt-3 flex gap-2">
          <button
            type="button"
            onClick={() => handleReview('Remove')}
            disabled={reviewReport.isPending}
            className="rounded-md bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
          >
            Remove listing
          </button>
          <button
            type="button"
            onClick={() => handleReview('Dismiss')}
            disabled={reviewReport.isPending}
            className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-100 disabled:opacity-50"
          >
            Dismiss
          </button>
        </div>
      )}
      {actionError && <p className="mt-2 text-sm text-red-600">{actionError}</p>}
    </div>
  )
}
