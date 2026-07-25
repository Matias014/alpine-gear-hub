import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { AuthLayout } from '../components/AuthLayout'
import { FormField, formInputClasses } from '../components/FormField'
import { authApi } from '../lib/authApi'
import { buttonPrimary } from '../lib/uiClasses'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../lib/validation/authSchemas'

export default function ForgotPasswordPage() {
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
      await authApi.requestPasswordReset(values)
      setSubmitted(true)
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Could not send the reset email')
    }
  }

  if (submitted) {
    return (
      <AuthLayout title="Check your email" subtitle="Password reset">
        <p className="text-sm text-gray-600">
          If an account exists for that email, we&apos;ve sent a link to reset your password.
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
    <AuthLayout title="Forgot your password?" subtitle="Password reset">
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
          {isSubmitting ? 'Sending…' : 'Send reset link'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-gray-600">
        Remembered it?{' '}
        <Link to="/login" className="font-medium text-emerald-700 hover:underline">
          Log in
        </Link>
      </p>
    </AuthLayout>
  )
}
