import * as z from 'zod'

// Mirrors the backend LoginResponse (System.Text.Json camel-cases the property names).
export const loginResponseSchema = z.object({
  accessToken: z.string(),
  accessTokenExpiresAtUtc: z.string(),
  refreshToken: z.string(),
  refreshTokenExpiresAtUtc: z.string(),
  userId: z.string(),
  username: z.string(),
  email: z.string(),
  role: z.string(),
})
export type LoginResponse = z.infer<typeof loginResponseSchema>

export const refreshResponseSchema = z.object({
  accessToken: z.string(),
  accessTokenExpiresAtUtc: z.string(),
  refreshToken: z.string(),
  refreshTokenExpiresAtUtc: z.string(),
})
export type RefreshResponse = z.infer<typeof refreshResponseSchema>

export const userListItemSchema = z.object({
  id: z.string(),
  username: z.string(),
  email: z.string(),
})
export type UserListItem = z.infer<typeof userListItemSchema>

export const pagedUsersSchema = z.object({
  items: z.array(userListItemSchema),
  page: z.number(),
  pageSize: z.number(),
  totalCount: z.number(),
  totalPages: z.number(),
})
export type PagedUsers = z.infer<typeof pagedUsersSchema>
