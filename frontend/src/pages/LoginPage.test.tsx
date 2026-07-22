import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import LoginPage from './LoginPage'
import { useAuth } from '../contexts/AuthContext'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))

function renderAt(initialEntries: Parameters<typeof MemoryRouter>[0]['initialEntries']) {
  return render(
    <MemoryRouter initialEntries={initialEntries}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={<p>home page</p>} />
        <Route path="/listings/1" element={<p>listing page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  it('logs in and navigates home by default', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useAuth).mockReturnValue({ login } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderAt(['/login'])
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password1!')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(login).toHaveBeenCalledWith({ email: 'jane@example.com', password: 'Password1!' })
    expect(await screen.findByText('home page')).toBeInTheDocument()
  })

  it('navigates back to the page that redirected here', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useAuth).mockReturnValue({ login } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderAt([{ pathname: '/login', state: { from: '/listings/1' } }])
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password1!')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('listing page')).toBeInTheDocument()
  })

  it('shows a server error and stays on the page when login fails', async () => {
    const login = vi.fn().mockRejectedValue(new Error('Invalid credentials'))
    vi.mocked(useAuth).mockReturnValue({ login } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderAt(['/login'])
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'wrong')
    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('Invalid credentials')).toBeInTheDocument()
  })
})
