'use client';

import 'reflect-metadata';
import { Avatar, Button, Typography, Card } from 'antd';
import { FilePreviewCard } from '@/components/file-preview/file-preview-card.component';
import { useQuery } from '@tanstack/react-query';
import { container } from 'tsyringe';
import { OrdersService } from '@/features/orders/services/orders.service';
import { useTranslate } from '@/locales/use-locales';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { useParams, useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import type { ActivityLog, StepWithLogs } from '@/api/orders/types';
import { useMemo, useState } from 'react';
import EditNoteModalComponent from './edit-note-modal.component';
import OrderActionModalComponent from './order-action-modal.component';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import {
  formatLogDate,
  isEditableNoteType,
  isEditableOrderStateType,
  getActionTypeFromLogType,
  isActivityNoteType,
} from '../utils/log-type-config';
import LogTypeBadge from './log-type-badge.component';
import { translateKnownLogTitle } from '../utils/workflow-labels';

function findLogInSteps(steps: StepWithLogs[], logId: number): { log: ActivityLog } | null {
  for (const step of steps) {
    const log = step.logs.find((l: ActivityLog) => l.id === logId);
    if (log) {
      return { log };
    }
  }
  return null;
}

export default function LogDetailPageComponent() {
  const { t } = useTranslate('orders');
  const service = container.resolve(OrdersService);
  const router = useRouter();
  const params = useParams();
  const { hasPermission } = usePermissionContext();

  // Permission checks
  const canEditNote = hasPermission(PERMISSIONS.NOTE_EDIT);
  const canChangeOrderStatus = hasPermission(PERMISSIONS.ORDER_CHANGE_STATUS);

  const orderId = params.orderId as string;
  const logId = params.logId as string;

  const [editNoteModalOpen, setEditNoteModalOpen] = useState(false);
  const [orderActionModalOpen, setOrderActionModalOpen] = useState(false);

  const { data: orderDetails, isLoading } = useQuery({
    queryKey: ['order-edit-details', orderId],
    queryFn: () => service.getDetails(orderId),
    enabled: !!orderId,
  });

  const logData = useMemo(() => {
    if (!orderDetails?.steps || !logId) return null;
    return findLogInSteps(orderDetails.steps, Number(logId));
  }, [orderDetails?.steps, logId]);

  // Determine if we need to fetch note details
  const shouldFetchNoteDetails = logData?.log && isActivityNoteType(logData.log.logType) && logData.log.noteId;

  // Fetch note details if this is a note-related log
  const { data: noteDetail, isLoading: isNoteLoading } = useQuery({
    queryKey: ['note-detail', orderId, logData?.log?.noteId],
    queryFn: () => service.getNoteDetail(orderId, logData!.log.noteId!),
    enabled: !!shouldFetchNoteDetails,
  });

  if (isLoading || (shouldFetchNoteDetails && isNoteLoading)) {
    return <LoadingSpinnerComponent />;
  }

  if (!logData) {
    return (
      <div className='flex flex-col items-center justify-center min-h-[400px]'>
        <Typography.Text type='secondary'>{t('logNotFound')}</Typography.Text>
        <Button type='link' onClick={() => router.push(paths.panel.orderDetails(orderId))}>
          {t('backToOrderDetails')}
        </Button>
      </div>
    );
  }

  const { log } = logData;

  // Use files from noteDetail if available, otherwise from log
  const displayFiles = noteDetail?.files || log.files || [];
  const hasFiles = displayFiles.length > 0 && displayFiles.some((f) => f.url);

  // Determine edit capability (check both log type and permissions)
  const isEditableNote = isEditableNoteType(log.logType) && log.noteId;
  const isEditableOrderState = isEditableOrderStateType(log.logType);
  const isOrderInProgress = orderDetails?.status?.toLowerCase() === 'inprogress';
  const canEditNoteLog = isEditableNote && canEditNote && isOrderInProgress;
  const canEditOrderStateLog = isEditableOrderState && canChangeOrderStatus && isOrderInProgress;
  const canEdit = canEditNoteLog || canEditOrderStateLog;

  // Get client and provider info from order details
  const clientName = orderDetails?.clientName;
  const providerName = orderDetails?.providerName;
  const clientEmail = orderDetails?.clientEmail;

  const handleEditClick = () => {
    if (canEditNoteLog) {
      setEditNoteModalOpen(true);
    } else if (canEditOrderStateLog) {
      setOrderActionModalOpen(true);
    }
  };

  return (
    <>
      <Card className='shadow-card bg-white'>
        {/* User Info Header */}
        <div className='flex justify-between items-start mb-6'>
          <div className='flex items-center gap-3'>
            <Avatar size={48} src={log.actorProfileUrl || undefined} className='bg-gray-200' />
            <div>
              <Typography.Text className='block font-semibold text-base'>{log.actorFullName || '--'}</Typography.Text>
              <Typography.Text className='!text-gray text-sm'>{formatLogDate(log.logDate)}</Typography.Text>
            </div>
          </div>
          {canEdit && (
            <Button type='default' className='!rounded-full !border-gray-300 !px-6' onClick={handleEditClick}>
              {t('edit')}
            </Button>
          )}
        </div>

        {/* Log Title with Badge */}
        <div className='flex items-center gap-3 mb-4'>
          <Typography.Text className='font-semibold text-base'>{translateKnownLogTitle(log.title, t)}</Typography.Text>
          <LogTypeBadge logType={log.logType} />
        </div>

        {/* Description */}
        <div className='mb-6'>
          <Typography.Paragraph className='!text-gray !mb-0 whitespace-pre-wrap leading-relaxed'>
            {translateKnownLogTitle(noteDetail?.description || log.description, t)}
          </Typography.Paragraph>
        </div>

        {/* Client/Provider Info */}
        {(clientName || providerName) && (
          <div className='flex gap-16 mt-6'>
            {clientName && (
              <div>
                <Typography.Text className='!text-gray-400 block text-sm mb-1'>{t('client')}</Typography.Text>
                <Typography.Text className='font-medium'>{clientName}</Typography.Text>
              </div>
            )}
            {providerName && (
              <div>
                <Typography.Text className='!text-gray-400 block text-sm mb-1'>{t('provider')}</Typography.Text>
                <Typography.Text className='font-medium'>{providerName}</Typography.Text>
              </div>
            )}
          </div>
        )}

        {/* Files/Attachments */}
        {hasFiles && (
          <div className='mt-6'>
            <Typography.Text className='!text-gray-400 block text-sm mb-2'>{t('attachments')}</Typography.Text>
            <div className='flex flex-wrap gap-3'>
              {displayFiles
                .filter((f) => f.url)
                .map((file) => (
                  <FilePreviewCard key={file.id} file={{ id: file.id, fileName: file.name || t('file'), url: file.url! }} />
                ))}
            </div>
          </div>
        )}
      </Card>

      {/* Edit Note Modal */}
      {canEditNoteLog && <EditNoteModalComponent open={editNoteModalOpen} onClose={() => setEditNoteModalOpen(false)} orderId={orderId} noteId={log.noteId!} noteDetail={noteDetail} />}

      {/* Order Action Modal */}
      {canEditOrderStateLog && (
        <OrderActionModalComponent
          open={orderActionModalOpen}
          onClose={() => setOrderActionModalOpen(false)}
          orderId={orderId}
          actionType={getActionTypeFromLogType(log.logType)}
        />
      )}
    </>
  );
}
