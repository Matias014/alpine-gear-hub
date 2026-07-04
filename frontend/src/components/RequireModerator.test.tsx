import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { RequireModerator } from './RequireModerator'
import { useAuth } from '../contexts/AuthContext'
import type { UserRole } from '../types/auth'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/login" element={<p>login page</p>} />
        <Route path="/" element={<p>home page</p>} />
        <Route
          path="/moderation"
          element={
            <RequireModerator>
              <p>moderation queue</p>
            </RequireModerator>
          }
        />
      </Routes>
    </MemoryRouter>,
  )
}

function mockAuth(overrides: { isAuthenticated: boolean; isLoading: boolean; role?: UserRole }) {
  vi.mocked(useAuth).mockReturnValue({
    isAuthenticated: overrides.isAuthenticated,
    isLoading: overrides.isLoading,
    user: overrides.role ? { id: 'u1', fullName: 'Jane', email: 'j@x.com', role: overrides.role } : null,
  } as ReturnType<typeof useAuth>)
}

describe('RequireModerator', () => {
  it('renders nothing while loading', () => {
    mockAuth({ isAuthenticated: false, isLoading: true })
    renderAt('/moderation')
    expect(screen.queryByText('moderation queue')).not.toBeInTheDocument()
  })

  it('redirects unauthenticated users to /login', () => {
    mockAuth({ isAuthenticated: false, isLoading: false })
    renderAt('/moderation')
    expect(screen.getByText('login page')).toBeInTheDocument()
  })

  it('redirects a plain Member to home', () => {
    mockAuth({ isAuthenticated: true, isLoading: false, role: 'Member' })
    renderAt('/moderation')
    expect(screen.getByText('home page')).toBeInTheDocument()
  })

  it('renders the queue for a Moderator', () => {
    mockAuth({ isAuthenticated: true, isLoading: false, role: 'Moderator' })
    renderAt('/moderation')
    expect(screen.getByText('moderation queue')).toBeInTheDocument()
  })

  it('renders the queue for an Admin', () => {
    mockAuth({ isAuthenticated: true, isLoading: false, role: 'Admin' })
    renderAt('/moderation')
    expect(screen.getByText('moderation queue')).toBeInTheDocument()
  })
})
