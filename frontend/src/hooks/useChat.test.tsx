import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useUnreadMessagesCount } from './useChat'
import { chatApi } from '../lib/chatApi'
import { useAuth } from '../contexts/AuthContext'
import type { ConversationSummaryResponse } from '../types/chat'

vi.mock('../lib/chatApi', () => ({ chatApi: { getConversations: vi.fn() } }))
vi.mock('../contexts/AuthContext', () => ({ useAuth: vi.fn() }))

function conversation(unreadCount: number): ConversationSummaryResponse {
  return {
    id: crypto.randomUUID(),
    listingId: 'listing-1',
    otherParticipantId: 'user-2',
    lastMessageBody: null,
    lastMessageAt: null,
    unreadCount,
  }
}

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useUnreadMessagesCount', () => {
  it('sums unreadCount across every conversation', async () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true } as ReturnType<typeof useAuth>)
    vi.mocked(chatApi.getConversations).mockResolvedValue([conversation(2), conversation(0), conversation(3)])

    const { result } = renderHook(() => useUnreadMessagesCount(), { wrapper })

    await waitFor(() => expect(result.current).toBe(5))
  })

  it('returns 0 when there are no conversations', async () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: true } as ReturnType<typeof useAuth>)
    vi.mocked(chatApi.getConversations).mockResolvedValue([])

    const { result } = renderHook(() => useUnreadMessagesCount(), { wrapper })

    await waitFor(() => expect(chatApi.getConversations).toHaveBeenCalled())
    expect(result.current).toBe(0)
  })

  it('returns 0 and skips fetching entirely when unauthenticated', () => {
    vi.mocked(useAuth).mockReturnValue({ isAuthenticated: false } as ReturnType<typeof useAuth>)

    const { result } = renderHook(() => useUnreadMessagesCount(), { wrapper })

    expect(result.current).toBe(0)
    expect(chatApi.getConversations).not.toHaveBeenCalled()
  })
})
