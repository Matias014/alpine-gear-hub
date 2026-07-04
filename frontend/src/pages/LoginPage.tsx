import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AuthLayout } from '../components/AuthLayout'
import { FormField, formInputClasses } from '../components/FormField'
import { useAuth } from '../contexts/AuthContext'
import { buttonPrimary } from '../lib/uiClasses'
import { loginSchema, type LoginFormValues } from '../lib/validation/authSchemas'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  async function onSubmit(values: LoginFormValues) {
    setServerError(null)
    try {
      await login(values)
      const redirectTo = (location.state as { from?: string } | null)?.from ?? '/'
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Login failed')
    }
  }

  return (
    <AuthLayout title="Log in" subtitle="Welcome back">
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

        <FormField label="Password" htmlFor="password" error={errors.password?.message}>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            className={formInputClasses}
            {...register('password')}
          />
        </FormField>

        {serverError && <p className="text-sm text-red-600">{serverError}</p>}

        <button type="submit" disabled={isSubmitting} className={`w-full ${buttonPrimary}`}>
          {isSubmitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-gray-600">
        Don&apos;t have an account?{' '}
        <Link to="/register" className="font-medium text-emerald-700 hover:underline">
          Create one
        </Link>
      </p>
    </AuthLayout>
  )
}
