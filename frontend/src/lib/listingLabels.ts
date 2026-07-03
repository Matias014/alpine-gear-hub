import type { GearCondition, ListingStatus } from '../types/listing'

export const conditionLabels: Record<GearCondition, string> = {
  New: 'New',
  LikeNew: 'Like New',
  Good: 'Good',
  Fair: 'Fair',
  Poor: 'Poor',
}

export const statusStyles: Record<ListingStatus, string> = {
  Draft: 'bg-gray-100 text-gray-700',
  Active: 'bg-emerald-100 text-emerald-700',
  Reserved: 'bg-amber-100 text-amber-700',
  Sold: 'bg-blue-100 text-blue-700',
  Expired: 'bg-orange-100 text-orange-700',
  Removed: 'bg-red-100 text-red-700',
}

export function formatPrice(price: number, currency: string): string {
  try {
    return new Intl.NumberFormat(undefined, { style: 'currency', currency }).format(price)
  } catch {
    return `${price} ${currency}`
  }
}
