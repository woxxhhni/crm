import type { TablePaginationConfig } from 'antd';
import i18n from '@/locales/i18n';

/**
 * Standard pagination configuration for all list-view tables.
 * Creates a consistent, polished pagination bar across the app.
 *
 * Usage:
 *   pagination={getTablePagination({ current, total, pageSize, onChange })}
 */
export interface PaginationParams {
  current: number;
  total: number;
  pageSize: number;
  onChange: (page: number, pageSize: number) => void;
}

export function getTablePagination({
  current,
  total,
  pageSize,
  onChange,
}: PaginationParams): TablePaginationConfig {
  return {
    current,
    total,
    pageSize,
    onChange,
    showSizeChanger: true,
    showQuickJumper: total > 50,
    pageSizeOptions: ['10', '20', '50', '100'],
    size: 'default',
    showTotal: (total, range) =>
      i18n.t('paginationTotal', {
        ns: 'common',
        start: range[0],
        end: range[1],
        total,
      }),
    style: {
      padding: '16px 24px',
      margin: 0,
      borderTop: '1px solid #f0f0f0',
      background: '#fafbfc',
      borderRadius: '0 0 16px 16px',
    },
  };
}
