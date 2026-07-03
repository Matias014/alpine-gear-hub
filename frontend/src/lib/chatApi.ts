import { api } from './api'
import type { ConversationResponse, ConversationSummaryResponse, MessageResponse } from '../types/chat'

export const chatApi = {
  getConversations: () => api.get<ConversationSummaryResponse[]>('/chat/conversations'),

  startConversation: (listingId: string) =>
    api.post<ConversationResponse>('/chat/conversations', { listingId }),

  getMessages: (conversationId: string) =>
    api.get<MessageResponse[]>(`/chat/conversations/${conversationId}/messages`),

  sendMessage: (conversationId: string, body: string) =>
    api.post<MessageResponse>(`/chat/conversations/${conversationId}/messages`, { body }),

  markAsRead: (conversationId: string) => api.post<void>(`/chat/conversations/${conversationId}/read`),
}
