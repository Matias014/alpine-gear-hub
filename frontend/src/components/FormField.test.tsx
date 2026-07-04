import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { FormField } from './FormField'

describe('FormField', () => {
  it('renders the label wired to the input via htmlFor/id', () => {
    render(
      <FormField label="Email" htmlFor="email">
        <input id="email" />
      </FormField>,
    )
    expect(screen.getByLabelText('Email')).toBeInTheDocument()
  })

  it('shows an error message when one is provided', () => {
    render(
      <FormField label="Email" htmlFor="email" error="Enter a valid email address">
        <input id="email" />
      </FormField>,
    )
    expect(screen.getByText('Enter a valid email address')).toBeInTheDocument()
  })

  it('renders no error message when none is provided', () => {
    const { container } = render(
      <FormField label="Email" htmlFor="email">
        <input id="email" />
      </FormField>,
    )
    expect(container.querySelector('.text-red-600')).not.toBeInTheDocument()
  })
})
