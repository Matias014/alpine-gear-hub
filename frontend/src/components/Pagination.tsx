interface PaginationProps {
  page: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
  onPageChange: (page: number) => void
}

export function Pagination({ page, totalPages, hasNextPage, hasPreviousPage, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <div className="mt-6 flex items-center justify-center gap-4">
      <button
        type="button"
        disabled={!hasPreviousPage}
        onClick={() => onPageChange(page - 1)}
        className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 disabled:opacity-40 disabled:hover:bg-transparent"
      >
        Previous
      </button>
      <span className="text-sm text-gray-600">
        Page {page} of {totalPages}
      </span>
      <button
        type="button"
        disabled={!hasNextPage}
        onClick={() => onPageChange(page + 1)}
        className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 disabled:opacity-40 disabled:hover:bg-transparent"
      >
        Next
      </button>
    </div>
  )
}
