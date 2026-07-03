import { useState } from 'react'
import { Elements, PaymentElement, useElements, useStripe } from '@stripe/react-stripe-js'
import { useCreatePromotion, usePromotionsForListing } from '../hooks/usePromotions'
import { stripePromise } from '../lib/stripe'
import type { PromotionTier } from '../types/promotion'

// Prices/durations mirror PromotionPricing on the backend exactly - if that ever changes, this
// needs to change with it (nothing enforces they stay in sync).
const TIERS: { value: PromotionTier; label: string; description: string }[] = [
  { value: 'Standard', label: 'Standard — €5 / 7 days', description: 'Boosts your listing above unpromoted ones.' },
  { value: 'Featured', label: 'Featured — €15 / 14 days', description: 'Longer boost with a "Featured" badge.' },
]

export function PromotionCheckout({ listingId }: { listingId: string }) {
  const { data: promotions } = usePromotionsForListing(listingId)
  const createPromotion = useCreatePromotion(listingId)
  const [tier, setTier] = useState<PromotionTier>('Standard')
  const [clientSecret, setClientSecret] = useState<string | null>(null)
  const [createError, setCreateError] = useState<string | null>(null)

  const activePromotion = promotions?.find(
    (p) => (p.paymentStatus === 'Pending' || p.paymentStatus === 'Completed') && new Date(p.endAt) > new Date(),
  )

  async function handleStartCheckout() {
    setCreateError(null)
    try {
      const promotion = await createPromotion.mutateAsync(tier)
      if (!promotion.clientSecret) throw new Error('Payment could not be initiated')
      setClientSecret(promotion.clientSecret)
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : 'Could not start checkout')
    }
  }

  if (activePromotion) {
    return (
      <p className="text-sm text-gray-600">
        This listing is promoted ({activePromotion.tier}) until{' '}
        {new Date(activePromotion.endAt).toLocaleDateString()}.
      </p>
    )
  }

  if (clientSecret) {
    return (
      <Elements stripe={stripePromise} options={{ clientSecret }}>
        <PaymentForm />
      </Elements>
    )
  }

  return (
    <div className="space-y-3">
      {TIERS.map((option) => (
        <label
          key={option.value}
          className={`block cursor-pointer rounded-md border p-3 text-sm ${
            tier === option.value ? 'border-emerald-600 bg-emerald-50' : 'border-gray-300'
          }`}
        >
          <input
            type="radio"
            name="tier"
            value={option.value}
            checked={tier === option.value}
            onChange={() => setTier(option.value)}
            className="mr-2"
          />
          <span className="font-medium text-gray-900">{option.label}</span>
          <p className="ml-5 text-xs text-gray-500">{option.description}</p>
        </label>
      ))}

      {createError && <p className="text-sm text-red-600">{createError}</p>}

      <button
        type="button"
        onClick={handleStartCheckout}
        disabled={createPromotion.isPending}
        className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:opacity-50"
      >
        {createPromotion.isPending ? 'Starting checkout…' : 'Promote this listing'}
      </button>
    </div>
  )
}

function PaymentForm() {
  const stripe = useStripe()
  const elements = useElements()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleConfirm() {
    if (!stripe || !elements) return

    setIsSubmitting(true)
    setError(null)

    const { error: confirmError } = await stripe.confirmPayment({
      elements,
      confirmParams: { return_url: window.location.href },
    })

    // Only reached on failure - a successful confirmation redirects to return_url instead.
    if (confirmError) {
      setError(confirmError.message ?? 'Payment failed')
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-3">
      <PaymentElement />
      {error && <p className="text-sm text-red-600">{error}</p>}
      <button
        type="button"
        onClick={handleConfirm}
        disabled={!stripe || isSubmitting}
        className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:opacity-50"
      >
        {isSubmitting ? 'Processing…' : 'Pay now'}
      </button>
    </div>
  )
}
