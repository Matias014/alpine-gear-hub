import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ConfirmEmailPage from './ConfirmEmailPage'
import { authApi } from '../lib/authApi'

vi.mock('../lib/authApi', () => ({
  authApi: { confirmEmail: vi.fn(), resendEmailConfirmation: vi.fn() },
}))

beforeEach(() => {
  vi.clearAllMocks()
})

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/confirm-email" element={<ConfirmEmailPage />} />
        <Route path="/login" element={<p>login page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('ConfirmEmailPage', () => {
  it('shows a resend form when there is no token in the URL', () => {
    renderAt('/confirm-email')
    expect(screen.getByRole('heading', { name: 'Resend confirmation email' })).toBeInTheDocument()
  })

  it('sends a resend request and shows a confirmation message', async () => {
    vi.mocked(authApi.resendEmailConfirmation).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderAt('/confirm-email')
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.click(screen.getByRole('button', { name: 'Send confirmation link' }))

    expect(authApi.resendEmailConfirmation).toHaveBeenCalledWith({ email: 'jane@example.com' })
    expect(await screen.findByText(/sent a new confirmation link/)).toBeInTheDocument()
  })

  it('shows a server error when the resend request fails', async () => {
    vi.mocked(authApi.resendEmailConfirmation).mockRejectedValue(new Error('Server is unavailable'))
    const user = userEvent.setup()

    renderAt('/confirm-email')
    await user.type(screen.getByLabelText('Email'), 'jane@example.com')
    await user.click(screen.getByRole('button', { name: 'Send confirmation link' }))

    expect(await screen.findByText('Server is unavailable')).toBeInTheDocument()
  })

  it('shows a validation error instead of submitting for an invalid email', async () => {
    vi.mocked(authApi.resendEmailConfirmation).mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderAt('/confirm-email')
    await user.type(screen.getByLabelText('Email'), 'not-an-email')
    await user.click(screen.getByRole('button', { name: 'Send confirmation link' }))

    expect(await screen.findByText('Enter a valid email address')).toBeInTheDocument()
    expect(authApi.resendEmailConfirmation).not.toHaveBeenCalled()
  })

  it('confirms automatically when a token is in the URL', async () => {
    vi.mocked(authApi.confirmEmail).mockResolvedValue(undefined)

    renderAt('/confirm-email?token=raw-token-123')

    expect(await screen.findByText('Email confirmed')).toBeInTheDocument()
    expect(authApi.confirmEmail).toHaveBeenCalledWith({ token: 'raw-token-123' })
  })

  it('shows an error and a way to request a new link when the token is invalid', async () => {
    vi.mocked(authApi.confirmEmail).mockRejectedValue(
      new Error('Email confirmation link is invalid or has expired.'),
    )

    renderAt('/confirm-email?token=bad-token')

    expect(await screen.findByText('Email confirmation link is invalid or has expired.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Request a new confirmation link' })).toHaveAttribute(
      'href',
      '/confirm-email',
    )
  })
})
