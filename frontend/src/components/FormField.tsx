import type { ReactNode } from 'react'

interface FormFieldProps {
  label: string
  htmlFor: string
  error?: string
  children: ReactNode
}

export const formInputClasses =
  'block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm transition-colors ' +
  'focus:border-emerald-600 focus:outline-none focus:ring-1 focus:ring-emerald-600'

export function FormField({ label, htmlFor, error, children }: FormFieldProps) {
  return (
    <div>
      <label htmlFor={htmlFor} className="mb-1 block text-sm font-medium text-gray-700">
        {label}
      </label>
      {children}
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  )
}
