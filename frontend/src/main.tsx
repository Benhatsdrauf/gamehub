import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createRouter, RouterProvider } from '@tanstack/react-router'
import { AuthProvider, useAuth } from '@/lib/auth/AuthProvider'
import { routeTree } from './routeTree.gen'
import './index.css'

const queryClient = new QueryClient()

// context.auth is a placeholder here; InnerApp injects the real value at render time.
const router = createRouter({
  routeTree,
  context: { auth: undefined! },
})

// Make the router instance type-aware across the app.
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

function InnerApp() {
  const auth = useAuth()

  // Wait for the startup silent-refresh to finish before route guards decide
  // "logged out" — otherwise a valid session would flash the login page on reload.
  if (auth.isBootstrapping) {
    return (
      <div className="min-h-svh flex items-center justify-center text-muted-foreground">
        Loading…
      </div>
    )
  }

  return <RouterProvider router={router} context={{ auth }} />
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <InnerApp />
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>,
)
