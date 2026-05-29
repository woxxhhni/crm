'use client';

import { Typography, Tag } from 'antd';
import { CheckCircleOutlined, ClockCircleOutlined, ArrowRightOutlined, MinusOutlined } from '@ant-design/icons';
import type { StepWithLogs } from '@/api/orders/types';
import { useTranslate } from '@/locales/use-locales';
import dayjs from 'dayjs';
import { translateWorkflowLabel } from '../utils/workflow-labels';

const { Text } = Typography;

interface Props {
  steps: StepWithLogs[];
  currentStepId?: number;
  currentStepName?: string;
  currentStageName?: string;
  orderStatus?: string;
  onStepClick?: (stepId: number) => void;
}

const STATUS_STYLES: Record<string, { color: string; bg: string; border: string; labelKey: string }> = {
  inprogress: { color: '#2563EB', bg: '#EFF6FF', border: '#BFDBFE', labelKey: 'inProgress' },
  completed: { color: '#059669', bg: '#ECFDF5', border: '#A7F3D0', labelKey: 'completed' },
  suspended: { color: '#D97706', bg: '#FFFBEB', border: '#FDE68A', labelKey: 'suspended' },
  canceled: { color: '#DC2626', bg: '#FEF2F2', border: '#FECACA', labelKey: 'canceled' },
};

function isActiveCurrentStep(step: StepWithLogs, currentStepId?: number) {
  return !step.isCompleted && (step.isCurrent || step.stepId === currentStepId);
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

export default function OrderTrackingProgressComponent({
  steps,
  currentStepId,
  currentStepName,
  currentStageName,
  orderStatus,
  onStepClick,
}: Props) {
  const { t } = useTranslate('orders');
  const statusKey = orderStatus?.toLowerCase() || 'inprogress';
  const statusStyle = STATUS_STYLES[statusKey] || STATUS_STYLES.inprogress;
  const activeCurrentStep = steps.find((step) => isActiveCurrentStep(step, currentStepId));
  const currentStepMeta = activeCurrentStep ?? steps.find((step) => step.stepId === currentStepId);
  const displayStageName = activeCurrentStep?.stageName ?? currentStepMeta?.stageName ?? currentStageName;
  const displayStepName = activeCurrentStep?.stepName ?? currentStepMeta?.stepName ?? currentStepName;
  const currentPositionCompleted = !!currentStepMeta?.isCompleted && !activeCurrentStep;

  const workflowSteps = [...steps].sort((a, b) => (a.stepOrderPosition ?? 0) - (b.stepOrderPosition ?? 0));
  const completedCount = workflowSteps.filter((s) => s.isCompleted).length;
  const maxReachedPosition = getMaxReachedPosition(workflowSteps);

  // Group the full workflow by stage so future steps remain visible in gray.
  const stageGroups = workflowSteps.reduce<{ stageName: string; steps: StepWithLogs[] }[]>((acc, step) => {
    const last = acc[acc.length - 1];
    if (last && last.stageName === step.stageName) {
      last.steps.push(step);
    } else {
      acc.push({ stageName: step.stageName, steps: [step] });
    }
    return acc;
  }, []);

  return (
    <div
      style={{
        background: '#fff',
        border: '1px solid #E2E8F0',
        borderRadius: 14,
        padding: '20px 24px',
        marginBottom: 20,
      }}
    >
      {/* Current position */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 12, marginBottom: 20 }}>
        <div>
          <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 4 }}>
            {t('trackingCurrentPosition')}
          </Text>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            {displayStageName && (
              <>
                <Text style={{ fontWeight: 600, fontSize: 15, color: '#0F172A' }}>{translateWorkflowLabel(displayStageName, t)}</Text>
                <Text style={{ color: '#CBD5E1' }}>›</Text>
              </>
            )}
            <Text style={{ fontWeight: 600, fontSize: 15, color: currentPositionCompleted ? '#059669' : '#2563EB' }}>
              {translateWorkflowLabel(displayStepName, t) || '—'}
            </Text>
            {currentPositionCompleted && (
              <Tag
                style={{
                  borderRadius: 6,
                  fontSize: 11,
                  padding: '0 8px',
                  margin: 0,
                  background: '#ECFDF5',
                  color: '#059669',
                  border: '1px solid #A7F3D0',
                }}
              >
                {t('stepCompleted')}
              </Tag>
            )}
          </div>
        </div>
        <Tag
          style={{
            borderRadius: 8,
            padding: '4px 14px',
            fontWeight: 600,
            fontSize: 12,
            border: `1px solid ${statusStyle.border}`,
            background: statusStyle.bg,
            color: statusStyle.color,
            margin: 0,
          }}
        >
          {t(statusStyle.labelKey)}
        </Tag>
      </div>

      {/* Step pipeline */}
      {workflowSteps.length > 0 && (
        <>
          <Text style={{ color: '#94A3B8', fontSize: 12, display: 'block', marginBottom: 12 }}>
            {t('trackingStepsProgress', { completed: completedCount, total: workflowSteps.length })}
          </Text>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {stageGroups.map((group) => (
              <div key={group.stageName}>
                <Text style={{
                  fontSize: 11, fontWeight: 700, color: '#64748B',
                  textTransform: 'uppercase', letterSpacing: 0.6,
                  marginBottom: 8, display: 'block',
                }}>
                  {translateWorkflowLabel(group.stageName, t)}
                </Text>
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 0, overflowX: 'auto', paddingBottom: 4 }}>
                  {group.steps.map((step, index) => {
                    const isCurrent = isActiveCurrentStep(step, currentStepId);
                    const isDone = step.isCompleted && !isCurrent;
                    const isSkipped = isSkippedStep(step, currentStepId, maxReachedPosition);
                    const dotColor = isDone ? '#10B981' : isCurrent ? '#2563EB' : isSkipped ? '#D97706' : '#94A3B8';
                    const bgColor = isDone ? '#ECFDF5' : isCurrent ? '#EFF6FF' : isSkipped ? '#FFFBEB' : '#F8FAFC';
                    const borderColor = isDone ? '#A7F3D0' : isCurrent ? '#BFDBFE' : isSkipped ? '#FDE68A' : '#E2E8F0';
                    const textColor = isCurrent ? '#2563EB' : isSkipped ? '#B45309' : '#475569';

                    return (
                      <div key={step.stepId} style={{ display: 'flex', alignItems: 'center', flexShrink: 0 }}>
                        <button
                          type="button"
                          onClick={() => onStepClick?.(step.stepId)}
                          style={{
                            display: 'flex', flexDirection: 'column', alignItems: 'center',
                            minWidth: 90, maxWidth: 120, padding: '8px 10px',
                            borderRadius: 10, background: bgColor, border: `1px solid ${borderColor}`,
                            borderStyle: isSkipped ? 'dashed' : 'solid',
                            cursor: onStepClick ? 'pointer' : 'default',
                            font: 'inherit',
                            outline: 'none',
                          }}
                          title={translateWorkflowLabel(step.stepName, t)}
                        >
                          <div style={{
                            width: 20, height: 20, borderRadius: '50%', background: dotColor,
                            display: 'flex', alignItems: 'center', justifyContent: 'center', marginBottom: 4,
                          }}>
                            {isDone ? (
                              <CheckCircleOutlined style={{ color: '#fff', fontSize: 11 }} />
                            ) : isCurrent ? (
                              <ClockCircleOutlined style={{ color: '#fff', fontSize: 10 }} />
                            ) : isSkipped ? (
                              <MinusOutlined style={{ color: '#fff', fontSize: 10 }} />
                            ) : null}
                          </div>
                          <Text style={{
                            fontSize: 10, fontWeight: isCurrent ? 600 : 500,
                            color: textColor,
                            textAlign: 'center', lineHeight: 1.25,
                          }}>
                            {translateWorkflowLabel(step.stepName, t)}
                          </Text>
                        </button>
                        {index < group.steps.length - 1 && (
                          <ArrowRightOutlined style={{ color: '#CBD5E1', fontSize: 10, margin: '0 2px', flexShrink: 0 }} />
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
