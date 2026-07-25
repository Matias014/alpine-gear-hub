import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useSearchParams } from 'react-router-dom'
import { AuthLayout } from '../components/AuthLayout'
import { FormField, formInputClasses } from '../components/FormField'
import { authApi } from '../lib/authApi'
import { buttonPrimary } from '../lib/uiClasses'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../lib/validation/authSchemas'

type ConfirmationState = 'confirming' | 'confirmed' | 'failed'

export default function ConfirmEmailPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')

  if (!token) return <ResendConfirmationForm />
  return <ConfirmWithToken token={token} />
}

function ConfirmWithToken({ token }: { token: string }) {
  const [state, setState] = useState<ConfirmationState>('confirming')
  const [error, setError] = useState<string | null>(null)
  // The token is single-use server-side, so this must fire at most once - a plain effect isn't
  // enough since StrictMode's dev-mode double-invoke would burn the token on the first (ignored)
  // call and then show the second call's "already used" failure instead.
  const hasStarted = useRef(false)

  useEffect(() => {
    if (hasStarted.current) return
    hasStarted.current = true

    authApi
      .confirmEmail({ token })
      .then(() => setState('confirmed'))
      .catch((err) => {
        setError(err instanceof Error ? err.message : 'Could not confirm your email')
        setState('failed')
      })
  }, [token])

  if (state === 'confirming') {
    return (
      <AuthLayout title="Confirming your email…" subtitle="Email confirmation">
        <p className="text-sm text-gray-600">One moment…</p>
      </AuthLayout>
    )
  }

  if (state === 'confirmed') {
    return (
      <AuthLayout title="Email confirmed" subtitle="Email confirmation">
        <p className="text-sm text-gray-600">
          Your email is confirmed. You can now publish listings and message other members.
        </p>
        <p className="mt-6 text-center text-sm text-gray-600">
          <Link to="/login" className="font-medium text-emerald-700 hover:underline">
            Log in
          </Link>
        </p>
      </AuthLayout>
    )
  }

  return (
    <AuthLayout title="Couldn't confirm your email" subtitle="Email confirmation">
      <p className="text-sm text-red-600">{error}</p>
      <p className="mt-6 text-center text-sm text-gray-600">
        <Link to="/confirm-email" className="font-medium text-emerald-700 hover:underline">
          Request a new confirmation link
        </Link>
      </p>
    </AuthLayout>
  )
}

function ResendConfirmationForm() {
  const [serverError, setServerError] = useState<string | null>(null)
  const [submitted, setSubmitted] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormValues>({ resolver: zodResolver(forgotPasswordSchema) })

  async function onSubmit(values: ForgotPasswordFormValues) {
    setServerError(null)
    try {
      await authApi.resendEmailConfirmation(values)
      setSubmitted(true)
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Could not send the confirmation email')
    }
  }

  if (submitted) {
    return (
      <AuthLayout title="Check your email" subtitle="Email confirmation">
        <p className="text-sm text-gray-600">
          If that account exists and isn&apos;t confirmed yet, we&apos;ve sent a new confirmation link.
        </p>
        <p className="mt-6 text-center text-sm text-gray-600">
          <Link to="/login" className="font-medium text-emerald-700 hover:underline">
            Back to log in
          </Link>
        </p>
      </AuthLayout>
    )
  }

  return (
    <AuthLayout title="Resend confirmation email" subtitle="Email confirmation">
      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <FormField label="Email" htmlFor="email" error={errors.email?.message}>
          <input
            id="email"
            type="email"
            autoComplete="email"
            className={formInputClasses}
            {...register('email')}
          />
        </FormField>

        {serverError && <p className="text-sm text-red-600">{serverError}</p>}

        <button type="submit" disabled={isSubmitting} className={`w-full ${buttonPrimary}`}>
          {isSubmitting ? 'Sending…' : 'Send confirmation link'}
        </button>
      </form>
    </AuthLayout>
  )
}
