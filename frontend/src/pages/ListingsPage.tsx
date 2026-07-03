import { useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { ListingCard } from '../components/ListingCard'
import { Pagination } from '../components/Pagination'
import { useCategories, useListings } from '../hooks/useListings'
import { conditionLabels } from '../lib/listingLabels'
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
      <h1 className="text-xl font-bold text-gray-900">{sellerId ? 'My listings' : 'Browse gear'}</h1>

      <div className="mt-4 flex flex-wrap items-end gap-3">
        <form onSubmit={handleSearchSubmit} className="flex gap-2">
          <input
            type="search"
            placeholder="Search listings…"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="rounded-md border border-gray-300 px-3 py-1.5 text-sm"
          />
          <button type="submit" className="rounded-md bg-gray-900 px-3 py-1.5 text-sm text-white">
            Search
          </button>
        </form>

        <select
          value={categoryId ?? ''}
          onChange={(e) => updateParam('categoryId', e.target.value || undefined)}
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm"
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
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm"
        >
          <option value="">Any condition</option>
          {CONDITIONS.map((value) => (
            <option key={value} value={value}>
              {conditionLabels[value]}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="mt-8 text-sm text-gray-500">Loading listings…</p>}
      {isError && <p className="mt-8 text-sm text-red-600">Couldn&apos;t load listings. Try again shortly.</p>}

      {data && data.items.length === 0 && (
        <p className="mt-8 text-sm text-gray-500">No listings match those filters.</p>
      )}

      {data && data.items.length > 0 && (
        <>
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
