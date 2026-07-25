import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ForgotPasswordPage from './ForgotPasswordPage'
import { authApi } from '../lib/authApi'

vi.mock('../lib/authApi', () => ({
  authApi: { requestPasswordReset: vi.fn() },
}))

beforeEach(() => {
  vi.clearAllMocks()
})

function renderPage() {
  return render(
    <MemoryRouter>
      <ForgotPasswordPage />
    </MemoryRouter>,
  )
}

describe('ForgotPasswordPage', () => {
  it('requests a reset link and shows a confirmation message', async () => {
    vi.mocked(authApi.requestPasswordReset).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.click(screen.getByRole('button', { name: 'Send reset link' }))

    expect(authApi.requestPasswordReset).toHaveBeenCalledWith({ email: 'jane@example.com' })
    expect(await screen.findByText(/we've sent a link to reset your password/)).toBeInTheDocument()
  })

  it('shows the same confirmation message even for an unregistered email', async () => {
    // The backend deliberately returns success either way (no user enumeration) - the frontend
    // just reflects that same always-succeeds behavior.
    vi.mocked(authApi.requestPasswordReset).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Email'), 'nobody@example.com')
    await user.click(screen.getByRole('button', { name: 'Send reset link' }))

    expect(await screen.findByText(/we've sent a link to reset your password/)).toBeInTheDocument()
  })

  it('shows a server error when the request fails unexpectedly', async () => {
    vi.mocked(authApi.requestPasswordReset).mockRejectedValue(new Error('Server is unavailable'))
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.click(screen.getByRole('button', { name: 'Send reset link' }))

    expect(await screen.findByText('Server is unavailable')).toBeInTheDocument()
  })

  it('shows a validation error instead of submitting for an invalid email', async () => {
    vi.mocked(authApi.requestPasswordReset).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Email'), 'not-an-email')
    await user.click(screen.getByRole('button', { name: 'Send reset link' }))

    expect(await screen.findByText('Enter a valid email address')).toBeInTheDocument()
    expect(authApi.requestPasswordReset).not.toHaveBeenCalled()
  })
})
