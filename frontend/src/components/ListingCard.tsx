import { Link } from 'react-router-dom'
import { conditionLabels, formatPrice, statusStyles } from '../lib/listingLabels'
import type { ListingSummaryResponse } from '../types/listing'

export function ListingCard({ listing }: { listing: ListingSummaryResponse }) {
  return (
    <Link
      to={`/listings/${listing.id}`}
      className="group flex h-full flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      {/* relative + the img absolutely positioned (not just h-full/w-full) - a portrait-oriented
          photo was otherwise pulling its own intrinsic ratio into this box's auto-height
          calculation and overriding aspect-square, stretching that one card's image tall instead
          of square. Absolute positioning takes the img out of that calculation entirely. */}
      <div className="relative aspect-square w-full shrink-0 overflow-hidden bg-gray-100">
        {listing.primaryImageUrl ? (
          <img
            src={listing.primaryImageUrl}
            alt={listing.title}
            className="absolute inset-0 h-full w-full object-cover transition group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">No photo</div>
        )}
      </div>

      {/* min-w-0 on both flex-col levels - without it, a flex item defaults to min-width:auto,
          so the title row refuses to shrink below its unwrapped text width and drags the whole
          card (and its grid column) wider instead of actually wrapping/line-clamping. */}
      <div className="flex min-w-0 flex-1 flex-col p-3">
        <div className="flex min-w-0 items-start justify-between gap-2">
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

        {/* mt-auto - anchors this row (and the status badge after it) to the bottom of the card
            instead of leaving a gap below it when a sibling card's longer title stretches this
            card taller. shrink-0 + truncate: condition is always short ("Like New" at most) so it
            never needs to shrink, but a long location used to wrap mid-word on narrow cards;
            now it ellipsizes on one line instead. */}
        <div className="mt-auto flex items-center justify-between gap-2 pt-2 text-xs text-gray-500">
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
