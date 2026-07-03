export type PromotionTier = 'Standard' | 'Featured'
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded'

export interface PromotionResponse {
  id: string
  listingId: string
  tier: PromotionTier
  startAt: string
  endAt: string
  price: number
  currency: string
  paymentStatus: PaymentStatus
  stripePaymentIntentId: string | null
  clientSecret: string | null
}
