import type { UserRole } from '@/api/general/types';

// All available permissions in the system
export const PERMISSIONS = {
  // Order permissions
  ORDER_CREATE: 'order:create',
  ORDER_EDIT: 'order:edit',
  ORDER_DELETE: 'order:delete',
  ORDER_CHANGE_STEP: 'order:change_step',
  ORDER_COMPLETE_STEP: 'order:complete_step',
  ORDER_ADD_NOTE: 'order:add_note',
  ORDER_CHANGE_STATUS: 'order:change_status', // complete, suspend, cancel
  ORDER_VIEW_CLIENT_PAYMENTS: 'order:view_client_payments',
  ORDER_VIEW_PROVIDER_PAYMENTS: 'order:view_provider_payments',

  // Employee assignment permissions
  ORDER_ASSIGN_EMPLOYEE: 'order:assign_employee',
  ORDER_VIEW_ASSIGNED_EMPLOYEES: 'order:view_assigned_employees',
  ORDER_REMOVE_ASSIGNED_EMPLOYEE: 'order:remove_assigned_employee',

  // Note permissions
  NOTE_EDIT: 'note:edit',

  // Employee management permissions
  EMPLOYEE_VIEW: 'employee:view',
  EMPLOYEE_VIEW_DETAIL: 'employee:view_detail',
  EMPLOYEE_CREATE: 'employee:create',
  EMPLOYEE_EDIT: 'employee:edit',
  EMPLOYEE_DELETE: 'employee:delete',

  // Client permissions
  CLIENT_CREATE: 'client:create',
  CLIENT_EDIT: 'client:edit',
  CLIENT_DELETE: 'client:delete',

  // Provider permissions
  PROVIDER_CREATE: 'provider:create',
  PROVIDER_EDIT: 'provider:edit',
  PROVIDER_DELETE: 'provider:delete',

  // File group permissions
  FILE_GROUP_DELETE: 'file_group:delete',
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];

// Role-based permission mapping
// Admin has all permissions
// Manager has all permissions except employee management
// Employee has restricted permissions
const ADMIN_PERMISSIONS: Permission[] = Object.values(PERMISSIONS);

const MANAGER_PERMISSIONS: Permission[] = Object.values(PERMISSIONS).filter(
  (permission) =>
    permission !== PERMISSIONS.EMPLOYEE_VIEW &&
    permission !== PERMISSIONS.EMPLOYEE_VIEW_DETAIL &&
    permission !== PERMISSIONS.EMPLOYEE_CREATE &&
    permission !== PERMISSIONS.EMPLOYEE_EDIT &&
    permission !== PERMISSIONS.EMPLOYEE_DELETE
);

// Employee: can only complete steps and add notes (restricted view-only for clients/providers)
const EMPLOYEE_PERMISSIONS: Permission[] = [
  PERMISSIONS.ORDER_COMPLETE_STEP,
  PERMISSIONS.ORDER_ADD_NOTE,
];

export const ROLE_PERMISSIONS: Record<UserRole, Permission[]> = {
  admin: ADMIN_PERMISSIONS,
  manager: MANAGER_PERMISSIONS,
  employee: EMPLOYEE_PERMISSIONS,
};

// Helper to check if a role has a specific permission
export function hasPermission(role: UserRole | undefined, permission: Permission): boolean {
  if (!role) return false;
  return ROLE_PERMISSIONS[role]?.includes(permission) ?? false;
}

// Helper to check if a role has any of the specified permissions
export function hasAnyPermission(role: UserRole | undefined, permissions: Permission[]): boolean {
  if (!role) return false;
  return permissions.some((permission) => hasPermission(role, permission));
}

// Helper to check if a role has all of the specified permissions
export function hasAllPermissions(role: UserRole | undefined, permissions: Permission[]): boolean {
  if (!role) return false;
  return permissions.every((permission) => hasPermission(role, permission));
}
