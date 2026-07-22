import { createFileRoute } from '@tanstack/react-router'
import { Button } from '@/components/ui/button'

export const Route = createFileRoute('/')({
  component: Home,
})

function Home() {
  return (
    <div className="min-h-svh flex flex-col items-center justify-center gap-4">
      <h1 className="text-3xl font-semibold tracking-tight">GameHub</h1>
      <p className="text-muted-foreground">Frontend scaffold is alive.</p>
      <Button>shadcn button</Button>
    </div>
  )
}
