'use client';

import 'reflect-metadata';
import { useState, useEffect, useCallback, useMemo } from 'react';
import { container } from '@/services/di-container';
import { GeneralService } from '@/features/general/services/general.service';
import type { CurrentUser } from '@/api/general/types';
import { PermissionContext } from './permission-context';
import { hasPermission as checkPermission, hasAnyPermission as checkAnyPermission, hasAllPermissions as checkAllPermissions, type Permission } from '../constants/permissions';
import { useAuthContext } from '@/features/auth/sign-in/hooks/use-auth-context';
import { REFETCH_USER_EVENT } from '@/services/http-client.service';

interface Props {
  children: React.ReactNode;
}

export function PermissionProvider({ children }: Props) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);
  const { authenticated } = useAuthContext();

  const fetchUser = useCallback(async () => {
    if (!authenticated) {
      setUser(null);
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const service = container.resolve(GeneralService);
      const currentUser = await service.getCurrentUser();
      setUser(currentUser);
    } catch (error) {
      console.error('Failed to fetch current user:', error);
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, [authenticated]);

  useEffect(() => {
    fetchUser();
  }, [fetchUser]);

  // Listen for refetch user event (triggered on 403 errors)
  useEffect(() => {
    const handleRefetchUser = () => {
      fetchUser();
    };

    window.addEventListener(REFETCH_USER_EVENT, handleRefetchUser);
    return () => {
      window.removeEventListener(REFETCH_USER_EVENT, handleRefetchUser);
    };
  }, [fetchUser]);

  const hasPermission = useCallback(
    (permission: Permission): boolean => {
      return checkPermission(user?.role, permission);
    },
    [user?.role]
  );

  const hasAnyPermission = useCallback(
    (permissions: Permission[]): boolean => {
      return checkAnyPermission(user?.role, permissions);
    },
    [user?.role]
  );

  const hasAllPermissions = useCallback(
    (permissions: Permission[]): boolean => {
      return checkAllPermissions(user?.role, permissions);
    },
    [user?.role]
  );

  const value = useMemo(
    () => ({
      user,
      role: user?.role ?? null,
      loading,
      hasPermission,
      hasAnyPermission,
      hasAllPermissions,
      refetchUser: fetchUser,
    }),
    [user, loading, hasPermission, hasAnyPermission, hasAllPermissions, fetchUser]
  );

  return <PermissionContext.Provider value={value}>{children}</PermissionContext.Provider>;
}
