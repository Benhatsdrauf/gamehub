import { useMemo, useState } from 'react'
import { createFileRoute, redirect } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import { useForm } from '@tanstack/react-form'
import * as z from 'zod'
import { toast } from 'sonner'
import { deleteUser, getUsers, promoteUser, updateUser } from '@/lib/api/users'
import type { UserListItem } from '@/lib/api/schemas'
import { ApiError } from '@/lib/api/error'
import { useAuth } from '@/lib/auth/AuthProvider'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'

export const Route = createFileRoute('/_authenticated/users')({
  beforeLoad: ({ context }) => {
    if (!context.auth.isAdmin) {
      throw redirect({ to: '/' })
    }
  },
  component: UsersPage,
})

const editSchema = z.object({
  username: z.string().min(1, 'Username is required').max(50, 'Max 50 characters'),
  email: z.email('Enter a valid email'),
})

function UsersPage() {
  const queryClient = useQueryClient()
  const { user } = useAuth()
  const currentUserId = user?.id

  const [editUser, setEditUser] = useState<UserListItem | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<UserListItem | null>(null)

  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: () => getUsers(1, 20),
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['users'] })

  const promote = useMutation({
    mutationFn: promoteUser,
    onSuccess: () => {
      toast.success('User promoted to admin')
      void invalidate()
    },
    onError: (error) =>
      toast.error(error instanceof ApiError ? error.message : 'Failed to promote user'),
  })

  const remove = useMutation({
    mutationFn: deleteUser,
    onSuccess: () => {
      toast.success('User deleted')
      setDeleteTarget(null)
      void invalidate()
    },
    onError: (error) =>
      toast.error(error instanceof ApiError ? error.message : 'Failed to delete user'),
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
        cell: ({ row }) => {
          const isSelf = row.original.id === currentUserId
          return (
            <div className="flex justify-end gap-2">
              <Button size="sm" variant="outline" onClick={() => setEditUser(row.original)}>
                Edit
              </Button>
              <Button
                size="sm"
                variant="outline"
                disabled={promote.isPending}
                onClick={() => promote.mutate(row.original.id)}
              >
                Promote
              </Button>
              <Button
                size="sm"
                variant="destructive"
                disabled={isSelf}
                title={isSelf ? "You can't delete your own account" : undefined}
                onClick={() => setDeleteTarget(row.original)}
              >
                Delete
              </Button>
            </div>
          )
        },
      },
    ],
    [currentUserId, promote],
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

      {editUser && (
        <EditUserDialog
          key={editUser.id}
          user={editUser}
          onClose={() => setEditUser(null)}
          onSaved={() => {
            setEditUser(null)
            void invalidate()
          }}
        />
      )}

      <AlertDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete user?</AlertDialogTitle>
            <AlertDialogDescription>
              This permanently deletes{' '}
              <span className="font-medium">{deleteTarget?.username}</span> (
              {deleteTarget?.email}). This cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              disabled={remove.isPending}
              onClick={() => {
                if (deleteTarget) remove.mutate(deleteTarget.id)
              }}
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

function EditUserDialog({
  user,
  onClose,
  onSaved,
}: {
  user: UserListItem
  onClose: () => void
  onSaved: () => void
}) {
  const [formError, setFormError] = useState<string | null>(null)

  const save = useMutation({
    mutationFn: (data: { username: string; email: string }) => updateUser(user.id, data),
    onSuccess: () => {
      toast.success('User updated')
      onSaved()
    },
    onError: (error) =>
      setFormError(error instanceof ApiError ? error.message : 'Failed to update user'),
  })

  const form = useForm({
    defaultValues: { username: user.username, email: user.email },
    validators: { onSubmit: editSchema },
    onSubmit: ({ value }) => {
      setFormError(null)
      save.mutate(value)
    },
  })

  return (
    <Dialog
      open
      onOpenChange={(open) => {
        if (!open) onClose()
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit user</DialogTitle>
          <DialogDescription>Update the username and email.</DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            void form.handleSubmit()
          }}
        >
          <FieldGroup>
            <form.Field name="username">
              {(field) => {
                const invalid = field.state.meta.isTouched && !field.state.meta.isValid
                return (
                  <Field data-invalid={invalid}>
                    <FieldLabel htmlFor="edit-username">Username</FieldLabel>
                    <Input
                      id="edit-username"
                      value={field.state.value}
                      onBlur={field.handleBlur}
                      onChange={(e) => field.handleChange(e.target.value)}
                      aria-invalid={invalid}
                    />
                    {invalid && <FieldError errors={field.state.meta.errors} />}
                  </Field>
                )
              }}
            </form.Field>

            <form.Field name="email">
              {(field) => {
                const invalid = field.state.meta.isTouched && !field.state.meta.isValid
                return (
                  <Field data-invalid={invalid}>
                    <FieldLabel htmlFor="edit-email">Email</FieldLabel>
                    <Input
                      id="edit-email"
                      type="email"
                      value={field.state.value}
                      onBlur={field.handleBlur}
                      onChange={(e) => field.handleChange(e.target.value)}
                      aria-invalid={invalid}
                    />
                    {invalid && <FieldError errors={field.state.meta.errors} />}
                  </Field>
                )
              }}
            </form.Field>

            {formError && <p className="text-destructive text-sm">{formError}</p>}

            <DialogFooter>
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button type="submit" disabled={save.isPending}>
                {save.isPending ? 'Saving…' : 'Save'}
              </Button>
            </DialogFooter>
          </FieldGroup>
        </form>
      </DialogContent>
    </Dialog>
  )
}
