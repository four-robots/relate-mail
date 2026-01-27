import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from './client'
import type { EmailListResponse, EmailDetail, Profile, EmailAddress } from './types'

// Email hooks
export function useEmails(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['emails', page, pageSize],
    queryFn: () => api.get<EmailListResponse>(`/emails?page=${page}&pageSize=${pageSize}`),
  })
}

export function useEmail(id: string) {
  return useQuery({
    queryKey: ['email', id],
    queryFn: () => api.get<EmailDetail>(`/emails/${id}`),
    enabled: !!id,
  })
}

export function useMarkEmailRead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isRead }: { id: string; isRead: boolean }) =>
      api.patch<EmailDetail>(`/emails/${id}`, { isRead }),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(['email', variables.id], data)
      queryClient.invalidateQueries({ queryKey: ['emails'] })
    },
  })
}

export function useDeleteEmail() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => api.delete(`/emails/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['emails'] })
    },
  })
}

// Profile hooks
export function useProfile() {
  return useQuery({
    queryKey: ['profile'],
    queryFn: () => api.get<Profile>('/profile'),
  })
}

export function useUpdateProfile() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: { displayName?: string }) =>
      api.put<Profile>('/profile', data),
    onSuccess: (data) => {
      queryClient.setQueryData(['profile'], data)
    },
  })
}

export function useAddEmailAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (address: string) =>
      api.post<EmailAddress>('/profile/addresses', { address }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] })
    },
  })
}

export function useRemoveEmailAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (addressId: string) =>
      api.delete(`/profile/addresses/${addressId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] })
    },
  })
}
