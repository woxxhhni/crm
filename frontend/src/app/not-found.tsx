'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { CONFIG } from '../../global-config';

export default function NotFound() {
  const router = useRouter();

  useEffect(() => {
    router.replace(CONFIG.auth.redirectPath);
  }, [router]);

  return null;
}
