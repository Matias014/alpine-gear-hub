import { describe, expect, it } from 'vitest'
import { reportSchema } from './reportSchema'

describe('reportSchema', () => {
  it('accepts a valid reason with no description', () => {
    const result = reportSchema.safeParse({ reason: 'Other' })
    expect(result.success).toBe(true)
  })

  it('accepts a valid reason with a description', () => {
    const result = reportSchema.safeParse({ reason: 'Scam', description: 'Seller asked for payment off-platform.' })
    expect(result.success).toBe(true)
  })

  it('rejects a reason outside the known enum', () => {
    const result = reportSchema.safeParse({ reason: 'Spam' })
    expect(result.success).toBe(false)
  })

  it('rejects a description over 1000 characters', () => {
    const result = reportSchema.safeParse({ reason: 'Other', description: 'a'.repeat(1001) })
    expect(result.success).toBe(false)
  })
})
