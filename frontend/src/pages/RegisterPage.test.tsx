import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import RegisterPage from './RegisterPage'
import { useAuth } from '../contexts/AuthContext'

vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))

function renderPage() {
  return render(
    <MemoryRouter initialEntries={['/register']}>
      <Routes>
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<p>home page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('RegisterPage', () => {
  it('registers and navigates home', async () => {
    const register = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useAuth).mockReturnValue({ register } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Full name'), 'Jane Climber')
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password1!')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(register).toHaveBeenCalledWith({
      fullName: 'Jane Climber',
      email: 'jane@example.com',
      password: 'Password1!',
    })
    expect(await screen.findByText('home page')).toBeInTheDocument()
  })

  it('shows a server error and stays on the page when registration fails', async () => {
    const register = vi.fn().mockRejectedValue(new Error('Email already in use'))
    vi.mocked(useAuth).mockReturnValue({ register } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Full name'), 'Jane Climber')
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'Password1!')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByText('Email already in use')).toBeInTheDocument()
  })

  it('shows a validation error instead of calling register when the password is too weak', async () => {
    const register = vi.fn()
    vi.mocked(useAuth).mockReturnValue({ register } as unknown as ReturnType<typeof useAuth>)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Full name'), 'Jane Climber')
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.type(screen.getByLabelText('Password'), 'weak')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(await screen.findByText('Password must be at least 8 characters')).toBeInTheDocument()
    expect(register).not.toHaveBeenCalled()
  })
})
