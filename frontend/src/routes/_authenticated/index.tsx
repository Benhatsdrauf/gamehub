import { createFileRoute } from '@tanstack/react-router'
import { useAuth } from '@/lib/auth/AuthProvider'

export const Route = createFileRoute('/_authenticated/')({
  component: Home,
})

function Home() {
  const { user, isAdmin } = useAuth()

  return (
    <div className="space-y-2">
      <h1 className="text-2xl font-semibold tracking-tight">Welcome back</h1>
      <p className="text-muted-foreground">
        Signed in as {user?.email} ({user?.role}).
      </p>
      {isAdmin && (
        <p className="text-sm">
          You&apos;re an admin — open <span className="font-medium">Users</span> to manage accounts.
        </p>
      )}
    </div>
  )
}
