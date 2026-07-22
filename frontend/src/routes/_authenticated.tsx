import { createFileRoute, Link, Outlet, redirect, useNavigate } from '@tanstack/react-router'
import { useAuth } from '@/lib/auth/AuthProvider'
import { Button } from '@/components/ui/button'

// A pathless layout route: its children render at "/", "/users", etc. (no
// "_authenticated" in the URL). The guard runs before any child loads.
export const Route = createFileRoute('/_authenticated')({
  beforeLoad: ({ context, location }) => {
    if (!context.auth.isAuthenticated) {
      // Remember where they were headed so we can return them after login.
      throw redirect({ to: '/login', search: { redirect: location.href } })
    }
  },
  component: AuthenticatedLayout,
})

function AuthenticatedLayout() {
  const { user, isAdmin, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    await navigate({ to: '/login' })
  }

  return (
    <div className="min-h-svh flex flex-col">
      <header className="border-b">
        <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
          <nav className="flex items-center gap-4">
            <Link to="/" className="font-semibold">
              GameHub
            </Link>
            {isAdmin && (
              <Link
                to="/users"
                className="text-muted-foreground hover:text-foreground text-sm"
              >
                Users
              </Link>
            )}
          </nav>
          <div className="flex items-center gap-3">
            <span className="text-muted-foreground text-sm">{user?.email}</span>
            <Button variant="outline" size="sm" onClick={handleLogout}>
              Log out
            </Button>
          </div>
        </div>
      </header>
      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
