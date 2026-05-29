'use client';

import { useEffect } from 'react';

import { useRouter } from '@/routes/hooks';
import {CONFIG} from "../../global-config";

// ----------------------------------------------------------------------

export default function Page() {
  const router = useRouter();

  useEffect(() => {
    router.push(CONFIG.auth.redirectPath);
  }, [router]);

  return null;
}
