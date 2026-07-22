import { apiRequest, apiSend } from '@/lib/api/client'
import { pagedUsersSchema, type PagedUsers } from '@/lib/api/schemas'

// Admin-only endpoints (the server enforces [Authorize(Roles="Admin")]).
export function getUsers(page = 1, pageSize = 20): Promise<PagedUsers> {
  return apiRequest(`/users?page=${page}&pageSize=${pageSize}`, pagedUsersSchema)
}

export function promoteUser(id: string): Promise<void> {
  return apiSend(`/users/${id}/promote`, { method: 'POST' })
}
