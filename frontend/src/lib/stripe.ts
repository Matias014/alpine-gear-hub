import { loadStripe } from '@stripe/stripe-js'

// Same "placeholder" story as the backend's Stripe:SecretKey - this needs a real publishable key
// from https://dashboard.stripe.com/test/apikeys (paired with the backend's real secret key)
// before checkout can actually load. Without one, loadStripe() itself fails, not just payment.
const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY ?? 'pk_test_placeholder'

export const stripePromise = loadStripe(publishableKey)
