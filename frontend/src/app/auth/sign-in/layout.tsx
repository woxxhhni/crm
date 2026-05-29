import { Suspense } from 'react';
import { Spin } from 'antd';
import { GuestGuard } from '@/features/auth/sign-in/guard';

type Props = {
    children: React.ReactNode;
};

export default function Layout({ children }: Props) {
    return (
        <Suspense fallback={<Spin fullscreen />}>
            <GuestGuard>
                {children}
            </GuestGuard>
        </Suspense>
    );
}
