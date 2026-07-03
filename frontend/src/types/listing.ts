export type GearCondition = 'New' | 'LikeNew' | 'Good' | 'Fair' | 'Poor'
export type ListingStatus = 'Draft' | 'Active' | 'Reserved' | 'Sold' | 'Expired' | 'Removed'
export type ListingStatusAction = 'Reserve' | 'Sell' | 'Renew' | 'Remove'

export interface CategoryResponse {
  id: string
  name: string
  slug: string
}

export interface ListingImageResponse {
  id: string
  url: string
  sortOrder: number
  isPrimary: boolean
}

export interface ListingResponse {
  id: string
  sellerId: string
  categoryId: string
  categoryName: string
  title: string
  description: string
  price: number
  currency: string
  condition: GearCondition
  status: ListingStatus
  location: string
  isPromoted: boolean
  createdAt: string
  expiresAt: string | null
  images: ListingImageResponse[]
}

export interface ListingSummaryResponse {
  id: string
  sellerId: string
  title: string
  price: number
  currency: string
  condition: GearCondition
  status: ListingStatus
  location: string
  isPromoted: boolean
  primaryImageUrl: string | null
  createdAt: string
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface ListingFilters {
  categoryId?: string
  condition?: GearCondition
  minPrice?: number
  maxPrice?: number
  search?: string
  sellerId?: string
  page?: number
  pageSize?: number
}

export interface CreateListingRequest {
  categoryId: string
  title: string
  description: string
  price: number
  currency: string
  condition: GearCondition
  location: string
}

export type UpdateListingRequest = Omit<CreateListingRequest, 'categoryId'>
