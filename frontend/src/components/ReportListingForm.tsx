import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { FormField, formInputClasses } from './FormField'
import { useCreateReport } from '../hooks/useModeration'
import { reasonLabels } from '../lib/moderationLabels'
import { reportSchema, type ReportFormValues } from '../lib/validation/reportSchema'
import type { ReportReason } from '../types/moderation'

const REASONS: ReportReason[] = ['Counterfeit', 'Prohibited', 'Scam', 'SafetyConcern', 'Other']

export function ReportListingForm({ listingId, onDone }: { listingId: string; onDone: () => void }) {
  const createReport = useCreateReport()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ReportFormValues>({ resolver: zodResolver(reportSchema), defaultValues: { reason: 'Other' } })

  async function onSubmit(values: ReportFormValues) {
    setServerError(null)
    try {
      await createReport.mutateAsync({ listingId, reason: values.reason, description: values.description })
      onDone()
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Could not submit the report')
    }
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      className="mt-3 space-y-3 rounded-md border border-gray-200 bg-gray-50 p-3"
    >
      <FormField label="Reason" htmlFor="reason" error={errors.reason?.message}>
        <select id="reason" className={formInputClasses} {...register('reason')}>
          {REASONS.map((reason) => (
            <option key={reason} value={reason}>
              {reasonLabels[reason]}
            </option>
          ))}
        </select>
      </FormField>

      <FormField label="Details (optional)" htmlFor="description" error={errors.description?.message}>
        <textarea id="description" rows={3} className={formInputClasses} {...register('description')} />
      </FormField>

      {serverError && <p className="text-sm text-red-600">{serverError}</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
        >
          {isSubmitting ? 'Submitting…' : 'Submit report'}
        </button>
        <button
          type="button"
          onClick={onDone}
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-100"
        >
          Cancel
        </button>
      </div>
    </form>
  )
}
