import { describe, expect, it } from 'vitest'
import { conditionLabels, formatPrice, statusStyles } from './listingLabels'
import type { GearCondition, ListingStatus } from '../types/listing'

describe('conditionLabels', () => {
  it('has a human-readable label for every GearCondition value', () => {
    const conditions: GearCondition[] = ['New', 'LikeNew', 'Good', 'Fair', 'Poor']
    for (const condition of conditions) {
      expect(conditionLabels[condition]).toBeTruthy()
    }
  })
})

describe('statusStyles', () => {
  it('has a style class for every ListingStatus value', () => {
    const statuses: ListingStatus[] = ['Draft', 'Active', 'Reserved', 'Sold', 'Expired', 'Removed']
    for (const status of statuses) {
      expect(statusStyles[status]).toMatch(/bg-\S+ text-\S+/)
    }
  })
})

describe('formatPrice', () => {
  it('formats a price using the given currency and the runtime locale', () => {
    const expected = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EUR' }).format(129.5)
    expect(formatPrice(129.5, 'EUR')).toBe(expected)
  })

  it('falls back to "amount currency" for an invalid currency code', () => {
    expect(formatPrice(50, 'NOTREAL')).toBe('50 NOTREAL')
  })
})
