import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { chatApi } from '../lib/chatApi'
import { useAuth } from '../contexts/AuthContext'

export function useConversations() {
  const { isAuthenticated } = useAuth()
  return useQuery({
    queryKey: ['conversations'],
    queryFn: chatApi.getConversations,
    enabled: isAuthenticated,
  })
}

export function useUnreadMessagesCount() {
  const { data } = useConversations()
  return data?.reduce((sum, conversation) => sum + conversation.unreadCount, 0) ?? 0
}

export function useMessages(conversationId: string | undefined) {
  return useQuery({
    queryKey: ['messages', conversationId],
    queryFn: () => chatApi.getMessages(conversationId!),
    enabled: conversationId !== undefined,
  })
}

export function useStartConversation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (listingId: string) => chatApi.startConversation(listingId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  })
}

export function useSendMessage(conversationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: string) => chatApi.sendMessage(conversationId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages', conversationId] })
      queryClient.invalidateQueries({ queryKey: ['conversations'] })
    },
  })
}

export function useMarkConversationAsRead(conversationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => chatApi.markAsRead(conversationId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  })
}
