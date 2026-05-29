'use client';

import { usePermissionContext } from '../context/permission-context';
import type { Permission } from '../constants/permissions';

export function usePermission(permission: Permission): boolean {
  const { hasPermission } = usePermissionContext();
  return hasPermission(permission);
}

export function useAnyPermission(permissions: Permission[]): boolean {
  const { hasAnyPermission } = usePermissionContext();
  return hasAnyPermission(permissions);
}

export function useAllPermissions(permissions: Permission[]): boolean {
  const { hasAllPermissions } = usePermissionContext();
  return hasAllPermissions(permissions);
}

export function useCurrentUser() {
  const { user, loading, refetchUser } = usePermissionContext();
  return { user, loading, refetchUser };
}

export function useRole() {
  const { role, loading } = usePermissionContext();
  return { role, loading };
}
