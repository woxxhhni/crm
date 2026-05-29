'use client';

import { Typography, Empty, Avatar, Checkbox, Button, Image, Tag, Tooltip, Select } from 'antd';
import {
  FileOutlined, ArrowRightOutlined, UserOutlined,
  ClockCircleOutlined, EditOutlined, CheckCircleOutlined,
  SwapOutlined, PlusOutlined, MessageOutlined, TeamOutlined,
} from '@ant-design/icons';
import { useTranslate } from '@/locales/use-locales';
import type { StepWithLogs, ActivityLog, OrderStageAssignee } from '@/api/orders/types';
import { useState, useMemo, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { paths } from '@/routes/paths';
import AddNoteModalComponent from './add-note-modal.component';
import CompleteStepModalComponent from './complete-step-modal.component';
import ChangeStepModalComponent from './change-step-modal.component';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import OrderTrackingProgressComponent from './order-tracking-progress.component';
import { ACTIVITY_NOTE_TYPES, STATUS_CHANGE_TYPES, STEP_CHANGE_TYPES, formatLogDate, getLogTypeConfig } from '../utils/log-type-config';
import LogTypeBadge from './log-type-badge.component';
import { translateKnownLogTitle, translateWorkflowLabel } from '../utils/workflow-labels';
import { defaultSelectFilter } from '@/utils/select-utils';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

const { Text, Title } = Typography;

function getLatestLogTime(logs: ActivityLog[]): number {
  if (!logs?.length) return 0;
  return Math.max(...logs.map((log) => (log.logDate ? dayjs(log.logDate).valueOf() : 0)));
}

function isActiveCurrentStep(step: StepWithLogs, currentStepId?: number) {
  return !step.isCompleted && (step.isCurrent || step.stepId === currentStepId);
}

/** Keep the workflow order; logs newest-first within each step. */
function sortStepsForActivityDisplay(steps: StepWithLogs[], currentStepId?: number): StepWithLogs[] {
  return [...steps]
    .sort((a, b) => {
      const aPosition = a.stepOrderPosition ?? 0;
      const bPosition = b.stepOrderPosition ?? 0;
      if (aPosition !== bPosition) return aPosition - bPosition;
      return getLatestLogTime(b.logs) - getLatestLogTime(a.logs);
    })
    .map((step) => ({
      ...step,
      logs: [...(step.logs ?? [])].sort(
        (a, b) => dayjs(b.logDate).valueOf() - dayjs(a.logDate).valueOf(),
      ),
    }));
}

function getMaxReachedPosition(steps: StepWithLogs[]) {
  return Math.max(
    0,
    ...steps
      .filter((step) => step.isCompleted || step.isCurrent || step.stepEnteredAt || step.logs?.length)
      .map((step) => step.stepOrderPosition ?? 0),
  );
}

function isSkippedStep(step: StepWithLogs, currentStepId: number | undefined, maxReachedPosition: number) {
  return (
    !step.isCompleted &&
    !isActiveCurrentStep(step, currentStepId) &&
    !step.stepEnteredAt &&
    !(step.logs?.length) &&
    (step.stepOrderPosition ?? 0) < maxReachedPosition
  );
}

function shouldShowStepDetails(
  step: StepWithLogs,
  currentStepId: number | undefined,
  isSkipped: boolean,
  hasOriginalLogs: boolean,
  isFilteringLogs: boolean,
) {
  if (isFilteringLogs) {
    return !!(step.logs?.length);
  }

  return (
    step.isCompleted ||
    isActiveCurrentStep(step, currentStepId) ||
    !!step.stepEnteredAt ||
    !!step.stepExitedAt ||
    !!(step.logs?.length) ||
    hasOriginalLogs ||
    isSkipped
  );
}

interface Props {
  steps: StepWithLogs[];
  orderId: string | number;
  orderStatus?: string;
  currentStepId?: number;
  currentStepName?: string;
  currentStageName?: string;
  stageAssignments?: OrderStageAssignee[];
  employeeOptions?: { label: string; value: number | string }[];
  employeeOptionsLoading?: boolean;
  canAssignStageAssignee?: boolean;
  updatingStageId?: number | null;
  onStageAssigneeChange?: (stageId: number, employeeId: number | null) => void;
}

type FilterType = 'all' | 'activityNote' | 'statusChangeLogs' | 'stepChangeLogs';

/* ─── Filter pill colors ─── */
const FILTER_COLORS: Record<FilterType, { bg: string; activeBg: string; color: string; activeColor: string; border: string }> = {
  all:              { bg: '#F8FAFC', activeBg: '#1B3A5C', color: '#64748B', activeColor: '#fff', border: '#E2E8F0' },
  activityNote:     { bg: '#F8FAFC', activeBg: '#EFF6FF', color: '#64748B', activeColor: '#2563EB', border: '#BFDBFE' },
  statusChangeLogs: { bg: '#F8FAFC', activeBg: '#FEF2F2', color: '#64748B', activeColor: '#DC2626', border: '#FECACA' },
  stepChangeLogs:   { bg: '#F8FAFC', activeBg: '#F0FDF4', color: '#64748B', activeColor: '#16A34A', border: '#BBF7D0' },
};

/* ─── Log icon by category ─── */
function getLogIcon(logType: string) {
  const lt = logType.toLowerCase();
  if (lt === 'employeeassigned') return <UserOutlined style={{ fontSize: 14 }} />;
  if (ACTIVITY_NOTE_TYPES.includes(lt)) return <MessageOutlined style={{ fontSize: 14 }} />;
  if (STATUS_CHANGE_TYPES.includes(lt)) return <CheckCircleOutlined style={{ fontSize: 14 }} />;
  if (STEP_CHANGE_TYPES.includes(lt)) return <SwapOutlined style={{ fontSize: 14 }} />;
  return <ClockCircleOutlined style={{ fontSize: 14 }} />;
}

/* ─── Log Item ─── */
function LogItem({ log, orderId, onClick }: { log: ActivityLog; orderId: string | number; onClick: () => void }) {
  const { t } = useTranslate('orders');
  const hasFiles = log.files && log.files.length > 0 && log.files.some((f) => f.url);
  const config = getLogTypeConfig(log.logType);
  const lt = log.logType.toLowerCase();
  const showStepTransition =
    (lt === 'statusforward' || lt === 'statusbackward') &&
    log.fromStepName &&
    log.toStepName &&
    log.fromStepName !== log.toStepName;
  const description = translateKnownLogTitle(log.description, t);

  // Format date nicely
  const logDate = log.logDate ? dayjs(log.logDate) : null;
  const dateDisplay = logDate ? logDate.format('MMM DD, YYYY') : '—';
  const timeDisplay = logDate ? logDate.format('h:mm A') : '';

  return (
    <div
      style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
        padding: '16px 20px', background: '#fff', border: '1px solid #F1F5F9',
        borderRadius: 12, marginBottom: 12, cursor: 'pointer',
        transition: 'all 0.2s ease',
      }}
      className='hover:border-blue-300 hover:shadow-sm'
      onClick={onClick}
    >
      {/* Left: icon + content */}
      <div style={{ display: 'flex', gap: 14, flex: 1 }}>
        <div
          style={{
            width: 36, height: 36, borderRadius: 10,
            background: config.bgColor + '20',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: config.bgColor, flexShrink: 0, marginTop: 2,
          }}
        >
          {getLogIcon(log.logType)}
        </div>
        <div style={{ flex: 1, minWidth: 0 }}>
          {/* Title + badge row */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6, flexWrap: 'wrap' }}>
            <Text style={{ fontWeight: 500, fontSize: 14, color: '#0F172A' }}>{translateKnownLogTitle(log.title, t)}</Text>
            <LogTypeBadge logType={log.logType} />
          </div>
          {/* Step transition detail */}
          {showStepTransition && (
            <div style={{
              display: 'inline-flex', alignItems: 'center', gap: 6,
              background: '#F0FDF4', borderRadius: 6, padding: '3px 10px', marginBottom: 6,
              border: '1px solid #BBF7D0',
            }}>
              <SwapOutlined style={{ color: '#16A34A', fontSize: 11 }} />
              <Text style={{ fontSize: 12, color: '#16A34A', fontWeight: 500 }}>{translateWorkflowLabel(log.fromStepName, t)}</Text>
              <ArrowRightOutlined style={{ color: '#16A34A', fontSize: 9 }} />
              <Text style={{ fontSize: 12, color: '#16A34A', fontWeight: 600 }}>{translateWorkflowLabel(log.toStepName, t)}</Text>
            </div>
          )}
          {/* Description preview */}
          {description && (
            <Text style={{ color: '#64748B', fontSize: 13, display: 'block', marginBottom: 6, lineHeight: 1.5 }}>
              {description.length > 120 ? `${description.slice(0, 120)}…` : description}
            </Text>
          )}
          {/* Date + time */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <Text style={{ color: '#94A3B8', fontSize: 12 }}>
              <ClockCircleOutlined style={{ marginRight: 4, fontSize: 11 }} />
              {dateDisplay}
            </Text>
            {timeDisplay && (
              <Text style={{ color: '#CBD5E1', fontSize: 12 }}>
                {timeDisplay}
              </Text>
            )}
          </div>
          {/* Files */}
          {hasFiles && (
            <div style={{ display: 'flex', gap: 8, marginTop: 10, flexWrap: 'wrap' }}>
              {log.files
                .filter((f) => f.url)
                .map((file) => {
                  const isImage = file.url?.match(/\.(jpg|jpeg|png|gif|webp)(\?|$)/i);
                  return (
                    <div key={file.id}>
                      {isImage ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 6, background: '#F8FAFC', borderRadius: 6, padding: '4px 10px' }}>
                          <Image
                            src={file.url!}
                          alt={file.name || t('attachment')}
                            width={18}
                            height={18}
                            className='rounded object-cover'
                            style={{ width: 18, height: 18, objectFit: 'cover', borderRadius: 4 }}
                            preview={{ mask: null }}
                          />
                          <span style={{ fontSize: 12, color: '#64748B' }}>{file.name || t('image')}</span>
                        </div>
                      ) : (
                        <a
                          href={file.url!} target='_blank' rel='noopener noreferrer'
                          style={{
                            display: 'flex', alignItems: 'center', gap: 6,
                            background: '#F8FAFC', borderRadius: 6, padding: '4px 10px',
                            color: '#64748B', textDecoration: 'none', fontSize: 12,
                          }}
                        >
                          <FileOutlined style={{ color: '#94A3B8', fontSize: 12 }} />
                          {file.name || t('file')}
                        </a>
                      )}
                    </div>
                  );
                })}
            </div>
          )}
        </div>
      </div>

      {/* Right: actor */}
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4, marginLeft: 16, flexShrink: 0 }}>
        <Text style={{ fontSize: 10, color: '#CBD5E1', lineHeight: 1 }}>{t('logActor')}</Text>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <Avatar
            size={30}
            src={log.actorProfileUrl || undefined}
            icon={<UserOutlined />}
            style={{ backgroundColor: '#3B82F6' }}
          />
          <Text style={{ fontSize: 12, color: '#64748B', whiteSpace: 'nowrap' }}>{log.actorFullName}</Text>
        </div>
      </div>
    </div>
  );
}

/* ─── Stage milestone divider ─── */
function StageMilestone({
  stageId,
  stageName,
  stepCount,
  assignee,
  canAssign,
  employeeOptions,
  employeeOptionsLoading,
  isUpdating,
  onAssigneeChange,
}: {
  stageId: number;
  stageName: string;
  stepCount: number;
  assignee?: OrderStageAssignee;
  canAssign?: boolean;
  employeeOptions?: { label: string; value: number | string }[];
  employeeOptionsLoading?: boolean;
  isUpdating?: boolean;
  onAssigneeChange?: (stageId: number, employeeId: number | null) => void;
}) {
  const { t } = useTranslate('orders');
  const assigneeName = assignee?.employeeName;

  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 12, flexWrap: 'wrap',
      margin: '28px 0 16px',
    }}>
      <div style={{ flex: 1, height: 1, background: 'linear-gradient(to right, transparent, #CBD5E1)' }} />
      <div style={{
        display: 'flex', flexDirection: 'column', alignItems: 'center',
        padding: '8px 20px', borderRadius: 10,
        background: '#F8FAFC', border: '1px solid #E2E8F0',
      }}>
        <Text style={{ fontWeight: 700, fontSize: 13, color: '#0F172A', letterSpacing: 0.2 }}>
          {translateWorkflowLabel(stageName, t)}
        </Text>
        <Text style={{ fontSize: 11, color: '#94A3B8', marginTop: 2 }}>
          {t('trackingStageSteps', { count: stepCount })}
        </Text>
      </div>
      <div style={{ flex: 1, height: 1, background: 'linear-gradient(to left, transparent, #CBD5E1)' }} />
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          minWidth: 220,
          justifyContent: 'flex-end',
        }}
      >
        <TeamOutlined style={{ color: '#64748B', fontSize: 14 }} />
        <Text style={{ color: '#64748B', fontSize: 12, whiteSpace: 'nowrap' }}>
          {t('stageResponsible')}
        </Text>
        {canAssign ? (
          <Select
            size='small'
            allowClear
            showSearch
            filterOption={defaultSelectFilter}
            placeholder={t('noStageResponsible')}
            value={assignee?.employeeId ?? undefined}
            loading={employeeOptionsLoading || isUpdating}
            disabled={isUpdating}
            style={{ width: 180 }}
            options={employeeOptions || []}
            onChange={(value) => onAssigneeChange?.(stageId, value == null ? null : Number(value))}
          />
        ) : (
          <Tag
            style={{
              borderRadius: 6,
              margin: 0,
              color: assigneeName ? '#2563EB' : '#94A3B8',
              borderColor: assigneeName ? '#BFDBFE' : '#E2E8F0',
              background: assigneeName ? '#EFF6FF' : '#F8FAFC',
            }}
          >
            {assigneeName || t('noStageResponsible')}
          </Tag>
        )}
      </div>
    </div>
  );
}

/* ─── Step Section ─── */
function StepSection({
  stageName, stepName, logs, stepEnteredAt, stepExitedAt, isCurrent,
  orderId, onAddNote, onCompleteStep, onChangeStep, onLogClick, isCompleted, showActions, hasOriginalLogs,
  canChangeStep, canCompleteStep, canAddNote, stepId, isSkipped, isSelected,
}: {
  stageName: string; stepName: string; logs: ActivityLog[];
  stepEnteredAt?: string | null; stepExitedAt?: string | null; isCurrent?: boolean;
  orderId: string | number; onAddNote: () => void; onCompleteStep: () => void; onChangeStep: () => void;
  onLogClick: (logId: number) => void; isCompleted?: boolean; showActions?: boolean; hasOriginalLogs?: boolean;
  canChangeStep?: boolean; canCompleteStep?: boolean; canAddNote?: boolean; stepId: number; isSkipped?: boolean; isSelected?: boolean;
}) {
  const { t } = useTranslate('orders');

  const displayLogs = logs || [];
  const emptyMessage = hasOriginalLogs ? t('noLogsMatchFilter') : t('noActivityForStep');

  if (isSkipped && displayLogs.length === 0) {
    return (
      <div
        id={`order-step-${stepId}`}
        style={{
          scrollMarginTop: 24,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 12,
          flexWrap: 'wrap',
          borderRadius: 10,
          padding: '9px 14px',
          marginBottom: 8,
          background: '#FFFBEB',
          border: isSelected ? '1px solid #2563EB' : '1px dashed #FDE68A',
          boxShadow: isSelected ? '0 0 0 3px rgba(37, 99, 235, 0.12)' : undefined,
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 0, flex: '1 1 260px' }}>
          <div
            style={{
              width: 7, height: 7, borderRadius: '50%', background: '#D97706', flexShrink: 0,
            }}
          />
          <Text style={{ fontSize: 12, color: '#64748B', fontWeight: 600, whiteSpace: 'nowrap' }}>
            {translateWorkflowLabel(stageName, t)}
          </Text>
          <Text style={{ color: '#CBD5E1', flexShrink: 0 }}>-</Text>
          <Text
            style={{
              fontSize: 12,
              color: '#92400E',
              fontWeight: 500,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
          >
            {translateWorkflowLabel(stepName, t)}
          </Text>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, flex: '0 0 auto' }}>
          <Text style={{ fontSize: 11, color: '#D97706' }}>{emptyMessage}</Text>
          <Tag
            style={{
              borderRadius: 6, fontSize: 11, padding: '0 8px', margin: 0,
              background: '#FFF7ED', color: '#D97706', border: '1px solid #FDE68A',
            }}
          >
            {t('stepSkipped')}
          </Tag>
        </div>
      </div>
    );
  }

  return (
    <>
      <div
        id={`order-step-${stepId}`}
        style={{
          scrollMarginTop: 24,
          borderRadius: 14, padding: '20px 24px', marginBottom: 16,
          background: isCurrent ? '#F0F7FF' : isSkipped ? '#FFFBEB' : '#FAFBFC',
          border: isSelected ? '1px solid #2563EB' : isCurrent ? '1px solid #BFDBFE' : isSkipped ? '1px dashed #FDE68A' : '1px solid #E2E8F0',
          boxShadow: isSelected ? '0 0 0 3px rgba(37, 99, 235, 0.12)' : undefined,
        }}
      >
        {/* Step header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16, flexWrap: 'wrap', gap: 12 }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
              <div
                style={{
                  width: 8, height: 8, borderRadius: '50%',
                  background: isCurrent ? '#2563EB' : isCompleted ? '#10B981' : isSkipped ? '#D97706' : '#94A3B8',
                }}
              />
              <Text style={{ fontWeight: 600, fontSize: 14, color: '#0F172A' }}>
                {translateWorkflowLabel(stageName, t)}
              </Text>
              <Text style={{ color: '#94A3B8', fontWeight: 400 }}>›</Text>
              <Text style={{ fontWeight: 500, fontSize: 14, color: isCurrent ? '#2563EB' : '#475569' }}>
                {translateWorkflowLabel(stepName, t)}
              </Text>
              {isCurrent && (
                <Tag style={{
                  borderRadius: 6, fontSize: 11, padding: '0 8px', margin: 0,
                  background: '#EFF6FF', color: '#2563EB', border: '1px solid #BFDBFE',
                }}>
                  {t('trackingCurrentStep')}
                </Tag>
              )}
              {isCompleted && !isCurrent && (
                <Tag
                  style={{
                    borderRadius: 6, fontSize: 11, padding: '0 8px', margin: 0,
                    background: '#ECFDF5', color: '#059669', border: '1px solid #A7F3D0',
                  }}
                >
                  <CheckCircleOutlined style={{ marginRight: 4, fontSize: 10 }} />
                  {t('stepCompleted')}
                </Tag>
              )}
              {isSkipped && (
                <Tag
                  style={{
                    borderRadius: 6, fontSize: 11, padding: '0 8px', margin: 0,
                    background: '#FFFBEB', color: '#D97706', border: '1px solid #FDE68A',
                  }}
                >
                  {t('stepSkipped')}
                </Tag>
              )}
            </div>
            {(stepEnteredAt || stepExitedAt) && (
              <Text style={{ color: '#94A3B8', fontSize: 12, marginLeft: 18 }}>
                {stepEnteredAt && `${t('stepEntered')}: ${dayjs(stepEnteredAt).format('MMM D, YYYY h:mm A')}`}
                {stepEnteredAt && stepExitedAt && ' · '}
                {stepExitedAt && `${t('stepExited')}: ${dayjs(stepExitedAt).format('MMM D, YYYY h:mm A')}`}
              </Text>
            )}
          </div>
          {showActions && isCurrent && (
            <div style={{ display: 'flex', gap: 8 }}>
              {canChangeStep && (
                <Button
                  size='small'
                  onClick={onChangeStep}
                  icon={<SwapOutlined />}
                  style={{
                    borderRadius: 8, height: 32, fontSize: 12,
                    borderColor: '#E2E8F0', color: '#64748B',
                  }}
                >
                  {t('changeStep')}
                </Button>
              )}
              {canCompleteStep && (
                <Button
                  size='small'
                  onClick={onCompleteStep}
                  icon={<CheckCircleOutlined />}
                  style={{
                    borderRadius: 8, height: 32, fontSize: 12,
                    borderColor: '#BBF7D0', color: '#16A34A', background: '#F0FDF4',
                  }}
                >
                  {t('completeStep')}
                </Button>
              )}
              {canAddNote && (
                <Button
                  size='small'
                  onClick={onAddNote}
                  icon={<PlusOutlined />}
                  style={{
                    borderRadius: 8, height: 32, fontSize: 12,
                    borderColor: '#BFDBFE', color: '#2563EB', background: '#EFF6FF',
                  }}
                >
                  {t('addNote')}
                </Button>
              )}
            </div>
          )}
        </div>
        {displayLogs.length > 0 ? (
          <div>
            {displayLogs.map((log) => (
              <LogItem key={log.id} log={log} orderId={orderId} onClick={() => onLogClick(log.id)} />
            ))}
          </div>
        ) : (
          <div style={{ textAlign: 'center', padding: '20px 0' }}>
            <Text style={{ color: '#CBD5E1', fontSize: 13 }}>{emptyMessage}</Text>
          </div>
        )}
      </div>
    </>
  );
}

/* ─── Main Activity Logs Tab ─── */
export default function ActivityLogsTabComponent({
  steps,
  orderId,
  orderStatus,
  currentStepId,
  currentStepName,
  currentStageName,
  stageAssignments = [],
  employeeOptions = [],
  employeeOptionsLoading = false,
  canAssignStageAssignee = false,
  updatingStageId = null,
  onStageAssigneeChange,
}: Props) {
  const { t } = useTranslate('orders');
  const router = useRouter();
  const { hasPermission } = usePermissionContext();

  // Permission checks
  const canChangeStep = hasPermission(PERMISSIONS.ORDER_CHANGE_STEP);
  const canCompleteStep = hasPermission(PERMISSIONS.ORDER_COMPLETE_STEP);
  const canAddNote = hasPermission(PERMISSIONS.ORDER_ADD_NOTE);

  const showActions = orderStatus?.toLowerCase() === 'inprogress';
  const [filters, setFilters] = useState<FilterType[]>(['all']);
  const [addNoteModalOpen, setAddNoteModalOpen] = useState(false);
  const [completeStepModalOpen, setCompleteStepModalOpen] = useState(false);
  const [changeStepModalOpen, setChangeStepModalOpen] = useState(false);
  const [selectedStep, setSelectedStep] = useState<{ stepId: number; stepName: string } | null>(null);
  const [focusedStepId, setFocusedStepId] = useState<number | null>(null);

  const handleCompleteStepClick = (stepId: number, stepName: string) => {
    setSelectedStep({ stepId, stepName });
    setCompleteStepModalOpen(true);
  };

  const handleChangeStepClick = () => {
    setChangeStepModalOpen(true);
  };

  const handleLogClick = (logId: number) => {
    router.push(paths.panel.orderLogDetail(orderId, logId));
  };

  const handleProgressStepClick = (stepId: number) => {
    setFocusedStepId(stepId);
    document.getElementById(`order-step-${stepId}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  const handleFilterChange = (filter: FilterType, checked: boolean) => {
    if (filter === 'all') {
      setFilters(checked ? ['all'] : []);
    } else {
      let newFilters = filters.filter((f) => f !== 'all');
      if (checked) {
        newFilters = [...newFilters, filter];
      } else {
        newFilters = newFilters.filter((f) => f !== filter);
      }
      if (newFilters.length === 0) {
        setFilters(['all']);
        return;
      }
      setFilters(newFilters as FilterType[]);
    }
  };

  const filteredSteps = useMemo(() => {
    if (filters.includes('all') || filters.length === 0) {
      return steps;
    }

    return steps.map((step) => ({
      ...step,
      logs: step.logs.filter((log) => {
        const logType = log.logType.toLowerCase();
        if (filters.includes('activityNote') && ACTIVITY_NOTE_TYPES.includes(logType)) {
          return true;
        }
        if (filters.includes('statusChangeLogs') && STATUS_CHANGE_TYPES.includes(logType)) {
          return true;
        }
        if (filters.includes('stepChangeLogs') && STEP_CHANGE_TYPES.includes(logType)) {
          return true;
        }
        return false;
      }),
    }));
  }, [steps, filters]);

  const displaySteps = useMemo(
    () => sortStepsForActivityDisplay(filteredSteps, currentStepId),
    [filteredSteps, currentStepId],
  );
  const isFilteringLogs = !filters.includes('all') && filters.length > 0;
  const maxReachedPosition = useMemo(() => getMaxReachedPosition(steps || []), [steps]);
  const stageAssignmentMap = useMemo(
    () => new Map(stageAssignments.map((assignment) => [assignment.stageId, assignment])),
    [stageAssignments],
  );

  // Count logs per filter type
  const logCounts = useMemo(() => {
    if (!steps) return { all: 0, activityNote: 0, statusChangeLogs: 0, stepChangeLogs: 0 };
    let all = 0, activityNote = 0, statusChangeLogs = 0, stepChangeLogs = 0;
    steps.forEach(step => {
      step.logs?.forEach(log => {
        all++;
        const lt = log.logType.toLowerCase();
        if (ACTIVITY_NOTE_TYPES.includes(lt)) activityNote++;
        if (STATUS_CHANGE_TYPES.includes(lt)) statusChangeLogs++;
        if (STEP_CHANGE_TYPES.includes(lt)) stepChangeLogs++;
      });
    });
    return { all, activityNote, statusChangeLogs, stepChangeLogs };
  }, [steps]);

  const filterLabels: Record<FilterType, string> = {
    all: t('all'),
    activityNote: t('activityNote'),
    statusChangeLogs: t('statusChangeLogs'),
    stepChangeLogs: t('stepChangeLogs'),
  };

  return (
    <div>
      <OrderTrackingProgressComponent
        steps={steps}
        currentStepId={currentStepId}
        currentStepName={currentStepName}
        currentStageName={currentStageName}
        orderStatus={orderStatus}
        onStepClick={handleProgressStepClick}
      />

      {/* Filter Pills */}
      <div style={{
        display: 'flex', gap: 10, marginBottom: 20, flexWrap: 'wrap',
      }}>
        {(Object.keys(FILTER_COLORS) as FilterType[]).map((filter) => {
          const isActive = filters.includes(filter);
          const colors = FILTER_COLORS[filter];
          const count = logCounts[filter];
          return (
            <button
              key={filter}
              onClick={() => handleFilterChange(filter, !isActive)}
              style={{
                display: 'flex', alignItems: 'center', gap: 6,
                padding: '6px 16px', borderRadius: 8,
                border: `1px solid ${isActive ? colors.border : '#E2E8F0'}`,
                background: isActive ? colors.activeBg : colors.bg,
                color: isActive ? colors.activeColor : colors.color,
                fontWeight: isActive ? 600 : 400, fontSize: 13,
                cursor: 'pointer', transition: 'all 0.15s ease',
                outline: 'none',
              }}
            >
              {filterLabels[filter]}
              <span style={{
                background: isActive ? 'rgba(255,255,255,0.3)' : '#E2E8F0',
                borderRadius: 6, padding: '1px 7px', fontSize: 11, fontWeight: 600,
                color: isActive ? colors.activeColor : '#94A3B8',
              }}>
                {count}
              </span>
            </button>
          );
        })}
      </div>

      {/* Activity Logs Content */}
      {!steps?.length ? (
        <div style={{ padding: 48, borderRadius: 14, border: '1px solid #E2E8F0', background: '#FAFBFC', textAlign: 'center' }}>
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Text style={{ color: '#94A3B8' }}>{t('noActivityLogs')}</Text>} />
        </div>
      ) : (
        <div>
          {(() => {
            const stageStepCounts = steps.reduce<Record<string, number>>((acc, s) => {
              acc[s.stageName] = (acc[s.stageName] || 0) + 1;
              return acc;
            }, {});
            const visibleSteps = displaySteps.filter((step) => {
              const originalStep = steps.find((s) => s.stepId === step.stepId);
              const hasOriginalLogs = !!(originalStep?.logs && originalStep.logs.length > 0);
              const skipped = isSkippedStep(step, currentStepId, maxReachedPosition);
              return shouldShowStepDetails(step, currentStepId, skipped, hasOriginalLogs, isFilteringLogs);
            });

            if (visibleSteps.length === 0) {
              return (
                <div style={{ padding: 40, borderRadius: 12, border: '1px solid #E2E8F0', background: '#FAFBFC', textAlign: 'center' }}>
                  <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<Text style={{ color: '#94A3B8' }}>{t('noLogsMatchFilter')}</Text>} />
                </div>
              );
            }

            let lastStage = '';

            return visibleSteps.flatMap((step) => {
              const originalStep = steps.find((s) => s.stepId === step.stepId);
              const hasOriginalLogs = !!(originalStep?.logs && originalStep.logs.length > 0);
              const skipped = isSkippedStep(step, currentStepId, maxReachedPosition);

              const elements: ReactNode[] = [];
              if (step.stageName !== lastStage) {
                lastStage = step.stageName;
                elements.push(
                  <StageMilestone
                    key={`stage-${step.stageId}`}
                    stageId={step.stageId}
                    stageName={step.stageName}
                    stepCount={stageStepCounts[step.stageName] || 1}
                    assignee={stageAssignmentMap.get(step.stageId)}
                    canAssign={canAssignStageAssignee}
                    employeeOptions={employeeOptions}
                    employeeOptionsLoading={employeeOptionsLoading}
                    isUpdating={updatingStageId === step.stageId}
                    onAssigneeChange={onStageAssigneeChange}
                  />
                );
              }

              const uniqueKey = `${step.stageId}-${step.stepId}-${step.isCurrent ? 'current' : step.isCompleted ? 'done' : 'active'}`;
              elements.push(
                <StepSection
                  key={uniqueKey}
                  stageName={step.stageName}
                  stepName={step.stepName}
                  stepId={step.stepId}
                  logs={step.logs}
                  stepEnteredAt={step.stepEnteredAt}
                  stepExitedAt={step.stepExitedAt}
                  isCurrent={isActiveCurrentStep(step, currentStepId)}
                  isSkipped={skipped}
                  isSelected={focusedStepId === step.stepId}
                  orderId={orderId}
                  onAddNote={() => setAddNoteModalOpen(true)}
                  onCompleteStep={() => handleCompleteStepClick(step.stepId, translateWorkflowLabel(step.stepName, t))}
                  onChangeStep={handleChangeStepClick}
                  onLogClick={handleLogClick}
                  isCompleted={step.isCompleted}
                  showActions={showActions}
                  hasOriginalLogs={hasOriginalLogs}
                  canChangeStep={canChangeStep}
                  canCompleteStep={canCompleteStep}
                  canAddNote={canAddNote}
                />
              );
              return elements;
            });
          })()}
        </div>
      )}

      <AddNoteModalComponent open={addNoteModalOpen} onClose={() => setAddNoteModalOpen(false)} orderId={orderId} />

      {selectedStep && (
        <CompleteStepModalComponent
          open={completeStepModalOpen}
          onClose={() => {
            setCompleteStepModalOpen(false);
            setSelectedStep(null);
          }}
          orderId={orderId}
          stepId={selectedStep.stepId}
          stepName={selectedStep.stepName}
        />
      )}

      <ChangeStepModalComponent open={changeStepModalOpen} onClose={() => setChangeStepModalOpen(false)} orderId={orderId} />
    </div>
  );
}
