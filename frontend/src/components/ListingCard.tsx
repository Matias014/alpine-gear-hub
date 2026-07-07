import { Link } from 'react-router-dom'
import { conditionLabels, formatPrice, statusStyles } from '../lib/listingLabels'
import type { ListingSummaryResponse } from '../types/listing'

export function ListingCard({ listing }: { listing: ListingSummaryResponse }) {
  return (
    <Link
      to={`/listings/${listing.id}`}
      className="group block overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="aspect-square w-full bg-gray-100">
        {listing.primaryImageUrl ? (
          <img
            src={listing.primaryImageUrl}
            alt={listing.title}
            className="h-full w-full object-cover transition group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">No photo</div>
        )}
      </div>

      <div className="p-3">
        <div className="flex items-start justify-between gap-2">
          <h3 className="line-clamp-2 text-sm font-medium text-gray-900" title={listing.title}>
            {listing.title}
          </h3>
          {listing.isPromoted && (
            <span className="shrink-0 rounded bg-amber-400 px-1.5 py-0.5 text-[10px] font-semibold uppercase text-white">
              Featured
            </span>
          )}
        </div>

        <p className="mt-1 text-base font-semibold text-gray-900">{formatPrice(listing.price, listing.currency)}</p>

        {/* shrink-0 + truncate (not two plain spans) - condition is always short ("Like New" at
            most) so it never needs to shrink, but a long location used to wrap mid-word once the
            card got narrow; now it ellipsizes on one line instead. */}
        <div className="mt-2 flex items-center justify-between gap-2 text-xs text-gray-500">
          <span className="shrink-0">{conditionLabels[listing.condition]}</span>
          <span className="truncate text-right" title={listing.location}>
            {listing.location}
          </span>
        </div>

        {listing.status !== 'Active' && (
          <span
            className={`mt-2 inline-block rounded px-1.5 py-0.5 text-[10px] font-medium ${statusStyles[listing.status]}`}
          >
            {listing.status}
          </span>
        )}
      </div>
    </Link>
  )
}
