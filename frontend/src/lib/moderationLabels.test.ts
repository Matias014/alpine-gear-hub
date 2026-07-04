import { describe, expect, it } from 'vitest'
import { reasonLabels, reportStatusStyles } from './moderationLabels'
import type { ReportReason, ReportStatus } from '../types/moderation'

describe('reasonLabels', () => {
  it('has a human-readable label for every ReportReason value', () => {
    const reasons: ReportReason[] = ['Counterfeit', 'Prohibited', 'Scam', 'SafetyConcern', 'Other']
    for (const reason of reasons) {
      expect(reasonLabels[reason]).toBeTruthy()
    }
  })
})

describe('reportStatusStyles', () => {
  it('has a style class for every ReportStatus value', () => {
    const statuses: ReportStatus[] = ['Pending', 'Reviewed', 'Dismissed']
    for (const status of statuses) {
      expect(reportStatusStyles[status]).toMatch(/bg-\S+ text-\S+/)
    }
  })
})
