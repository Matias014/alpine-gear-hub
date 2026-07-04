import { describe, expect, it } from 'vitest'
import { decodeJwtSubject } from './jwt'

function makeToken(payload: object): string {
  const base64url = (obj: object) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
  return `${base64url({ alg: 'HS256' })}.${base64url(payload)}.signature`
}

describe('decodeJwtSubject', () => {
  it('extracts the sub claim from a well-formed token', () => {
    const token = makeToken({ sub: 'user-123', role: 'Member' })
    expect(decodeJwtSubject(token)).toBe('user-123')
  })

  it('handles payloads that need base64url -> base64 normalization', () => {
    // Pick a subject whose JSON encoding is very likely to produce +/ characters in base64.
    const token = makeToken({ sub: '>>>???///+++' })
    expect(decodeJwtSubject(token)).toBe('>>>???///+++')
  })

  it('returns null when the token has no sub claim', () => {
    const token = makeToken({ role: 'Member' })
    expect(decodeJwtSubject(token)).toBeNull()
  })

  it('returns null for a malformed token', () => {
    expect(decodeJwtSubject('not-a-jwt')).toBeNull()
  })

  it('returns null for an empty string', () => {
    expect(decodeJwtSubject('')).toBeNull()
  })
})
