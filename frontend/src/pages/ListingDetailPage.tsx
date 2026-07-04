import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ReportListingForm } from '../components/ReportListingForm'
import { useAuth } from '../contexts/AuthContext'
import { useStartConversation } from '../hooks/useChat'
import { useListing } from '../hooks/useListings'
import { conditionLabels, formatPrice, statusStyles } from '../lib/listingLabels'
import { buttonPrimary } from '../lib/uiClasses'

export default function ListingDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { user, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const { data: listing, isLoading, isError } = useListing(id)
  const [activeImageIndex, setActiveImageIndex] = useState(0)
  const startConversation = useStartConversation()
  const [messageError, setMessageError] = useState<string | null>(null)
  const [showReportForm, setShowReportForm] = useState(false)
  const [reportSubmitted, setReportSubmitted] = useState(false)

  if (isLoading) return <p className="text-sm text-gray-500">Loading…</p>
  if (isError || !listing)
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white py-12 text-center">
        <p className="text-sm text-red-600">This listing couldn&apos;t be found.</p>
      </div>
    )

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
        <div className="aspect-square w-full overflow-hidden rounded-xl bg-gray-100 shadow-sm">
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
                className={`h-16 w-16 overflow-hidden rounded-lg ring-2 transition-colors ${
                  index === activeImageIndex ? 'ring-emerald-600' : 'ring-transparent hover:ring-gray-300'
                }`}
              >
                <img src={image.url} alt="" className="h-full w-full object-cover" />
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex items-start justify-between gap-2">
          <h1 className="text-2xl font-bold tracking-tight text-gray-900">{listing.title}</h1>
          {listing.isPromoted && (
            <span className="shrink-0 rounded bg-amber-400 px-2 py-1 text-xs font-semibold uppercase text-white">
              Featured
            </span>
          )}
        </div>

        <p className="mt-2 text-2xl font-semibold text-emerald-700">
          {formatPrice(listing.price, listing.currency)}
        </p>

        <span
          className={`mt-3 inline-block rounded px-2 py-1 text-xs font-medium ${statusStyles[listing.status]}`}
        >
          {listing.status}
        </span>

        <dl className="mt-4 grid grid-cols-2 gap-3 rounded-lg bg-gray-50 p-3 text-sm">
          <div>
            <dt className="text-gray-500">Category</dt>
            <dd className="font-medium text-gray-900">{listing.categoryName}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Condition</dt>
            <dd className="font-medium text-gray-900">{conditionLabels[listing.condition]}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Location</dt>
            <dd className="font-medium text-gray-900">{listing.location}</dd>
          </div>
          <div>
            <dt className="text-gray-500">Listed</dt>
            <dd className="font-medium text-gray-900">{new Date(listing.createdAt).toLocaleDateString()}</dd>
          </div>
        </dl>

        <p className="mt-4 whitespace-pre-wrap text-sm text-gray-700">{listing.description}</p>

        {isOwner && (
          <div className="mt-6 rounded-lg border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-900">
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
                className={buttonPrimary}
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

        {!isOwner && isAuthenticated && (
          <div className="mt-3">
            {reportSubmitted ? (
              <p className="text-sm text-gray-500">Thanks - a moderator will take a look.</p>
            ) : showReportForm ? (
              <ReportListingForm
                listingId={listing.id}
                onDone={() => {
                  setShowReportForm(false)
                  setReportSubmitted(true)
                }}
              />
            ) : (
              <button
                type="button"
                onClick={() => setShowReportForm(true)}
                className="text-sm text-gray-500 transition-colors hover:text-red-600 hover:underline"
              >
                Report this listing
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
