'use client';

import { useState, useEffect } from 'react';
import { useRouter, usePathname } from '@/routes/hooks';
import { useAuthContext } from '../hooks';
import { Spin } from 'antd';
import { paths } from '@/routes/paths';
import {CONFIG} from "../../../../../global-config";

// ----------------------------------------------------------------------

type AuthGuardProps = {
  children: React.ReactNode;
};

export function AuthGuard({ children }: AuthGuardProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { authenticated, loading } = useAuthContext();

  const [isChecking, setIsChecking] = useState(true);

  const createRedirectPath = (targetPath: string) => {
    const params = new URLSearchParams({ returnTo: pathname }).toString();
    return `${targetPath}?${params}`;
  };

  const checkPermissions = async (): Promise<void> => {
    if (loading) return;

    if (!authenticated) {
      const redirectPath = createRedirectPath(paths.auth.signIn);
      router.replace(redirectPath);
      return;
    }

    if (authenticated && pathname === paths.auth.signIn) {
      router.replace(paths.panel.clients);
      return;
    }

    setIsChecking(false);
  };

  useEffect(() => {
    checkPermissions();
  }, [authenticated, loading, pathname]);

  if (isChecking) {
    return <Spin fullscreen />;
  }

  return <>{children}</>;
}
