import { z } from 'zod'

// Mirrors CreateReportCommandValidator on the backend (description length only - reason binds
// straight to the enum, and ListingId/ReportedByUserId are filled in server-side).
export const reportSchema = z.object({
  reason: z.enum(['Counterfeit', 'Prohibited', 'Scam', 'SafetyConcern', 'Other']),
  description: z.string().max(1000, 'Description must be 1000 characters or fewer').optional(),
})

export type ReportFormValues = z.infer<typeof reportSchema>
