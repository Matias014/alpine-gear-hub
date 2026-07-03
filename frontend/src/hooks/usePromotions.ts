import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { promotionsApi } from '../lib/promotionsApi'
import type { PromotionTier } from '../types/promotion'

export function usePromotionsForListing(listingId: string) {
  return useQuery({
    queryKey: ['promotions', listingId],
    queryFn: () => promotionsApi.getPromotionsForListing(listingId),
  })
}

export function useCreatePromotion(listingId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (tier: PromotionTier) => promotionsApi.createPromotion(listingId, tier),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['promotions', listingId] }),
  })
}
