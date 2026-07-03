export interface ConversationResponse {
  id: string
  listingId: string
  buyerId: string
  sellerId: string
  createdAt: string
  lastMessageAt: string | null
}

export interface ConversationSummaryResponse {
  id: string
  listingId: string
  otherParticipantId: string
  lastMessageBody: string | null
  lastMessageAt: string | null
  unreadCount: number
}

export interface MessageResponse {
  id: string
  conversationId: string
  senderId: string
  body: string
  sentAt: string
  readAt: string | null
}
