export type ReportReason = 'Counterfeit' | 'Prohibited' | 'Scam' | 'SafetyConcern' | 'Other'
export type ReportStatus = 'Pending' | 'Reviewed' | 'Dismissed'
export type ReportResolution = 'Dismiss' | 'Remove'

export interface ReportResponse {
  id: string
  listingId: string
  reportedByUserId: string
  reason: ReportReason
  description: string | null
  status: ReportStatus
  reviewedByUserId: string | null
  reviewedAt: string | null
  createdAt: string
}

export interface ModerationPagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
