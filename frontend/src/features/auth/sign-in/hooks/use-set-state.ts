'use client';
import { useState, useCallback } from 'react';

export function useSetState<T extends object>(initialState: T) {
    const [state, setInternalState] = useState<T>(initialState);

    const setState = useCallback((newState: Partial<T>) => {
        setInternalState((prev) => ({ ...prev, ...newState }));
    }, []);

    return { state, setState };
}
