import { api } from './api'
import type {
  CategoryResponse,
  CreateListingRequest,
  ListingFilters,
  ListingImageResponse,
  ListingResponse,
  ListingStatusAction,
  ListingSummaryResponse,
  PagedResponse,
  UpdateListingRequest,
} from '../types/listing'

// The backend overwrites SellerId/RequesterId from the JWT regardless of what's sent, but the
// field is a non-nullable Guid on the wire - an empty string fails JSON deserialization outright
// (learned this the hard way testing against the real API), so this has to be a real Guid shape.
const EMPTY_GUID = '00000000-0000-0000-0000-000000000000'

function toQueryString(filters: ListingFilters): string {
  const params = new URLSearchParams()
  if (filters.categoryId) params.set('categoryId', filters.categoryId)
  if (filters.condition) params.set('condition', filters.condition)
  if (filters.minPrice !== undefined) params.set('minPrice', String(filters.minPrice))
  if (filters.maxPrice !== undefined) params.set('maxPrice', String(filters.maxPrice))
  if (filters.search) params.set('search', filters.search)
  if (filters.sellerId) params.set('sellerId', filters.sellerId)
  params.set('page', String(filters.page ?? 1))
  params.set('pageSize', String(filters.pageSize ?? 20))
  return params.toString()
}

export const listingsApi = {
  getCategories: () => api.get<CategoryResponse[]>('/categories'),

  getListings: (filters: ListingFilters) =>
    api.get<PagedResponse<ListingSummaryResponse>>(`/listings?${toQueryString(filters)}`),

  getListingById: (id: string) => api.get<ListingResponse>(`/listings/${id}`),

  createListing: (data: CreateListingRequest) =>
    api.post<ListingResponse>('/listings', { ...data, sellerId: EMPTY_GUID }),

  updateListing: (id: string, data: UpdateListingRequest) =>
    api.put<ListingResponse>(`/listings/${id}`, { ...data, listingId: id, requesterId: EMPTY_GUID }),

  publishListing: (id: string) => api.post<void>(`/listings/${id}/publish`),

  changeListingStatus: (id: string, action: ListingStatusAction) =>
    api.post<void>(`/listings/${id}/status`, { action }),

  uploadImage: (id: string, file: File) => api.upload<ListingImageResponse>(`/listings/${id}/images`, file),

  deleteImage: (id: string, imageId: string) => api.delete<void>(`/listings/${id}/images/${imageId}`),
}
