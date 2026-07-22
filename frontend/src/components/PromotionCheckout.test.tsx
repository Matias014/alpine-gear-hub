import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { PromotionCheckout } from './PromotionCheckout'
import { useCreatePromotion, usePromotionsForListing } from '../hooks/usePromotions'
import type { PromotionResponse } from '../types/promotion'

vi.mock('../hooks/usePromotions', () => ({ usePromotionsForListing: vi.fn(), useCreatePromotion: vi.fn() }))
vi.mock('../lib/stripe', () => ({ stripePromise: Promise.resolve(null) }))

const confirmPayment = vi.fn()
vi.mock('@stripe/react-stripe-js', () => ({
  Elements: ({ children }: { children: ReactNode }) => <div data-testid="stripe-elements">{children}</div>,
  PaymentElement: () => <div data-testid="payment-element" />,
  useStripe: () => ({ confirmPayment }),
  useElements: () => ({}),
}))

function makePromotion(overrides: Partial<PromotionResponse> = {}): PromotionResponse {
  return {
    id: 'promotion-1',
    listingId: 'listing-1',
    tier: 'Standard',
    startAt: '2026-01-01T00:00:00Z',
    endAt: '2026-01-08T00:00:00Z',
    price: 5,
    currency: 'EUR',
    paymentStatus: 'Completed',
    stripePaymentIntentId: 'pi_123',
    clientSecret: null,
    ...overrides,
  }
}

function renderCheckout() {
  return render(<PromotionCheckout listingId="listing-1" />)
}

describe('PromotionCheckout', () => {
  it('shows the active promotion instead of the checkout form when one is already running', () => {
    const farFuture = new Date(Date.now() + 1000 * 60 * 60 * 24 * 5).toISOString()
    vi.mocked(usePromotionsForListing).mockReturnValue({
      data: [makePromotion({ tier: 'Featured', endAt: farFuture })],
    } as unknown as ReturnType<typeof usePromotionsForListing>)
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)

    renderCheckout()
    expect(screen.getByText(/This listing is promoted \(Featured\) until/)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Promote this listing' })).not.toBeInTheDocument()
  })

  it('ignores an expired or failed promotion and shows the checkout form', () => {
    const past = new Date(Date.now() - 1000 * 60 * 60).toISOString()
    vi.mocked(usePromotionsForListing).mockReturnValue({
      data: [makePromotion({ endAt: past }), makePromotion({ id: 'promotion-2', paymentStatus: 'Failed' })],
    } as unknown as ReturnType<typeof usePromotionsForListing>)
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync: vi.fn(), isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)

    renderCheckout()
    expect(screen.getByRole('button', { name: 'Promote this listing' })).toBeInTheDocument()
  })

  it('lets the seller pick a tier and starts checkout', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Pending', clientSecret: null }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('radio', { name: /Featured/ }))
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))

    expect(mutateAsync).toHaveBeenCalledWith('Featured')
  })

  it('defaults to the Standard tier when starting checkout', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Pending', clientSecret: null }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))

    expect(mutateAsync).toHaveBeenCalledWith('Standard')
  })

  it('stays on the tier picker when no real Stripe key is configured (dev fallback)', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Completed', clientSecret: null }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))

    expect(screen.queryByTestId('stripe-elements')).not.toBeInTheDocument()
  })

  it('renders the Stripe payment form once a client secret comes back', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Pending', clientSecret: 'secret_123' }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))

    expect(await screen.findByTestId('stripe-elements')).toBeInTheDocument()
    expect(screen.getByTestId('payment-element')).toBeInTheDocument()
  })

  it('shows an error message when starting checkout fails', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockRejectedValue(new Error('You already have an active promotion'))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))

    expect(await screen.findByText('You already have an active promotion')).toBeInTheDocument()
  })

  it('confirms the payment when the seller pays', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Pending', clientSecret: 'secret_123' }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    confirmPayment.mockResolvedValue({ error: undefined })
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))
    await screen.findByTestId('stripe-elements')
    await user.click(screen.getByRole('button', { name: 'Pay now' }))

    expect(confirmPayment).toHaveBeenCalledWith(
      expect.objectContaining({ confirmParams: { return_url: window.location.href } }),
    )
  })

  it('shows an error message when payment confirmation fails', async () => {
    vi.mocked(usePromotionsForListing).mockReturnValue({ data: [] } as unknown as ReturnType<typeof usePromotionsForListing>)
    const mutateAsync = vi.fn().mockResolvedValue(makePromotion({ paymentStatus: 'Pending', clientSecret: 'secret_123' }))
    vi.mocked(useCreatePromotion).mockReturnValue({ mutateAsync, isPending: false } as unknown as ReturnType<typeof useCreatePromotion>)
    confirmPayment.mockResolvedValue({ error: { message: 'Your card was declined.' } })
    const user = userEvent.setup()

    renderCheckout()
    await user.click(screen.getByRole('button', { name: 'Promote this listing' }))
    await screen.findByTestId('stripe-elements')
    await user.click(screen.getByRole('button', { name: 'Pay now' }))

    expect(await screen.findByText('Your card was declined.')).toBeInTheDocument()
  })
})
