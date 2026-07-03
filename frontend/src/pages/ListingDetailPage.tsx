import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useStartConversation } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import { conditionLabels, formatPrice, statusStyles } from '../lib/listingLabels'

export default function ListingDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { user, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const { data: listing, isLoading, isError } = useListing(id)
  const [activeImageIndex, setActiveImageIndex] = useState(0)
  const startConversation = useStartConversation()
  const [messageError, setMessageError] = useState<string | null>(null)

  if (isLoading) return <p className="text-sm text-gray-500">Loading…</p>
  if (isError || !listing) return <p className="text-sm text-red-600">This listing couldn&apos;t be found.</p>

  const isOwner = user?.id === listing.sellerId
  const activeImage = listing.images[activeImageIndex]

  async function handleMessageSeller() {
    setMessageError(null)
    try {
      const conversation = await startConversation.mutateAsync(listing!.id)
      navigate(`/messages/${conversation.id}`)
    } catch (err) {
      setMessageError(err instanceof Error ? err.message : 'Could not start a conversation')
    }
  }

  return (
    <div className="grid gap-8 md:grid-cols-2">
      <div>
        <div className="aspect-square w-full overflow-hidden rounded-lg bg-gray-100">
          {activeImage ? (
            <img src={activeImage.url} alt={listing.title} className="h-full w-full object-cover" />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">No photo</div>
          )}
        </div>

        {listing.images.length > 1 && (
          <div className="mt-3 flex gap-2">
            {listing.images.map((image, index) => (
              <button
                key={image.id}
                type="button"
                onClick={() => setActiveImageIndex(index)}
                className={`h-16 w-16 overflow-hidden rounded-md border-2 ${
                  index === activeImageIndex ? 'border-emerald-600' : 'border-transparent'
                }`}
              >
                <img src={image.url} alt="" className="h-full w-full object-cover" />
              </button>
            ))}
          </div>
        )}
      </div>

      <div>
        <div className="flex items-start justify-between gap-2">
          <h1 className="text-2xl font-bold text-gray-900">{listing.title}</h1>
          {listing.isPromoted && (
            <span className="shrink-0 rounded bg-amber-400 px-2 py-1 text-xs font-semibold uppercase text-white">
              Featured
            </span>
          )}
        </div>

        <p className="mt-2 text-2xl font-semibold text-gray-900">
          {formatPrice(listing.price, listing.currency)}
        </p>

        <span className={`mt-2 inline-block rounded px-2 py-1 text-xs font-medium ${statusStyles[listing.status]}`}>
          {listing.status}
        </span>

        <dl className="mt-4 grid grid-cols-2 gap-3 text-sm">
          <div>
            <dt className="text-gray-500">Category</dt>
            <dd className="text-gray-900">{listing.categoryName}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Condition</dt>
            <dd className="text-gray-900">{conditionLabels[listing.condition]}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Location</dt>
            <dd className="text-gray-900">{listing.location}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Listed</dt>
            <dd className="text-gray-900">{new Date(listing.createdAt).toLocaleDateString()}</dd>
          </div>
        </dl>

        <p className="mt-4 whitespace-pre-wrap text-sm text-gray-700">{listing.description}</p>

        {isOwner && (
          <div className="mt-6 rounded-md border border-gray-200 bg-gray-50 p-3 text-sm">
            This is your listing.{' '}
            <Link to={`/listings/${listing.id}/edit`} className="font-medium text-emerald-700 hover:underline">
              Manage it
            </Link>
          </div>
        )}

        {!isOwner && (
          <div className="mt-6">
            {isAuthenticated ? (
              <button
                type="button"
                onClick={handleMessageSeller}
                disabled={startConversation.isPending}
                className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:opacity-50"
              >
                {startConversation.isPending ? 'Starting…' : 'Message seller'}
              </button>
            ) : (
              <Link to="/login" state={{ from: `/listings/${listing.id}` }} className="text-sm text-emerald-700 hover:underline">
                Log in to message the seller
              </Link>
            )}
            {messageError && <p className="mt-2 text-sm text-red-600">{messageError}</p>}
          </div>
        )}
      </div>
    </div>
  )
}
