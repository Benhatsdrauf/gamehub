import * as z from 'zod'

// The backend returns RFC7807 ProblemDetails (and ValidationProblemDetails with a
// per-field `errors` map). We normalize both into one ApiError the UI can render.
const problemSchema = z.object({
  title: z.string().optional(),
  detail: z.string().optional(),
  status: z.number().optional(),
  errors: z.record(z.string(), z.array(z.string())).optional(),
})

export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors?: Record<string, string[]>

  constructor(status: number, message: string, fieldErrors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

export async function toApiError(response: Response): Promise<ApiError> {
  let message = response.statusText || `Request failed (${response.status})`
  let fieldErrors: Record<string, string[]> | undefined

  try {
    const body = problemSchema.parse(await response.json())
    message = body.detail ?? body.title ?? message
    fieldErrors = body.errors
  } catch {
    // Non-JSON or unexpected body — keep the default message.
  }

  return new ApiError(response.status, message, fieldErrors)
}
