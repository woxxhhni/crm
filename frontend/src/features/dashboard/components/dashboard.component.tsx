'use client';

import { useMemo } from 'react';
import { Card, Spin, Typography, Result, Button } from 'antd';
import {
  CloseCircleOutlined,
  PauseCircleOutlined,
  CheckCircleOutlined,
  HourglassOutlined,
  ProjectOutlined,
  TeamOutlined,
  ShopOutlined,
  UserOutlined,
  ArrowRightOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { useRouter } from 'next/navigation';
import { container } from '@/services/di-container';
import { OrdersService } from '@/features/orders/services/orders.service';
import { ClientsService } from '@/features/clients/services/clients.service';
import { ProvidersService } from '@/features/providers/services/providers.service';
import { EmployeesService } from '@/features/employees/services/employees.service';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';
import { useTranslate } from '@/locales/use-locales';
import { CONFIG } from '../../../../global-config';
import { colors } from '@/config/theme';

const { Title, Text } = Typography;

/* ─── Summary Card (clickable) ─── */
interface SummaryCardProps {
  title: string;
  viewAllLabel: string;
  value: number | undefined;
  icon: React.ReactNode;
  accent: string;
  href: string;
  loading?: boolean;
}

const SummaryCard = ({ title, viewAllLabel, value, icon, accent, href, loading }: SummaryCardProps) => {
  const router = useRouter();
  return (
    <Card
      className='flex-1 min-w-[200px] cursor-pointer group'
      style={{
        borderRadius: 16,
        border: '1px solid #E2E8F0',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      }}
      styles={{ body: { padding: '24px' } }}
      hoverable
      onClick={() => router.push(href)}
      onMouseEnter={(e) => {
        e.currentTarget.style.boxShadow = '0 10px 25px rgba(0,0,0,0.08)';
        e.currentTarget.style.transform = 'translateY(-2px)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.boxShadow = 'none';
        e.currentTarget.style.transform = 'translateY(0)';
      }}
    >
      <div className='flex justify-between items-start'>
        <div>
          <Text className='!text-[13px] !font-medium' style={{ color: '#64748B' }}>
            {title}
          </Text>
          <div className='mt-2'>
            {loading ? (
              <Spin size='small' />
            ) : (
              <Title level={2} className='!mb-0' style={{ color: '#0F172A' }}>
                {value ?? 0}
              </Title>
            )}
          </div>
        </div>
        <div
          className='w-12 h-12 rounded-xl flex items-center justify-center text-xl'
          style={{ background: `${accent}15`, color: accent }}
        >
          {icon}
        </div>
      </div>
      <div
        className='flex items-center gap-1 mt-4 text-[12px] font-medium'
        style={{ color: accent }}
      >
        <span>{viewAllLabel}</span>
        <ArrowRightOutlined className='text-[10px] transition-transform duration-200 group-hover:translate-x-1' />
      </div>
    </Card>
  );
};

/* ─── Status Stat Card ─── */
interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  accent: string;
}

const StatCard = ({ title, value, icon, accent }: StatCardProps) => (
  <Card
    className='flex-1 min-w-[160px]'
    style={{ borderRadius: 14, border: '1px solid #E2E8F0' }}
    styles={{ body: { padding: '18px 20px' } }}
  >
    <div className='flex items-center gap-3'>
      <div
        className='w-10 h-10 rounded-lg flex items-center justify-center text-base'
        style={{ background: `${accent}15`, color: accent }}
      >
        {icon}
      </div>
      <div>
        <Title level={3} className='!mb-0' style={{ color: '#0F172A' }}>
          {value}
        </Title>
        <Text className='!text-[12px]' style={{ color: '#64748B' }}>
          {title}
        </Text>
      </div>
    </div>
  </Card>
);

/* ─── Quick Action Link ─── */
interface QuickActionProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  href: string;
  accent: string;
}

const QuickAction = ({ title, description, icon, href, accent }: QuickActionProps) => {
  const router = useRouter();
  return (
    <div
      className='flex items-center gap-4 p-4 rounded-xl cursor-pointer transition-all duration-200'
      style={{ border: '1px solid #E2E8F0' }}
      onClick={() => router.push(href)}
      onMouseEnter={(e) => {
        e.currentTarget.style.background = '#F8FAFC';
        e.currentTarget.style.borderColor = accent;
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.background = 'transparent';
        e.currentTarget.style.borderColor = '#E2E8F0';
      }}
    >
      <div
        className='w-10 h-10 rounded-lg flex items-center justify-center text-base flex-shrink-0'
        style={{ background: `${accent}15`, color: accent }}
      >
        {icon}
      </div>
      <div className='flex-1 min-w-0'>
        <div className='text-[14px] font-semibold' style={{ color: '#0F172A' }}>
          {title}
        </div>
        <div className='text-[12px]' style={{ color: '#64748B' }}>
          {description}
        </div>
      </div>
      <ArrowRightOutlined className='text-[12px]' style={{ color: '#94A3B8' }} />
    </div>
  );
};

/* ─── Donut Tooltip ─── */
interface CustomTooltipProps {
  active?: boolean;
  payload?: Array<{
    name: string;
    value: number;
    payload: { name: string; value: number; color: string };
  }>;
}

const CustomTooltip = ({ active, payload }: CustomTooltipProps) => {
  if (active && payload && payload.length > 0) {
    const data = payload[0].payload;
    return (
      <div
        className='px-3 py-2 rounded-lg shadow-lg'
        style={{ background: '#FFFFFF', border: '1px solid #E2E8F0' }}
      >
        <p className='text-sm font-medium' style={{ color: '#0F172A' }}>
          {data.name}
        </p>
        <p className='text-sm' style={{ color: '#64748B' }}>
          {data.value}%
        </p>
      </div>
    );
  }
  return null;
};

/* ─── Stage Colors ─── */
const STAGE_COLORS: Record<string, string> = {
  'Order Placement': '#3B82F6',
  'Pre-Production': '#8B5CF6',
  Manufacturing: '#10B981',
  'Post-Production': '#F59E0B',
  'Shipping & Delivery': '#EF4444',
};

const STAGE_LABEL_KEYS: Record<string, string> = {
  'Order Placement': 'stageOrderPlacement',
  'Pre-Production': 'stagePreProduction',
  Manufacturing: 'stageManufacturing',
  'Post-Production': 'stagePostProduction',
  'Shipping & Delivery': 'stageShippingDelivery',
};

/* ═══════════════════════════════════════════════
 * DASHBOARD COMPONENT
 * ═══════════════════════════════════════════════ */
export const DashboardComponent = () => {
  const router = useRouter();
  const { t } = useTranslate('dashboard');
  const { t: tCommon } = useTranslate('common');
  const { role, loading: permissionLoading } = usePermissionContext();

  const canViewDashboard = role === 'admin' || role === 'manager';
  const canViewEmployees = role === 'admin' || role === 'manager';

  // Services
  const ordersService = useMemo(() => container.resolve(OrdersService), []);
  const clientsService = useMemo(() => container.resolve(ClientsService), []);
  const providersService = useMemo(() => container.resolve(ProvidersService), []);
  const employeesService = useMemo(() => container.resolve(EmployeesService), []);

  // ─── API Queries ───
  const { data: summaryData, isLoading: summaryLoading } = useQuery({
    queryKey: ['orders-summary'],
    queryFn: () => ordersService.getSummary(),
    enabled: canViewDashboard,
  });

  const { data: clientsData, isLoading: clientsLoading } = useQuery({
    queryKey: ['clients-count'],
    queryFn: () => clientsService.list(1, 1),
    enabled: canViewDashboard,
  });

  const { data: providersData, isLoading: providersLoading } = useQuery({
    queryKey: ['providers-count'],
    queryFn: () => providersService.list(1, 1),
    enabled: canViewDashboard,
  });

  const { data: employeesData, isLoading: employeesLoading } = useQuery({
    queryKey: ['employees-count'],
    queryFn: () => employeesService.list(1, 1),
    enabled: canViewDashboard && canViewEmployees,
  });

  // ─── Access Denied ───
  if (!permissionLoading && !canViewDashboard) {
    return (
      <Result
        status='403'
        title={tCommon('accessDenied')}
        subTitle={tCommon('noPermissionToViewPage')}
        extra={
          <Button type='primary' onClick={() => router.push(CONFIG.auth.redirectPath)}>
            {tCommon('backToHome')}
          </Button>
        }
      />
    );
  }

  // ─── Loading State ───
  if (summaryLoading && clientsLoading && providersLoading) {
    return (
      <div className='flex items-center justify-center h-96'>
        <Spin size='large' />
      </div>
    );
  }

  const chartData =
    summaryData?.stageSummaries?.map((stage) => ({
      name: t(STAGE_LABEL_KEYS[stage.name] || stage.name),
      value: stage.percent,
      color: STAGE_COLORS[stage.name] || '#94A3B8',
    })) || [];

  const totalProjects = summaryData?.total || 0;

  return (
    <div className='space-y-6'>
      {/* ── Row 1: Overview Summary Cards ── */}
      <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4'>
        <SummaryCard
          title={t('totalOrders')}
          viewAllLabel={t('viewAll')}
          value={summaryData?.total}
          icon={<ProjectOutlined />}
          accent={colors.accent}
          href='/panel/orders'
          loading={summaryLoading}
        />
        <SummaryCard
          title={t('totalClients')}
          viewAllLabel={t('viewAll')}
          value={clientsData?.total}
          icon={<TeamOutlined />}
          accent='#8B5CF6'
          href='/panel/clients'
          loading={clientsLoading}
        />
        <SummaryCard
          title={t('totalProviders')}
          viewAllLabel={t('viewAll')}
          value={providersData?.total}
          icon={<ShopOutlined />}
          accent='#10B981'
          href='/panel/providers'
          loading={providersLoading}
        />
        {canViewEmployees && (
          <SummaryCard
            title={t('totalEmployees')}
            viewAllLabel={t('viewAll')}
            value={employeesData?.total}
            icon={<UserOutlined />}
            accent='#F59E0B'
            href='/panel/employees'
            loading={employeesLoading}
          />
        )}
      </div>

      {/* ── Row 2: Order Status Breakdown ── */}
      <div className='grid grid-cols-2 sm:grid-cols-4 gap-3'>
        <StatCard
          title={t('inProgress')}
          value={summaryData?.inProgress || 0}
          icon={<HourglassOutlined />}
          accent={colors.status.inProgress}
        />
        <StatCard
          title={t('completed')}
          value={summaryData?.completed || 0}
          icon={<CheckCircleOutlined />}
          accent={colors.status.completed}
        />
        <StatCard
          title={t('canceled')}
          value={summaryData?.canceled || 0}
          icon={<CloseCircleOutlined />}
          accent={colors.status.canceled}
        />
        <StatCard
          title={t('suspended')}
          value={summaryData?.suspended || 0}
          icon={<PauseCircleOutlined />}
          accent={colors.status.suspended}
        />
      </div>

      {/* ── Row 3: Chart + Quick Actions ── */}
      <div className='grid grid-cols-1 lg:grid-cols-3 gap-4'>
        {/* Pipeline Chart */}
        <Card
          className='lg:col-span-2'
          style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
          styles={{ header: { borderBottom: '1px solid #F1F5F9' } }}
          title={
            <span className='text-[15px] font-semibold' style={{ color: '#0F172A' }}>
              {t('productionPipelineBreakdown')}
            </span>
          }
        >
          <Text className='!text-[13px]' style={{ color: '#64748B' }}>
            {t('projectStagesDistribution')}
          </Text>

          {chartData.length > 0 ? (
            <div className='flex flex-col items-center mt-6'>
              <div className='relative w-[240px] h-[240px]'>
                <ResponsiveContainer width='100%' height='100%'>
                  <PieChart>
                    <Pie
                      data={chartData}
                      cx='50%'
                      cy='50%'
                      innerRadius={75}
                      outerRadius={110}
                      dataKey='value'
                      strokeWidth={0}
                      cursor='pointer'
                    >
                      {chartData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip
                      content={<CustomTooltip />}
                      cursor={false}
                      wrapperStyle={{ zIndex: 1000 }}
                    />
                  </PieChart>
                </ResponsiveContainer>
                {/* Center Text */}
                <div className='absolute inset-0 flex flex-col items-center justify-center pointer-events-none'>
                  <span className='text-3xl font-bold' style={{ color: '#0F172A' }}>
                    {totalProjects}
                  </span>
                  <span className='text-xs' style={{ color: '#64748B' }}>
                    {t('totalProjects')}
                  </span>
                </div>
              </div>

              {/* Legend */}
              <div className='flex flex-wrap justify-center gap-x-5 gap-y-2 mt-6'>
                {chartData.map((stage) => (
                  <div key={stage.name} className='flex items-center gap-2'>
                    <div
                      className='w-2.5 h-2.5 rounded-full'
                      style={{ backgroundColor: stage.color }}
                    />
                    <span className='text-[12px]' style={{ color: '#0F172A' }}>
                      {stage.name}
                    </span>
                    <span className='text-[12px] font-medium' style={{ color: '#64748B' }}>
                      {stage.value}%
                    </span>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className='flex items-center justify-center py-16'>
              <Text style={{ color: '#94A3B8' }}>{t('noPipelineData')}</Text>
            </div>
          )}
        </Card>

        {/* Quick Actions Panel */}
        <Card
          style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
          styles={{ header: { borderBottom: '1px solid #F1F5F9' } }}
          title={
            <span className='text-[15px] font-semibold' style={{ color: '#0F172A' }}>
              {t('quickActions')}
            </span>
          }
        >
          <div className='space-y-3'>
            <QuickAction
              title={t('manageOrders')}
              description={t('manageOrdersDescription')}
              icon={<ProjectOutlined />}
              href='/panel/orders'
              accent={colors.accent}
            />
            <QuickAction
              title={t('manageClients')}
              description={t('manageClientsDescription')}
              icon={<TeamOutlined />}
              href='/panel/clients'
              accent='#8B5CF6'
            />
            <QuickAction
              title={t('manageProviders')}
              description={t('manageProvidersDescription')}
              icon={<ShopOutlined />}
              href='/panel/providers'
              accent='#10B981'
            />
            {canViewEmployees && (
              <QuickAction
                title={t('manageEmployees')}
                description={t('manageEmployeesDescription')}
                icon={<UserOutlined />}
                href='/panel/employees'
                accent='#F59E0B'
              />
            )}
          </div>
        </Card>
      </div>
    </div>
  );
};
