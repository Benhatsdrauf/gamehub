// Base URL of the .NET API. Overridable per-environment via VITE_API_URL (.env),
// with a dev default so the app runs out of the box.
export const API_URL =
  (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5206/api'
