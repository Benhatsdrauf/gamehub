import { createRootRouteWithContext, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { Toaster } from '@/components/ui/sonner'
import type { AuthContextValue } from '@/lib/auth/AuthProvider'

// Everything injected into the router at render time. beforeLoad guards read this.
export interface RouterContext {
  auth: AuthContextValue
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => (
    <>
      <Outlet />
      <Toaster />
      <TanStackRouterDevtools />
    </>
  ),
})
