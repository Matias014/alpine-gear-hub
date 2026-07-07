import { useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { ListingCard } from '../components/ListingCard'
import { Pagination } from '../components/Pagination'
import { formInputClasses } from '../components/FormField'
import { useCategories, useListings } from '../hooks/useListings'
import { conditionLabels } from '../lib/listingLabels'
import { buttonPrimary } from '../lib/uiClasses'
import type { GearCondition } from '../types/listing'

const CONDITIONS = Object.keys(conditionLabels) as GearCondition[]

export default function ListingsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [searchInput, setSearchInput] = useState(searchParams.get('search') ?? '')

  const sellerId = searchParams.get('sellerId') ?? undefined
  const categoryId = searchParams.get('categoryId') ?? undefined
  const condition = (searchParams.get('condition') as GearCondition | null) ?? undefined
  const search = searchParams.get('search') ?? undefined
  const page = Number(searchParams.get('page') ?? '1')

  const { data: categories } = useCategories()
  const { data, isLoading, isError } = useListings({ sellerId, categoryId, condition, search, page, pageSize: 20 })

  function updateParam(key: string, value: string | undefined) {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    next.delete('page')
    setSearchParams(next)
  }

  function handleSearchSubmit(e: FormEvent) {
    e.preventDefault()
    updateParam('search', searchInput || undefined)
  }

  function handlePageChange(newPage: number) {
    const next = new URLSearchParams(searchParams)
    next.set('page', String(newPage))
    setSearchParams(next)
  }

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-gray-900">
        {sellerId ? 'My listings' : 'Browse gear'}
      </h1>

      <div className="mt-4 flex flex-wrap items-end gap-3 rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
        <form onSubmit={handleSearchSubmit} className="flex gap-2">
          <input
            type="search"
            placeholder="Search listings…"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className={formInputClasses}
          />
          <button type="submit" className={buttonPrimary}>
            Search
          </button>
        </form>

        {/* Category/condition filters only make sense for the full marketplace - "My listings"
            is just your own handful of items, so a category dropdown there is more clutter than
            help; search still earns its place if you ever have a lot of listings. */}
        {!sellerId && (
          <>
            <select
              value={categoryId ?? ''}
              onChange={(e) => updateParam('categoryId', e.target.value || undefined)}
              className={formInputClasses}
            >
              <option value="">All categories</option>
              {categories?.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>

            <select
              value={condition ?? ''}
              onChange={(e) => updateParam('condition', e.target.value || undefined)}
              className={formInputClasses}
            >
              <option value="">Any condition</option>
              {CONDITIONS.map((value) => (
                <option key={value} value={value}>
                  {conditionLabels[value]}
                </option>
              ))}
            </select>
          </>
        )}
      </div>

      {isLoading && <p className="mt-8 text-sm text-gray-500">Loading listings…</p>}
      {isError && <p className="mt-8 text-sm text-red-600">Couldn&apos;t load listings. Try again shortly.</p>}

      {data && data.items.length === 0 && (
        <div className="mt-8 rounded-xl border border-dashed border-gray-300 bg-white py-12 text-center">
          <p className="text-sm text-gray-500">No listings match those filters.</p>
        </div>
      )}

      {data && data.items.length > 0 && (
        <>
          {/* Grid's default stretch is what we want here - every card in a row matches the
              tallest one, and ListingCard anchors its price/condition/location to its own bottom
              via flexbox, so the extra height reads as intentional instead of as a dead gap. */}
          <div className="mt-6 grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
            {data.items.map((listing) => (
              <ListingCard key={listing.id} listing={listing} />
            ))}
          </div>

          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            hasNextPage={data.hasNextPage}
            hasPreviousPage={data.hasPreviousPage}
            onPageChange={handlePageChange}
          />
        </>
      )}
    </div>
  )
}
