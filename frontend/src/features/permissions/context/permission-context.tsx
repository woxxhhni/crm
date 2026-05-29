'use client';

import { createContext, useContext } from 'react';
import type { CurrentUser, UserRole } from '@/api/general/types';
import type { Permission } from '../constants/permissions';

export interface PermissionContextValue {
  user: CurrentUser | null;
  role: UserRole | null;
  loading: boolean;
  hasPermission: (permission: Permission) => boolean;
  hasAnyPermission: (permissions: Permission[]) => boolean;
  hasAllPermissions: (permissions: Permission[]) => boolean;
  refetchUser: () => Promise<void>;
}

export const PermissionContext = createContext<PermissionContextValue | null>(null);

export function usePermissionContext(): PermissionContextValue {
  const context = useContext(PermissionContext);
  if (!context) {
    throw new Error('usePermissionContext must be used within a PermissionProvider');
  }
  return context;
}
