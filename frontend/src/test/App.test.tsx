import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import App from '../App'
import { AuthProvider } from '../contexts/AuthContext'

function renderApp() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('App', () => {
  it('renders without crashing', () => {
    renderApp()
    // HomePage's own heading is now marketing copy, not the "AlpineGearHub" wordmark - that
    // only lives in the persistent nav bar now, as a link rather than a heading.
    expect(screen.getByRole('link', { name: 'AlpineGearHub' })).toBeInTheDocument()
  })
})
