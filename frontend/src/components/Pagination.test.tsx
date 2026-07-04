import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Pagination } from './Pagination'

describe('Pagination', () => {
  it('renders nothing when there is only one page', () => {
    const { container } = render(
      <Pagination page={1} totalPages={1} hasNextPage={false} hasPreviousPage={false} onPageChange={vi.fn()} />,
    )
    expect(container).toBeEmptyDOMElement()
  })

  it('shows the current page and total', () => {
    render(<Pagination page={2} totalPages={5} hasNextPage={true} hasPreviousPage={true} onPageChange={vi.fn()} />)
    expect(screen.getByText('Page 2 of 5')).toBeInTheDocument()
  })

  it('disables Previous on the first page and Next on the last page', () => {
    render(<Pagination page={1} totalPages={3} hasNextPage={true} hasPreviousPage={false} onPageChange={vi.fn()} />)
    expect(screen.getByRole('button', { name: 'Previous' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Next' })).toBeEnabled()
  })

  it('calls onPageChange with the adjacent page number', async () => {
    const onPageChange = vi.fn()
    const user = userEvent.setup()
    render(<Pagination page={2} totalPages={5} hasNextPage={true} hasPreviousPage={true} onPageChange={onPageChange} />)

    await user.click(screen.getByRole('button', { name: 'Next' }))
    expect(onPageChange).toHaveBeenCalledWith(3)

    await user.click(screen.getByRole('button', { name: 'Previous' }))
    expect(onPageChange).toHaveBeenCalledWith(1)
  })
})
