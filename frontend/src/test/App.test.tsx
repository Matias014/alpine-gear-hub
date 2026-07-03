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
    // "AlpineGearHub" now also appears in the persistent nav bar's home link, so this has to be
    // scoped to the page heading specifically rather than matching on text alone.
    expect(screen.getByRole('heading', { name: 'AlpineGearHub' })).toBeInTheDocument()
  })
})
