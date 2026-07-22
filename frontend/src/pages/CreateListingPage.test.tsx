import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import CreateListingPage from './CreateListingPage'
import { useCategories, useCreateListing } from '../hooks/useListings'

vi.mock('../hooks/useListings', () => ({ useCategories: vi.fn(), useCreateListing: vi.fn() }))

function renderPage() {
  return render(
    <MemoryRouter initialEntries={['/listings/new']}>
      <Routes>
        <Route path="/listings/new" element={<CreateListingPage />} />
        <Route path="/listings/:id/edit" element={<p>edit listing page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('CreateListingPage', () => {
  it('creates a draft listing and navigates to its edit page', async () => {
    vi.mocked(useCategories).mockReturnValue({
      data: [{ id: 'cat-1', name: 'Ropes', slug: 'ropes' }],
    } as unknown as ReturnType<typeof useCategories>)
    const mutateAsync = vi.fn().mockResolvedValue({ id: 'listing-1' })
    vi.mocked(useCreateListing).mockReturnValue({ mutateAsync } as unknown as ReturnType<typeof useCreateListing>)
    const user = userEvent.setup()

    renderPage()
    await user.selectOptions(screen.getByLabelText('Category'), 'cat-1')
    await user.type(screen.getByLabelText('Title'), 'Petzl GriGri')
    await user.type(screen.getByLabelText('Description'), 'Barely used belay device.')
    await user.type(screen.getByLabelText('Price'), '65')
    await user.type(screen.getByLabelText('Location'), 'Chamonix')
    await user.click(screen.getByRole('button', { name: 'Create draft listing' }))

    expect(mutateAsync).toHaveBeenCalledWith({
      categoryId: 'cat-1',
      title: 'Petzl GriGri',
      description: 'Barely used belay device.',
      price: 65,
      currency: 'EUR',
      condition: 'Good',
      location: 'Chamonix',
    })
    expect(await screen.findByText('edit listing page')).toBeInTheDocument()
  })

  it('shows a validation error instead of submitting when no category is selected', async () => {
    vi.mocked(useCategories).mockReturnValue({
      data: [{ id: 'cat-1', name: 'Ropes', slug: 'ropes' }],
    } as unknown as ReturnType<typeof useCategories>)
    const mutateAsync = vi.fn()
    vi.mocked(useCreateListing).mockReturnValue({ mutateAsync } as unknown as ReturnType<typeof useCreateListing>)
    const user = userEvent.setup()

    renderPage()
    await user.type(screen.getByLabelText('Title'), 'Petzl GriGri')
    await user.type(screen.getByLabelText('Description'), 'Barely used belay device.')
    await user.type(screen.getByLabelText('Price'), '65')
    await user.type(screen.getByLabelText('Location'), 'Chamonix')
    await user.click(screen.getByRole('button', { name: 'Create draft listing' }))

    expect(await screen.findByText('Choose a category')).toBeInTheDocument()
    expect(mutateAsync).not.toHaveBeenCalled()
  })

  it('shows a server error when creation fails', async () => {
    vi.mocked(useCategories).mockReturnValue({
      data: [{ id: 'cat-1', name: 'Ropes', slug: 'ropes' }],
    } as unknown as ReturnType<typeof useCategories>)
    const mutateAsync = vi.fn().mockRejectedValue(new Error('Server is unavailable'))
    vi.mocked(useCreateListing).mockReturnValue({ mutateAsync } as unknown as ReturnType<typeof useCreateListing>)
    const user = userEvent.setup()

    renderPage()
    await user.selectOptions(screen.getByLabelText('Category'), 'cat-1')
    await user.type(screen.getByLabelText('Title'), 'Petzl GriGri')
    await user.type(screen.getByLabelText('Description'), 'Barely used belay device.')
    await user.type(screen.getByLabelText('Price'), '65')
    await user.type(screen.getByLabelText('Location'), 'Chamonix')
    await user.click(screen.getByRole('button', { name: 'Create draft listing' }))

    expect(await screen.findByText('Server is unavailable')).toBeInTheDocument()
  })
})
