import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ResetPasswordPage from './ResetPasswordPage'
import { authApi } from '../lib/authApi'

vi.mock('../lib/authApi', () => ({
  authApi: { confirmPasswordReset: vi.fn() },
}))

beforeEach(() => {
  vi.clearAllMocks()
})

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/login" element={<p>login page</p>} />
        <Route path="/forgot-password" element={<p>forgot password page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('ResetPasswordPage', () => {
  it('shows an invalid-link message when the URL has no token', () => {
    renderAt('/reset-password')
    expect(screen.getByText('This password reset link is missing its token.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Request a new link' })).toHaveAttribute('href', '/forgot-password')
  })

  it('resets the password and navigates to login', async () => {
    vi.mocked(authApi.confirmPasswordReset).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderAt('/reset-password?token=raw-token-123')
    await user.type(screen.getByLabelText('New password'), 'Str0ng!Pass')
    await user.type(screen.getByLabelText('Confirm password'), 'Str0ng!Pass')
    await user.click(screen.getByRole('button', { name: 'Reset password' }))

    expect(authApi.confirmPasswordReset).toHaveBeenCalledWith({
      token: 'raw-token-123',
      newPassword: 'Str0ng!Pass',
    })
    expect(await screen.findByText('login page')).toBeInTheDocument()
  })

  it('shows a validation error instead of submitting when passwords do not match', async () => {
    vi.mocked(authApi.confirmPasswordReset).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderAt('/reset-password?token=raw-token-123')
    await user.type(screen.getByLabelText('New password'), 'Str0ng!Pass')
    await user.type(screen.getByLabelText('Confirm password'), 'Different1!')
    await user.click(screen.getByRole('button', { name: 'Reset password' }))

    expect(await screen.findByText('Passwords must match')).toBeInTheDocument()
    expect(authApi.confirmPasswordReset).not.toHaveBeenCalled()
  })

  it('shows a server error when the token is invalid or expired', async () => {
    vi.mocked(authApi.confirmPasswordReset).mockRejectedValue(
      new Error('Password reset link is invalid or has expired.'),
    )
    const user = userEvent.setup()

    renderAt('/reset-password?token=raw-token-123')
    await user.type(screen.getByLabelText('New password'), 'Str0ng!Pass')
    await user.type(screen.getByLabelText('Confirm password'), 'Str0ng!Pass')
    await user.click(screen.getByRole('button', { name: 'Reset password' }))

    expect(await screen.findByText('Password reset link is invalid or has expired.')).toBeInTheDocument()
  })
})
