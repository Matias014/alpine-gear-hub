import { describe, expect, it } from 'vitest'
import { forgotPasswordSchema, loginSchema, registerSchema, resetPasswordSchema } from './authSchemas'

describe('loginSchema', () => {
  it('accepts a valid email and non-empty password', () => {
    const result = loginSchema.safeParse({ email: 'jane@example.com', password: 'anything' })
    expect(result.success).toBe(true)
  })

  it('rejects an invalid email', () => {
    const result = loginSchema.safeParse({ email: 'not-an-email', password: 'anything' })
    expect(result.success).toBe(false)
  })

  it('rejects an empty password', () => {
    const result = loginSchema.safeParse({ email: 'jane@example.com', password: '' })
    expect(result.success).toBe(false)
  })
})

describe('registerSchema', () => {
  const base = { fullName: 'Jane Climber', email: 'jane@example.com' }

  it('accepts a password meeting every complexity rule', () => {
    const result = registerSchema.safeParse({ ...base, password: 'Str0ng!Pass' })
    expect(result.success).toBe(true)
  })

  it.each([
    ['too short', 'Ab1!'],
    ['missing uppercase', 'weak1!pass'],
    ['missing lowercase', 'WEAK1!PASS'],
    ['missing a digit', 'Weak!Pass'],
    ['missing a special character', 'Weak1Pass'],
  ])('rejects a password %s', (_label, password) => {
    const result = registerSchema.safeParse({ ...base, password })
    expect(result.success).toBe(false)
  })

  it('rejects a full name over 100 characters', () => {
    const result = registerSchema.safeParse({ ...base, fullName: 'a'.repeat(101), password: 'Str0ng!Pass' })
    expect(result.success).toBe(false)
  })

  it('rejects an empty full name', () => {
    const result = registerSchema.safeParse({ ...base, fullName: '', password: 'Str0ng!Pass' })
    expect(result.success).toBe(false)
  })
})

describe('forgotPasswordSchema', () => {
  it('accepts a valid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'jane@example.com' })
    expect(result.success).toBe(true)
  })

  it('rejects an invalid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'not-an-email' })
    expect(result.success).toBe(false)
  })
})

describe('resetPasswordSchema', () => {
  it('accepts a strong password that matches its confirmation', () => {
    const result = resetPasswordSchema.safeParse({ password: 'Str0ng!Pass', confirmPassword: 'Str0ng!Pass' })
    expect(result.success).toBe(true)
  })

  it('rejects mismatched passwords', () => {
    const result = resetPasswordSchema.safeParse({ password: 'Str0ng!Pass', confirmPassword: 'Different1!' })
    expect(result.success).toBe(false)
  })

  it.each([
    ['too short', 'Ab1!'],
    ['missing uppercase', 'weak1!pass'],
    ['missing lowercase', 'WEAK1!PASS'],
    ['missing a digit', 'Weak!Pass'],
    ['missing a special character', 'Weak1Pass'],
  ])('rejects a password %s', (_label, password) => {
    const result = resetPasswordSchema.safeParse({ password, confirmPassword: password })
    expect(result.success).toBe(false)
  })
})
