import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import * as signalR from '@microsoft/signalr'
import { useAuth } from '../contexts/AuthContext'
import { tokenStorage } from '../lib/tokenStorage'
import type { MessageResponse } from '../types/chat'

// Mounted once near the root (in Layout) so the connection persists across every route while
// logged in, rather than reconnecting per chat page visit.
export function useChatNotifications() {
  const { isAuthenticated } = useAuth()
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!isAuthenticated) return

    // accessTokenFactory (not a token baked straight into the URL) so that if withAutomaticReconnect
    // kicks in after the access token has silently refreshed, it reconnects with the current token
    // instead of the one that was live when the connection first opened.
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => tokenStorage.get()?.accessToken ?? '' })
      .withAutomaticReconnect()
      .build()

    connection.on('MessageReceived', (message: MessageResponse) => {
      queryClient.invalidateQueries({ queryKey: ['conversations'] })
      queryClient.invalidateQueries({ queryKey: ['messages', message.conversationId] })
    })

    connection.start().catch((err: unknown) => {
      console.error('Chat connection failed to start', err)
    })

    return () => {
      void connection.stop()
    }
  }, [isAuthenticated, queryClient])
}
