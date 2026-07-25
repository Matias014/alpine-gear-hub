import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { AuthLayout } from '../components/AuthLayout'
import { FormField, formInputClasses } from '../components/FormField'
import { authApi } from '../lib/authApi'
import { buttonPrimary } from '../lib/uiClasses'
import { resetPasswordSchema, type ResetPasswordFormValues } from '../lib/validation/authSchemas'

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const navigate = useNavigate()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormValues>({ resolver: zodResolver(resetPasswordSchema) })

  async function onSubmit(values: ResetPasswordFormValues) {
    setServerError(null)
    try {
      await authApi.confirmPasswordReset({ token, newPassword: values.password })
      navigate('/login', { replace: true })
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Could not reset your password')
    }
  }

  if (!token) {
    return (
      <AuthLayout title="Invalid link" subtitle="Password reset">
        <p className="text-sm text-red-600">This password reset link is missing its token.</p>
        <p className="mt-6 text-center text-sm text-gray-600">
          <Link to="/forgot-password" className="font-medium text-emerald-700 hover:underline">
            Request a new link
          </Link>
        </p>
      </AuthLayout>
    )
  }

  return (
    <AuthLayout title="Choose a new password" subtitle="Password reset">
      <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
        <FormField label="New password" htmlFor="password" error={errors.password?.message}>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            className={formInputClasses}
            {...register('password')}
          />
        </FormField>

        <FormField label="Confirm password" htmlFor="confirmPassword" error={errors.confirmPassword?.message}>
          <input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            className={formInputClasses}
            {...register('confirmPassword')}
          />
        </FormField>

        {serverError && <p className="text-sm text-red-600">{serverError}</p>}

        <button type="submit" disabled={isSubmitting} className={`w-full ${buttonPrimary}`}>
          {isSubmitting ? 'Resetting…' : 'Reset password'}
        </button>
      </form>
    </AuthLayout>
  )
}
