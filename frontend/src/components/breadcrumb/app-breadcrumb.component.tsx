'use client';

import { Breadcrumb } from 'antd';
import Link from 'next/link';
import { useTranslate } from '@/locales/use-locales';

interface OrderDetails {
  title?: string;
  sellCurrency?: string;
  buyCurrency?: string;
}

interface EntityDetails {
  name?: string;
}

interface LogDetails {
  title?: string;
}

interface PaymentDetails {
  amount?: number;
  paymentType?: string;
}

interface ExtraProviderDetails {
  name?: string;
}

interface AppBreadcrumbProps {
  pathname: string;
  orderDetails?: OrderDetails;
  clientDetails?: EntityDetails;
  providerDetails?: EntityDetails;
  employeeDetails?: EntityDetails;
  logDetails?: LogDetails | null;
  paymentDetails?: PaymentDetails;
  extraProviderDetails?: ExtraProviderDetails;
  isClientPaymentPage?: boolean;
}

export const AppBreadcrumb = ({
  pathname,
  orderDetails,
  clientDetails,
  providerDetails,
  employeeDetails,
  logDetails,
  paymentDetails,
  extraProviderDetails,
  isClientPaymentPage,
}: AppBreadcrumbProps) => {
  const { t } = useTranslate('common');

  const segmentLabels: Record<string, string> = {
    dashboard: t('dashboard'),
    orders: t('orders'),
    clients: t('clients'),
    providers: t('providers'),
    employees: t('employees'),
    backups: t('backups'),
    logs: t('logs'),
    'client-payments': t('clientPayments'),
    'provider-payments': t('providerPayments'),
    'extra-providers': t('extraProviders'),
  };

  // Helper to get entity title by type and ID
  const getEntityTitle = (entityType: string, id: string) => {
    switch (entityType) {
      case 'orders':
        return orderDetails?.title || `${t('order')} ${id}`;
      case 'clients':
        return clientDetails?.name || `${t('client')} ${id}`;
      case 'providers':
        return providerDetails?.name || `${t('provider')} ${id}`;
      case 'employees':
        return employeeDetails?.name || `${t('employee')} ${id}`;
      case 'logs':
        return logDetails?.title || `${t('log')} ${id}`;
      case 'extra-providers':
        return extraProviderDetails?.name || `${t('provider')} ${id}`;
      case 'client-payments':
      case 'provider-payments':
        if (paymentDetails?.amount) {
          const currency = isClientPaymentPage ? orderDetails?.sellCurrency : orderDetails?.buyCurrency;
          return `${paymentDetails.amount} ${currency || ''}`.trim();
        }
        return `${t('payment')} ${id}`;
      default:
        return id;
    }
  };

  // Build breadcrumb items dynamically
  const buildBreadcrumbItems = () => {
    let pathParts = pathname?.split('/').filter(Boolean) || [];
    pathParts = pathParts.filter((p) => p.toLowerCase() !== 'panel');

    // Segments that don't have list pages (only detail pages)
    const nonClickableSegments = ['logs', 'client-payments', 'provider-payments', 'extra-providers'];

    // Entity types that use /details/[id] pattern
    const detailsPatternEntities = ['orders', 'clients', 'providers', 'employees'];

    const items: Array<{ key: string; title: React.ReactElement; displayText: string }> = [];
    let actualPath = '/panel'; // The actual navigable path

    for (let i = 0; i < pathParts.length; i++) {
      const part = pathParts[i];
      const prevPart = i > 0 ? pathParts[i - 1] : null;
      const isNumeric = /^[0-9]+$/.test(part);
      const isLastItem = i === pathParts.length - 1;

        let displayText = segmentLabels[part] || part.replace(/-/g, ' ');
      let shouldSkip = false;
      let isClickable = true;

      // Skip "details" segment (redundant)
      if (part === 'details') {
        actualPath += '/details';
        shouldSkip = true;
        continue;
      }

      // Handle numeric IDs
      if (isNumeric) {
        // Get the entity type from context
        let entityType = prevPart || '';

        // If previous part is 'details', get the actual entity type from grandparent
        if (prevPart === 'details') {
          const grandParent = i > 1 ? pathParts[i - 2] : null;
          entityType = grandParent || '';
        }

        // Get display title
        displayText = getEntityTitle(entityType, part);

        // Build the correct path for this entity
        if (detailsPatternEntities.includes(entityType)) {
          // Entities that use /entity/details/[id] pattern
          actualPath = `/panel/${entityType}/details/${part}`;
        } else {
          // Nested routes like /orders/[id]/logs/[id]
          actualPath += `/${part}`;
        }
      } else {
        // Non-numeric segments
        actualPath += `/${part}`;

        // Determine if this segment should be clickable
        if (nonClickableSegments.includes(part)) {
          isClickable = false;
        }
      }

      if (!shouldSkip) {
        items.push({
          key: `${part}-${i}`,
          displayText,
          title: isLastItem || !isClickable ? (
            <span className="font-medium capitalize text-sm" style={{ color: 'var(--text)' }}>{displayText}</span>
          ) : (
            <Link href={actualPath} className="capitalize text-sm hover:opacity-80" style={{ color: 'var(--text-muted)' }}>
              {displayText}
            </Link>
          ),
        });
      }
    }

    return items;
  };

  const breadcrumbItems = buildBreadcrumbItems();
  const showBreadcrumb = breadcrumbItems.length >= 2;

  // Detail pages: these have their own card headers, so we only show the breadcrumb trail (no duplicate title)
  const isDetailPage = breadcrumbItems.length >= 2;

  // For list pages (e.g. /panel/orders), show the page title as heading
  const pageTitle = breadcrumbItems.length > 0 ? breadcrumbItems[breadcrumbItems.length - 1].displayText : '';

  return (
    <div className="flex flex-col gap-[2px]">
      {showBreadcrumb && (
        <Breadcrumb separator=" / " items={breadcrumbItems} className="text-sm" />
      )}

      {/* Only show standalone title on list pages (1 breadcrumb item), not on detail pages */}
      {!isDetailPage && pageTitle && (
        <span className="font-semibold text-base capitalize" style={{ color: 'var(--text)' }}>
          {pageTitle}
        </span>
      )}
    </div>
  );
};
