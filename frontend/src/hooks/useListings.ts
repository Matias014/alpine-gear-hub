import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { listingsApi } from '../lib/listingsApi'
import type {
  CreateListingRequest,
  ListingFilters,
  ListingStatusAction,
  UpdateListingRequest,
} from '../types/listing'

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: listingsApi.getCategories,
    staleTime: 1000 * 60 * 10,
  })
}

export function useListings(filters: ListingFilters) {
  return useQuery({
    queryKey: ['listings', filters],
    queryFn: () => listingsApi.getListings(filters),
  })
}

export function useListing(id: string | undefined) {
  return useQuery({
    queryKey: ['listing', id],
    queryFn: () => listingsApi.getListingById(id!),
    enabled: id !== undefined,
  })
}

export function useCreateListing() {
  return useMutation({
    mutationFn: (data: CreateListingRequest) => listingsApi.createListing(data),
  })
}

function useInvalidateListing(id: string) {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['listing', id] })
    queryClient.invalidateQueries({ queryKey: ['listings'] })
  }
}

export function useUpdateListing(id: string) {
  const invalidate = useInvalidateListing(id)
  return useMutation({
    mutationFn: (data: UpdateListingRequest) => listingsApi.updateListing(id, data),
    onSuccess: invalidate,
  })
}

export function usePublishListing(id: string) {
  const invalidate = useInvalidateListing(id)
  return useMutation({
    mutationFn: () => listingsApi.publishListing(id),
    onSuccess: invalidate,
  })
}

export function useChangeListingStatus(id: string) {
  const invalidate = useInvalidateListing(id)
  return useMutation({
    mutationFn: (action: ListingStatusAction) => listingsApi.changeListingStatus(id, action),
    onSuccess: invalidate,
  })
}

export function useUploadListingImage(id: string) {
  const invalidate = useInvalidateListing(id)
  return useMutation({
    mutationFn: (file: File) => listingsApi.uploadImage(id, file),
    onSuccess: invalidate,
  })
}

export function useDeleteListingImage(id: string) {
  const invalidate = useInvalidateListing(id)
  return useMutation({
    mutationFn: (imageId: string) => listingsApi.deleteImage(id, imageId),
    onSuccess: invalidate,
  })
}
