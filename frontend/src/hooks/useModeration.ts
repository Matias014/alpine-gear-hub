import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { moderationApi } from '../lib/moderationApi'
import type { ReportReason, ReportResolution, ReportStatus } from '../types/moderation'

export function useCreateReport() {
  return useMutation({
    mutationFn: ({
      listingId,
      reason,
      description,
    }: {
      listingId: string
      reason: ReportReason
      description?: string
    }) => moderationApi.createReport(listingId, reason, description),
  })
}

export function useReports(status: ReportStatus | undefined, page: number) {
  return useQuery({
    queryKey: ['reports', status, page],
    queryFn: () => moderationApi.getReports(status, page),
  })
}

export function useReviewReport() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, resolution }: { id: string; resolution: ReportResolution }) =>
      moderationApi.reviewReport(id, resolution),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reports'] })
      queryClient.invalidateQueries({ queryKey: ['listings'] })
      queryClient.invalidateQueries({ queryKey: ['listing'] })
    },
  })
}
