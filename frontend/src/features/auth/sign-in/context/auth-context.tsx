'use client';

import { createContext } from 'react';


// ----------------------------------------------------------------------
export type AuthContextValue = {
    user: any;
    loading: boolean;
    authenticated: boolean;
    unauthenticated: boolean;
    checkUserSession?: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
