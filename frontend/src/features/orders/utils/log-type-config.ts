// Log type filter categories
// 1xxx – Note (Activity Note filter)
export const ACTIVITY_NOTE_TYPES = ['noteadded', 'noteedited', 'noteremoved'];

// 3xxx – OrderState (Status Change Logs filter)
export const STATUS_CHANGE_TYPES = ['ordercompleted', 'ordercanceled', 'ordersuspended', 'orderreturnedtoprogress', 'ordercreated', 'orderedited', 'employeeassigned'];

// 2xxx – StatusChange (Step Change Logs filter)
export const STEP_CHANGE_TYPES = ['statusforward', 'statusbackward', 'statuscomplete'];

// Note log types that can be edited (using EditNoteModal)
export const EDITABLE_NOTE_TYPES = ['noteadded', 'noteedited', 'noteremoved'];

// Order state log types that can be edited (using OrderActionModal)
export const EDITABLE_ORDER_STATE_TYPES = ['ordercompleted', 'ordercanceled', 'ordersuspended'];

// Map logType to actionType for OrderActionModal
export const LOG_TYPE_TO_ACTION: Record<string, 'complete' | 'cancel' | 'suspend' | 'unsuspend'> = {
  ordercompleted: 'complete',
  ordercanceled: 'cancel',
  ordersuspended: 'suspend',
  orderreturnedtoprogress: 'unsuspend',
};

export interface LogTypeStyle {
  color: string;
  bgColor: string;
  borderColor: string;
  label: string;
}

// Log type display configuration
// Each group has the same color, different groups have different colors
export const LOG_TYPE_CONFIG: Record<string, LogTypeStyle> = {
  // 1xxx – Note (Blue)
  noteadded: { color: '#fff', bgColor: '#1890ff', borderColor: '#1890ff', label: 'Note Added' },
  noteedited: { color: '#fff', bgColor: '#1890ff', borderColor: '#1890ff', label: 'Note Edited' },
  noteremoved: { color: '#fff', bgColor: '#1890ff', borderColor: '#1890ff', label: 'Note Removed' },

  // 2xxx – StatusChange / Step changes
  statuscomplete: { color: '#fff', bgColor: '#52c41a', borderColor: '#52c41a', label: 'Step Completed' },
  statusforward: { color: '#fff', bgColor: '#722ed1', borderColor: '#722ed1', label: 'Step Forward' },
  statusbackward: { color: '#fff', bgColor: '#722ed1', borderColor: '#722ed1', label: 'Step Backward' },

  // 3xxx – OrderState (Teal Green)
  ordercompleted: { color: '#fff', bgColor: '#389e0d', borderColor: '#389e0d', label: 'Order Completed' },
  ordercanceled: { color: '#fff', bgColor: '#ff4d4f', borderColor: '#ff4d4f', label: 'Order Canceled' },
  ordersuspended: { color: '#fff', bgColor: '#faad14', borderColor: '#faad14', label: 'Order Suspended' },
  orderreturnedtoprogress: { color: '#fff', bgColor: '#389e0d', borderColor: '#389e0d', label: 'Order Returned to Progress' },
  ordercreated: { color: '#fff', bgColor: '#389e0d', borderColor: '#389e0d', label: 'Order Created' },
  orderedited: { color: '#fff', bgColor: '#389e0d', borderColor: '#389e0d', label: 'Order Edited' },

  // 4xxx – Employee (Cyan)
  employeeassigned: { color: '#fff', bgColor: '#13c2c2', borderColor: '#13c2c2', label: 'Employee Assigned' },
  employeeremoved: { color: '#fff', bgColor: '#13c2c2', borderColor: '#13c2c2', label: 'Employee Removed' },

  // 5xxx – ClientPayment (Orange)
  clientpaymentadded: { color: '#fff', bgColor: '#fa8c16', borderColor: '#fa8c16', label: 'Client Payment Added' },
  clientpaymentedited: { color: '#fff', bgColor: '#fa8c16', borderColor: '#fa8c16', label: 'Client Payment Edited' },
  clientpaymentremoved: { color: '#fff', bgColor: '#fa8c16', borderColor: '#fa8c16', label: 'Client Payment Removed' },

  // 6xxx – ProviderPayment (Magenta)
  providerpaymentadded: { color: '#fff', bgColor: '#eb2f96', borderColor: '#eb2f96', label: 'Provider Payment Added' },
  providerpaymentedited: { color: '#fff', bgColor: '#eb2f96', borderColor: '#eb2f96', label: 'Provider Payment Edited' },
  providerpaymentremoved: { color: '#fff', bgColor: '#eb2f96', borderColor: '#eb2f96', label: 'Provider Payment Removed' },

  // 7xxx – UniqueNumber (Geekblue)
  uniquenumberadded: { color: '#fff', bgColor: '#2f54eb', borderColor: '#2f54eb', label: 'Unique Number Added' },
  uniquenumberremoved: { color: '#fff', bgColor: '#2f54eb', borderColor: '#2f54eb', label: 'Unique Number Removed' },
};

const DEFAULT_STYLE: LogTypeStyle = { color: '#fff', bgColor: '#595959', borderColor: '#595959', label: '' };

export function getLogTypeConfig(logType: string): LogTypeStyle {
  const key = logType.toLowerCase();
  const config = LOG_TYPE_CONFIG[key];
  return config || { ...DEFAULT_STYLE, label: logType };
}

export function isEditableNoteType(logType: string): boolean {
  return EDITABLE_NOTE_TYPES.includes(logType.toLowerCase());
}

export function isEditableOrderStateType(logType: string): boolean {
  return EDITABLE_ORDER_STATE_TYPES.includes(logType.toLowerCase());
}

export function getActionTypeFromLogType(logType: string): 'complete' | 'cancel' | 'suspend' | 'unsuspend' | null {
  return LOG_TYPE_TO_ACTION[logType.toLowerCase()] || null;
}

export function isActivityNoteType(logType: string): boolean {
  return ACTIVITY_NOTE_TYPES.includes(logType.toLowerCase());
}

export function formatLogDate(dateString: string): string {
  // Extract date parts directly from the string to avoid timezone conversion issues
  // Expected format: 2025-12-02T20:30:00 or 2025-12-02T20:30:00Z
  const datePart = dateString.split('T')[0]; // Get "2025-12-02"
  const [year, month, day] = datePart.split('-');
  return `${month}-${day}-${year}`;
}
