import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { FormField, formInputClasses } from '../components/FormField'
import { useCategories, useCreateListing } from '../hooks/useListings'
import { conditionLabels } from '../lib/listingLabels'
import { buttonPrimary } from '../lib/uiClasses'
import { listingSchema, type ListingFormInput, type ListingFormValues } from '../lib/validation/listingSchemas'
import type { GearCondition } from '../types/listing'

const CONDITIONS = Object.keys(conditionLabels) as GearCondition[]
const CURRENCIES = ['EUR', 'USD', 'GBP', 'PLN']

export default function CreateListingPage() {
  const navigate = useNavigate()
  const { data: categories } = useCategories()
  const createListing = useCreateListing()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ListingFormInput, unknown, ListingFormValues>({
    resolver: zodResolver(listingSchema),
    defaultValues: { currency: 'EUR', condition: 'Good' },
  })

  async function onSubmit(values: ListingFormValues) {
    setServerError(null)
    try {
      const listing = await createListing.mutateAsync(values)
      navigate(`/listings/${listing.id}/edit`, { replace: true })
    } catch (err) {
      setServerError(err instanceof Error ? err.message : 'Could not create the listing')
    }
  }

  return (
    <div className="mx-auto max-w-lg">
      <h1 className="text-2xl font-bold tracking-tight text-gray-900">List your gear</h1>
      <p className="mt-1 text-sm text-gray-500">
        This creates a draft. You&apos;ll add photos and publish it on the next step.
      </p>

      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className="mt-6 space-y-4 rounded-xl border border-gray-200 bg-white p-6 shadow-sm"
      >
        <FormField label="Category" htmlFor="categoryId" error={errors.categoryId?.message}>
          <select id="categoryId" className={formInputClasses} {...register('categoryId')}>
            <option value="">Select a category</option>
            {categories?.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </FormField>

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

        {serverError && <p className="text-sm text-red-600">{serverError}</p>}

        <button type="submit" disabled={isSubmitting} className={`w-full ${buttonPrimary}`}>
          {isSubmitting ? 'Creating…' : 'Create draft listing'}
        </button>
      </form>
    </div>
  )
}
