'use client';

import ExtraProviderDetailsComponent from '@/features/orders/components/extra-provider-details.component';
import { useParams } from 'next/navigation';

export default function ExtraProviderDetailsPage() {
    const params = useParams();
    const orderId = typeof params?.orderId === 'string' ? params.orderId : '';
    const epId = typeof params?.epId === 'string' ? params.epId : '';

    if (!orderId || !epId) return null;

    return <ExtraProviderDetailsComponent orderId={orderId} epId={epId} />;
}
