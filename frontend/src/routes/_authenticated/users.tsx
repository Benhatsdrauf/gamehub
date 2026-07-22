import { useMemo } from 'react'
import { createFileRoute, redirect } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import { toast } from 'sonner'
import { getUsers, promoteUser } from '@/lib/api/users'
import type { UserListItem } from '@/lib/api/schemas'
import { ApiError } from '@/lib/api/error'
import { Button } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

export const Route = createFileRoute('/_authenticated/users')({
  // Layer an admin check on top of the parent's "must be authenticated" guard.
  beforeLoad: ({ context }) => {
    if (!context.auth.isAdmin) {
      throw redirect({ to: '/' })
    }
  },
  component: UsersPage,
})

function UsersPage() {
  const queryClient = useQueryClient()

  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: () => getUsers(1, 20),
  })

  const promote = useMutation({
    mutationFn: promoteUser,
    onSuccess: () => {
      toast.success('User promoted to admin')
      void queryClient.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (error) => {
      toast.error(error instanceof ApiError ? error.message : 'Failed to promote user')
    },
  })

  const columns = useMemo<ColumnDef<UserListItem>[]>(
    () => [
      { accessorKey: 'username', header: 'Username' },
      { accessorKey: 'email', header: 'Email' },
      {
        accessorKey: 'id',
        header: 'ID',
        cell: (info) => <span className="font-mono text-xs">{String(info.getValue())}</span>,
      },
      {
        id: 'actions',
        header: '',
        cell: ({ row }) => (
          <div className="text-right">
            <Button
              size="sm"
              variant="outline"
              disabled={promote.isPending}
              onClick={() => promote.mutate(row.original.id)}
            >
              Promote
            </Button>
          </div>
        ),
      },
    ],
    [promote],
  )

  const table = useReactTable({
    data: usersQuery.data?.items ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-muted-foreground text-sm">
          {usersQuery.data ? `${usersQuery.data.totalCount} total` : 'Loading…'}
        </p>
      </div>

      {usersQuery.isError && (
        <p className="text-destructive text-sm">
          {usersQuery.error instanceof ApiError
            ? usersQuery.error.message
            : 'Failed to load users.'}
        </p>
      )}

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {usersQuery.isPending ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="text-muted-foreground h-24 text-center">
                  Loading…
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={columns.length} className="text-muted-foreground h-24 text-center">
                  No users.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}
