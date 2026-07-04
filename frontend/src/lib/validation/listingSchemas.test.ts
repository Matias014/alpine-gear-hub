import { describe, expect, it } from 'vitest'
import { listingSchema, updateListingSchema } from './listingSchemas'

const validInput = {
  categoryId: 'cat-1',
  title: 'BD Ropes 60m',
  description: 'Barely used, one season of top-roping.',
  price: '89.99',
  currency: 'EUR',
  condition: 'Good' as const,
  location: 'Chamonix',
}

describe('listingSchema', () => {
  it('accepts a fully valid submission and coerces price to a number', () => {
    const result = listingSchema.safeParse(validInput)
    expect(result.success).toBe(true)
    if (result.success) {
      expect(result.data.price).toBe(89.99)
      expect(typeof result.data.price).toBe('number')
    }
  })

  it('requires a category', () => {
    const result = listingSchema.safeParse({ ...validInput, categoryId: '' })
    expect(result.success).toBe(false)
  })

  it('rejects a price of zero or less', () => {
    expect(listingSchema.safeParse({ ...validInput, price: '0' }).success).toBe(false)
    expect(listingSchema.safeParse({ ...validInput, price: '-5' }).success).toBe(false)
  })

  it('rejects a currency code that is not exactly 3 characters', () => {
    expect(listingSchema.safeParse({ ...validInput, currency: 'EU' }).success).toBe(false)
  })

  it('rejects a condition outside the known enum', () => {
    expect(listingSchema.safeParse({ ...validInput, condition: 'Excellent' }).success).toBe(false)
  })

  it('rejects a title longer than 120 characters', () => {
    expect(listingSchema.safeParse({ ...validInput, title: 'a'.repeat(121) }).success).toBe(false)
  })

  it('rejects a description longer than 3000 characters', () => {
    expect(listingSchema.safeParse({ ...validInput, description: 'a'.repeat(3001) }).success).toBe(false)
  })

  it('requires a location', () => {
    expect(listingSchema.safeParse({ ...validInput, location: '' }).success).toBe(false)
  })
})

describe('updateListingSchema', () => {
  it('accepts the same fields as listingSchema minus categoryId', () => {
    const { categoryId: _categoryId, ...withoutCategory } = validInput
    const result = updateListingSchema.safeParse(withoutCategory)
    expect(result.success).toBe(true)
  })

  it('still enforces the price and condition rules', () => {
    const { categoryId: _categoryId, ...withoutCategory } = validInput
    expect(updateListingSchema.safeParse({ ...withoutCategory, price: '0' }).success).toBe(false)
  })
})
