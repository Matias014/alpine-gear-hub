import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { RequireAuth } from './RequireAuth'
import { useAuth } from '../contexts/AuthContext'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/login" element={<p>login page</p>} />
        <Route
          path="/protected"
          element={
            <RequireAuth>
              <p>secret content</p>
            </RequireAuth>
          }
        />
      </Routes>
    </MemoryRouter>,
  )
}

describe('RequireAuth', () => {
  it('renders nothing while auth state is still loading', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, isLoading: true } as ReturnType<typeof useAuth>)
    renderAt('/protected')
    expect(screen.queryByText('secret content')).not.toBeInTheDocument()
    expect(screen.queryByText('login page')).not.toBeInTheDocument()
  })

  it('redirects to /login when not authenticated', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false, isLoading: false } as ReturnType<typeof useAuth>)
    renderAt('/protected')
    expect(screen.getByText('login page')).toBeInTheDocument()
  })

  it('renders the protected content once authenticated', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true, isLoading: false } as ReturnType<typeof useAuth>)
    renderAt('/protected')
    expect(screen.getByText('secret content')).toBeInTheDocument()
  })
})
