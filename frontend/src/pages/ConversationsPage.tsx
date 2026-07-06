import { Link } from 'react-router-dom'
import { useConversations } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import type { ConversationSummaryResponse } from '../types/chat'

export default function ConversationsPage() {
  const { data: conversations, isLoading, isError } = useConversations()

  if (isLoading) return <p className="text-sm text-gray-500">Loading conversations…</p>
  if (isError) return <p className="text-sm text-red-600">Couldn&apos;t load your conversations.</p>

  return (
    <div className="flex flex-1 flex-col">
      <h1 className="text-2xl font-bold tracking-tight text-gray-900">Messages</h1>

      {conversations && conversations.length === 0 ? (
        // Centered in the remaining space below the heading, matching the usual "empty state"
        // convention (e.g. an empty inbox) - a populated list stays top-aligned below instead,
        // since centering an actual list of items would look just as odd as this looked empty.
        <div className="mt-6 flex flex-1 items-center justify-center">
          <div className="rounded-xl border border-dashed border-gray-300 bg-white px-8 py-12 text-center">
            <p className="text-sm text-gray-500">No conversations yet. Message a seller from a listing to start one.</p>
          </div>
        </div>
      ) : (
        <div className="mt-4 divide-y divide-gray-200 rounded-xl border border-gray-200 bg-white shadow-sm">
          {conversations?.map((conversation) => (
            <ConversationListItem key={conversation.id} conversation={conversation} />
          ))}
        </div>
      )}
    </div>
  )
}

// The backend only gives us the other participant's id, not a name (no user-lookup endpoint
// exists yet) - showing the listing instead is both a workaround for that and arguably better
// UX for a marketplace: "your chat about the GriGri" reads better than a stranger's id anyway.
function ConversationListItem({ conversation }: { conversation: ConversationSummaryResponse }) {
  const { data: listing } = useListing(conversation.listingId)

  return (
    <Link to={`/messages/${conversation.id}`} className="flex items-center gap-3 p-4 transition-colors hover:bg-gray-50">
      <div className="h-12 w-12 shrink-0 overflow-hidden rounded-lg bg-gray-100">
        {listing?.images[0] && <img src={listing.images[0].url} alt="" className="h-full w-full object-cover" />}
      </div>

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-gray-900">{listing?.title ?? 'Listing'}</p>
        <p className="truncate text-sm text-gray-500">{conversation.lastMessageBody ?? 'No messages yet'}</p>
      </div>

      {conversation.unreadCount > 0 && (
        <span className="shrink-0 rounded-full bg-emerald-600 px-2 py-0.5 text-xs font-medium text-white">
          {conversation.unreadCount}
        </span>
      )}
    </Link>
  )
}
