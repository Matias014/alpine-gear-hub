import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useConversations, useMarkConversationAsRead, useMessages, useSendMessage } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import { buttonPrimary } from '../lib/uiClasses'

export default function ConversationPage() {
  const { id = '' } = useParams<{ id: string }>()
  const { user } = useAuth()
  const { data: messages, isLoading } = useMessages(id)
  const markAsRead = useMarkConversationAsRead(id)
  const sendMessage = useSendMessage(id)
  const [draft, setDraft] = useState('')
  const [sendError, setSendError] = useState<string | null>(null)
  const bottomRef = useRef<HTMLDivElement>(null)

  // Reusing the conversations list cache (already fetched for the nav badge / list page in the
  // common case) rather than adding a new endpoint just to know which listing this thread is about.
  const { data: conversations } = useConversations()
  const conversation = conversations?.find((c) => c.id === id)
  const { data: listing } = useListing(conversation?.listingId)

  useEffect(() => {
    if (id) markAsRead.mutate()
    // Only re-run when switching to a different conversation, not on every render - the mutation
    // object itself is a new reference each render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body = draft.trim()
    if (!body) return

    setSendError(null)
    try {
      await sendMessage.mutateAsync(body)
      setDraft('')
    } catch (err) {
      setSendError(err instanceof Error ? err.message : 'Could not send that message')
    }
  }

  return (
    <div className="mx-auto flex h-[70vh] max-w-2xl flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="flex items-center justify-between border-b border-gray-200 px-4 py-3">
        <Link to="/messages" className="text-sm font-medium text-emerald-700 hover:underline">
          &larr; Back to messages
        </Link>
        {listing && (
          <Link to={`/listings/${listing.id}`} className="text-sm font-medium text-gray-700 hover:underline">
            About: {listing.title}
          </Link>
        )}
      </div>

      <div className="flex-1 space-y-2 overflow-y-auto bg-gray-50 p-4">
        {isLoading && <p className="text-sm text-gray-500">Loading…</p>}

        {messages?.map((message) => {
          const isMine = message.senderId === user?.id
          return (
            <div key={message.id} className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}>
              <div
                className={`max-w-[75%] rounded-2xl px-3.5 py-2 text-sm shadow-sm ${
                  isMine ? 'bg-emerald-600 text-white' : 'border border-gray-200 bg-white text-gray-900'
                }`}
              >
                <p className="whitespace-pre-wrap">{message.body}</p>
                <p className={`mt-1 text-[10px] ${isMine ? 'text-emerald-100' : 'text-gray-500'}`}>
                  {new Date(message.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
            </div>
          )
        })}
        <div ref={bottomRef} />
      </div>

      <div className="border-t border-gray-200 p-3">
        {sendError && <p className="mb-2 text-sm text-red-600">{sendError}</p>}

        <form onSubmit={handleSubmit} className="flex gap-2">
          <input
            type="text"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            placeholder="Write a message…"
            className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm transition-colors focus:border-emerald-600 focus:outline-none focus:ring-1 focus:ring-emerald-600"
          />
          <button type="submit" disabled={sendMessage.isPending || !draft.trim()} className={buttonPrimary}>
            Send
          </button>
        </form>
      </div>
    </div>
  )
}
