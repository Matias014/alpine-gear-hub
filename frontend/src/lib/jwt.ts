// Just reading our own token to display/compare a user id - not a security check, the server
// re-verifies everything anyway, so no need to check the signature here.
export function decodeJwtSubject(token: string): string | null {
  try {
    const payload = token.split('.')[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = JSON.parse(atob(normalized)) as { sub?: string }
    return decoded.sub ?? null
  } catch {
    return null
  }
}
