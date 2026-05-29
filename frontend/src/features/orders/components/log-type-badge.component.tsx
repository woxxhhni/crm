import { getLogTypeConfig } from '../utils/log-type-config';
import { useTranslate } from '@/locales/use-locales';

interface LogTypeBadgeProps {
  logType: string;
  variant?: 'filled' | 'outlined';
}

export default function LogTypeBadge({ logType, variant = 'filled' }: LogTypeBadgeProps) {
  const { t } = useTranslate('orders');
  const config = getLogTypeConfig(logType);
  const labelKeyMap: Record<string, string> = {
    noteadded: 'noteAdded',
    noteedited: 'noteEdited',
    noteremoved: 'noteRemoved',
    statuscomplete: 'stepCompleted',
    statusforward: 'stepForward',
    statusbackward: 'stepBackward',
    ordercompleted: 'orderCompleted',
    ordercanceled: 'orderCanceled',
    ordersuspended: 'orderSuspended',
    orderreturnedtoprogress: 'orderReturnedToProgress',
    ordercreated: 'orderCreated',
    orderedited: 'orderEdited',
    employeeassigned: 'employeeAssigned',
    employeeremoved: 'employeeRemoved',
    clientpaymentadded: 'clientPaymentAdded',
    clientpaymentedited: 'clientPaymentEdited',
    clientpaymentremoved: 'clientPaymentDeleted',
    providerpaymentadded: 'providerPaymentAdded',
    providerpaymentedited: 'providerPaymentEdited',
    providerpaymentremoved: 'providerPaymentDeleted',
    uniquenumberadded: 'uniqueNumberAdded',
    uniquenumberremoved: 'uniqueNumberRemoved',
  };
  const labelKey = labelKeyMap[logType.toLowerCase()];

  const baseClasses = 'inline-block text-xs rounded-full font-medium';
  const paddingClasses = variant === 'outlined' ? 'px-3 py-0.5' : 'px-2.5 py-0.5';

  const style =
    variant === 'outlined'
      ? {
          color: config.color,
          borderColor: config.borderColor,
          backgroundColor: '#ffffff',
        }
      : {
          backgroundColor: config.bgColor,
          color: config.color,
        };

  const borderClass = variant === 'outlined' ? 'border' : '';

  return (
    <span className={`${baseClasses} ${paddingClasses} ${borderClass}`} style={style}>
      {labelKey ? t(labelKey) : config.label}
    </span>
  );
}
