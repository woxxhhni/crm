'use client';

import { QueryClient, QueryClientProvider, QueryCache, MutationCache } from '@tanstack/react-query';
import React, { useEffect, useCallback } from 'react';
import { App } from 'antd';
import { formatApiErrorMessage } from './api-error';
import { FORBIDDEN_ERROR_EVENT } from './http-client.service';

// Event-based error notification system
const ERROR_EVENT = 'app:show-error';

function isForbiddenError(error: unknown): boolean {
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as { response?: { status?: number } };
    return axiosError.response?.status === 403;
  }
  return false;
}

function dispatchError(error: unknown) {
  if (typeof window !== 'undefined') {
    // Skip 403 errors - they are handled by FORBIDDEN_ERROR_EVENT
    if (isForbiddenError(error)) {
      return;
    }
    window.dispatchEvent(new CustomEvent(ERROR_EVENT, { detail: error }));
  }
}

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error, query) => {
      if (query.state.data !== undefined || query.meta?.showErrorNotification) {
        dispatchError(error);
      }
    },
  }),
  mutationCache: new MutationCache({
    onError: (error, _variables, _context, mutation) => {
      if (mutation.meta?.showErrorNotification !== false) {
        dispatchError(error);
      }
    },
  }),
  defaultOptions: {
    queries: {
      staleTime: 0,
      refetchOnWindowFocus: false,
      refetchOnMount: true,
      refetchOnReconnect: false,
      retry: false,
    },
    mutations: {
      retry: false,
    },
  },
});

function GlobalErrorListener() {
  const { notification } = App.useApp();

  const handleError = useCallback(
    (event: CustomEvent) => {
      notification.error({
        message: 'Error',
        description: formatApiErrorMessage(event.detail),
      });
    },
    [notification]
  );

  const handleForbiddenError = useCallback(
    (event: CustomEvent) => {
      notification.error({
        message: 'Access Denied',
        description: event.detail || 'You do not have permission to perform this action',
      });
    },
    [notification]
  );

  useEffect(() => {
    window.addEventListener(ERROR_EVENT, handleError as EventListener);
    window.addEventListener(FORBIDDEN_ERROR_EVENT, handleForbiddenError as EventListener);
    return () => {
      window.removeEventListener(ERROR_EVENT, handleError as EventListener);
      window.removeEventListener(FORBIDDEN_ERROR_EVENT, handleForbiddenError as EventListener);
    };
  }, [handleError, handleForbiddenError]);

  return null;
}

export function ReactQueryProvider({ children }: { children: React.ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      <GlobalErrorListener />
      {children}
    </QueryClientProvider>
  );
}
