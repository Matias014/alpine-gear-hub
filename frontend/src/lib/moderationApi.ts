import { api } from './api'
import type {
  ModerationPagedResponse,
  ReportReason,
  ReportResolution,
  ReportResponse,
  ReportStatus,
} from '../types/moderation'

export const moderationApi = {
  createReport: (listingId: string, reason: ReportReason, description?: string) =>
    api.post<ReportResponse>('/moderation/reports', { listingId, reason, description }),

  getReports: (status: ReportStatus | undefined, page: number, pageSize = 20) => {
    const params = new URLSearchParams()
    if (status) params.set('status', status)
    params.set('page', String(page))
    params.set('pageSize', String(pageSize))
    return api.get<ModerationPagedResponse<ReportResponse>>(`/moderation/reports?${params.toString()}`)
  },

  reviewReport: (id: string, resolution: ReportResolution) =>
    api.post<ReportResponse>(`/moderation/reports/${id}/review`, { resolution }),
}
