import { useState, type ChangeEvent, type ReactNode } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useParams } from 'react-router-dom'
import { FormField, formInputClasses } from '../components/FormField'
import { PromotionCheckout } from '../components/PromotionCheckout'
import { useAuth } from '../contexts/AuthContext'
import {
  useChangeListingStatus,
  useDeleteListingImage,
  useListing,
  usePublishListing,
  useUpdateListing,
  useUploadListingImage,
} from '../hooks/useListings'
import { conditionLabels, statusStyles } from '../lib/listingLabels'
import { buttonPrimary } from '../lib/uiClasses'
import {
  updateListingSchema,
  type UpdateListingFormInput,
  type UpdateListingFormValues,
} from '../lib/validation/listingSchemas'
import type { GearCondition } from '../types/listing'

const CONDITIONS = Object.keys(conditionLabels) as GearCondition[]
const CURRENCIES = ['EUR', 'USD', 'GBP', 'PLN']
const MAX_IMAGES = 8

export default function EditListingPage() {
  const { id = '' } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const { data: listing, isLoading } = useListing(id)

  const updateListing = useUpdateListing(id)
  const publishListing = usePublishListing(id)
  const changeStatus = useChangeListingStatus(id)
  const uploadImage = useUploadListingImage(id)
  const deleteImage = useDeleteListingImage(id)

  const [formError, setFormError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<UpdateListingFormInput, unknown, UpdateListingFormValues>({
    resolver: zodResolver(updateListingSchema),
    values: listing
      ? {
          title: listing.title,
          description: listing.description,
          price: listing.price,
          currency: listing.currency,
          condition: listing.condition,
          location: listing.location,
        }
      : undefined,
  })

  if (isLoading) return <p className="text-sm text-gray-500">Loading…</p>
  if (!listing)
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white py-12 text-center">
        <p className="text-sm text-red-600">This listing couldn&apos;t be found.</p>
      </div>
    )
  if (user?.id !== listing.sellerId) {
    return (
      <div className="rounded-xl border border-dashed border-gray-300 bg-white py-12 text-center">
        <p className="text-sm text-red-600">Only the seller can manage this listing.</p>
      </div>
    )
  }

  async function onSubmit(values: UpdateListingFormValues) {
    setFormError(null)
    try {
      await updateListing.mutateAsync(values)
      reset(values)
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Could not save changes')
    }
  }

  async function runAction(action: () => Promise<unknown>) {
    setActionError(null)
    try {
      await action()
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'That action failed')
    }
  }

  async function handleImageUpload(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return
    await runAction(() => uploadImage.mutateAsync(file));
  }

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-gray-900">Manage listing</h1>
        <span className={`mt-2 inline-block rounded px-2 py-1 text-xs font-medium ${statusStyles[listing.status]}`}>
          {listing.status}
        </span>
      </div>

      <section className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h2 className="text-sm font-semibold text-gray-900">Photos</h2>
        <div className="mt-3 flex flex-wrap gap-3">
          {listing.images.map((image) => (
            <div key={image.id} className="group relative h-20 w-20 overflow-hidden rounded-lg border border-gray-200">
              <img src={image.url} alt="" className="h-full w-full object-cover" />
              <button
                type="button"
                onClick={() => runAction(() => deleteImage.mutateAsync(image.id))}
                className="absolute right-0 top-0 rounded-bl-lg bg-black/60 px-1.5 py-0.5 text-xs text-white transition-colors hover:bg-red-600"
              >
                ✕
              </button>
            </div>
          ))}
        </div>

        {listing.images.length < MAX_IMAGES ? (
          <label className="mt-3 inline-block cursor-pointer text-sm font-medium text-emerald-700 hover:underline">
            {uploadImage.isPending ? 'Uploading…' : '+ Add a photo'}
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp"
              className="hidden"
              onChange={handleImageUpload}
              disabled={uploadImage.isPending}
            />
          </label>
        ) : (
          <p className="mt-3 text-xs text-gray-500">Maximum of {MAX_IMAGES} photos reached.</p>
        )}
      </section>

      <section className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h2 className="text-sm font-semibold text-gray-900">Status</h2>
        {actionError && <p className="mt-1 text-sm text-red-600">{actionError}</p>}
        <div className="mt-3 flex flex-wrap gap-2">
          {listing.status === 'Draft' && (
            <ActionButton onClick={() => runAction(() => publishListing.mutateAsync())}>Publish</ActionButton>
          )}
          {listing.status === 'Active' && (
            <ActionButton onClick={() => runAction(() => changeStatus.mutateAsync('Reserve'))}>
              Mark as reserved
            </ActionButton>
          )}
          {(listing.status === 'Active' || listing.status === 'Reserved') && (
            <ActionButton onClick={() => runAction(() => changeStatus.mutateAsync('Sell'))}>
              Mark as sold
            </ActionButton>
          )}
          {listing.status === 'Expired' && (
            <ActionButton onClick={() => runAction(() => changeStatus.mutateAsync('Renew'))}>
              Renew listing
            </ActionButton>
          )}
          {listing.status !== 'Sold' && listing.status !== 'Removed' && (
            <ActionButton
              danger
              onClick={() => {
                // This is the one irreversible-feeling action on this page, sitting right next
                // to reversible status buttons - a native confirm is enough friction to catch a
                // misclick without needing a whole modal component just for this one case.
                if (!window.confirm(`Remove "${listing.title}"? This can't be undone.`)) return
                runAction(async () => {
                  await changeStatus.mutateAsync('Remove')
                  navigate('/listings')
                })
              }}
            >
              Remove listing
            </ActionButton>
          )}
        </div>
      </section>

      {(listing.status === 'Active' || listing.status === 'Reserved') && (
        <section className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <h2 className="text-sm font-semibold text-gray-900">Promotion</h2>
          <div className="mt-3">
            <PromotionCheckout listingId={listing.id} />
          </div>
        </section>
      )}

      <section className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <h2 className="text-sm font-semibold text-gray-900">Details</h2>
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-3 space-y-4">
          <FormField label="Title" htmlFor="title" error={errors.title?.message}>
            <input id="title" type="text" className={formInputClasses} {...register('title')} />
          </FormField>

          <FormField label="Description" htmlFor="description" error={errors.description?.message}>
            <textarea id="description" rows={5} className={formInputClasses} {...register('description')} />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Price" htmlFor="price" error={errors.price?.message}>
              <input
                id="price"
                type="number"
                step="0.01"
                min="0"
                className={formInputClasses}
                {...register('price')}
              />
            </FormField>

            <FormField label="Currency" htmlFor="currency" error={errors.currency?.message}>
              <select id="currency" className={formInputClasses} {...register('currency')}>
                {CURRENCIES.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </FormField>
          </div>

          <FormField label="Condition" htmlFor="condition" error={errors.condition?.message}>
            <select id="condition" className={formInputClasses} {...register('condition')}>
              {CONDITIONS.map((value) => (
                <option key={value} value={value}>
                  {conditionLabels[value]}
                </option>
              ))}
            </select>
          </FormField>

          <FormField label="Location" htmlFor="location" error={errors.location?.message}>
            <input id="location" type="text" className={formInputClasses} {...register('location')} />
          </FormField>

          {formError && <p className="text-sm text-red-600">{formError}</p>}

          <button type="submit" disabled={isSubmitting || !isDirty} className={`w-full ${buttonPrimary}`}>
            {isSubmitting ? 'Saving…' : 'Save changes'}
          </button>
        </form>
      </section>
    </div>
  )
}

function ActionButton({
  children,
  onClick,
  danger,
}: {
  children: ReactNode
  onClick: () => void
  danger?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
        danger
          ? 'border-red-300 text-red-700 hover:bg-red-50'
          : 'border-gray-300 text-gray-700 hover:bg-gray-50'
      }`}
    >
      {children}
    </button>
  )
}
