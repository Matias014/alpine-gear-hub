import type { ReactNode } from 'react'

interface AuthLayoutProps {
  title: string
  subtitle: string
  children: ReactNode
}

export function AuthLayout({ title, subtitle, children }: AuthLayoutProps) {
  return (
    // No min-h-screen here on purpose - this renders inside Layout's <main>, below the sticky
    // header, so min-h-screen was fighting the parent for height and throwing off the centering.
    <div className="flex items-center justify-center py-10 sm:py-16">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold tracking-tight text-gray-900">
            Alpine<span className="text-emerald-600">Gear</span>Hub
          </h1>
          <p className="mt-1 text-sm text-gray-500">{subtitle}</p>
        </div>
        <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-md">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">{title}</h2>
          {children}
        </div>
      </div>
    </div>
  )
}
