import { api } from './api'
import type { PromotionResponse, PromotionTier } from '../types/promotion'

export const promotionsApi = {
  createPromotion: (listingId: string, tier: PromotionTier) =>
    api.post<PromotionResponse>('/promotions', { listingId, tier }),

  getPromotionsForListing: (listingId: string) =>
    api.get<PromotionResponse[]>(`/promotions/listing/${listingId}`),
}
