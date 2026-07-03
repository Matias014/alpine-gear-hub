import type { ReportReason, ReportStatus } from '../types/moderation'

export const reasonLabels: Record<ReportReason, string> = {
  Counterfeit: 'Counterfeit item',
  Prohibited: 'Prohibited item',
  Scam: 'Looks like a scam',
  SafetyConcern: 'Safety concern',
  Other: 'Other',
}

export const reportStatusStyles: Record<ReportStatus, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  Reviewed: 'bg-emerald-100 text-emerald-700',
  Dismissed: 'bg-gray-100 text-gray-700',
}
