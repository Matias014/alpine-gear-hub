import { z } from 'zod'

// Mirrors CreateListingCommandValidator on the backend. Note the backend has no validator at
// all for UpdateListingCommand (only Create is registered in ListingsModule.cs) - these client
// checks are the only thing standing between a blank title and a raw DB-constraint 500.
const baseListingFields = {
  title: z.string().min(1, 'Title is required').max(120, 'Title must be 120 characters or fewer'),
  description: z
    .string()
    .min(1, 'Description is required')
    .max(3000, 'Description must be 3000 characters or fewer'),
  price: z.coerce.number().gt(0, 'Price must be greater than zero'),
  currency: z.string().length(3, 'Pick a currency'),
  condition: z.enum(['New', 'LikeNew', 'Good', 'Fair', 'Poor']),
  location: z.string().min(1, 'Location is required').max(120, 'Location must be 120 characters or fewer'),
}

export const listingSchema = z.object({
  categoryId: z.string().min(1, 'Choose a category'),
  ...baseListingFields,
})

// price uses z.coerce, so the schema's input type (raw, price: unknown) and output type
// (parsed, price: number) differ - zodResolver needs both, hence the separate input/output
// exports instead of just z.infer (which only gives the output side).
export type ListingFormInput = z.input<typeof listingSchema>
export type ListingFormValues = z.output<typeof listingSchema>

export const updateListingSchema = z.object(baseListingFields)

export type UpdateListingFormInput = z.input<typeof updateListingSchema>
export type UpdateListingFormValues = z.output<typeof updateListingSchema>
