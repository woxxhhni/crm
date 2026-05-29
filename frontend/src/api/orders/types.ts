import { ClientRecord } from '@/api/clients/types';

export interface OrderCreateRequest {
  description: string;
  buyCurrency: string;
  buyAmount?: string;
  sellCurrency: string;
  sellAmount?: string;
  clientId: number;
  providerId: number;

  date: string;
  title: string;
  employees?: string[];

  BFiles: File[];
  SFiles: File[];
}
export interface OrderRecord {
  id?: number;
  orderNumber?: string;
  title?: string;
  orderDate?: string;
  description?: string;
  buyCurrency?: string;
  buyAmount?: number;
  sellCurrency?: string;
  sellAmount?: number;
  clientBalance?: number;
  providerBalance?: number;
  balancesLastCalculatedAt?: string;
  clientId?: number;
  clientName?: string;
  clientEmail?: string;
  providerId?: number;
  providerName?: string;
  providerEmail?: string;
  currentStepId?: number;
  currentStepName?: string;
  currentStageId?: number;
  currentStageName?: string;
  createdByUserId?: number;
  createdAt?: string;
  updatedAt?: string;
  completedAt?: string;
  canceledAt?: string;
  suspendedAt?: string;
  firstActionDate?: string;
  status?: string;
  buyInvoiceLinks?: any;
  sellInvoiceLinks?: any;
  employees?: any[];
  stageAssignments?: OrderStageAssignee[];
  uniqueNumbers?: any[];
  steps?: StepWithLogs[];
  extraProviders?: ExtraProviderRecord[];
}

export interface OrderStageAssignee {
  stageId: number;
  stageName: string;
  stageOrderPosition: number;
  employeeId?: number | null;
  employeeName?: string | null;
  employeeEmail?: string | null;
  profileFileId?: number | null;
  profileUrl?: string | null;
  assignedAt?: string | null;
  assignedByUserId?: number | null;
}
export interface OrdersListResponse {
  page: number;
  pageSize: number;
  total: number;
  items: OrderRecord[];
}

// Payment Types
export interface PaymentTransaction {
  id: number;
  amount: number;
  remainingAmount: number;
  paymentType: string;
  userFullName: string;
  userProfileUrl: string;
  createdAt: string;
}

export interface PaymentsListResponse {
  name: string;
  totalBalance: number;
  paidAmount: number;
  currentBalance: number;
  currency: string;
  transactions: PaymentTransaction[];
}

export interface PaymentAttachment {
  id: number;
  name: string;
  url?: string;
}

export interface PaymentDetailResponse {
  id: number;
  amount: number;
  paymentType: string;
  description?: string;
  userFullName: string;
  userProfileUrl: string;
  createdAt: string;
  files: PaymentAttachment[];
}

export interface PaymentCreateDTO {
  amount: number;
  paymentType: string;
  description?: string;
  files?: File[];
}

export interface PaymentUpdateDTO {
  amount: number;
  paymentType: string;
  description?: string;
  files?: (File | number)[]; // Can contain both new File objects and existing file IDs
  removedFileIds?: number[];
}

// Activity Log Types
export interface LogFile {
  id: number;
  name: string | null;
  url: string | null;
}

export interface ActivityLog {
  id: number;
  orderId: number;
  stepId: number | null;
  stepName: string | null;
  stageId: number | null;
  stageName: string | null;
  noteId: number | null;
  noteTitle: string | null;
  logType: string;
  title: string;
  description: string;
  fromStepId: number | null;
  fromStepName: string | null;
  toStepId: number | null;
  toStepName: string | null;
  actorUserId: number;
  actorFullName: string;
  actorProfileFileId: number | null;
  actorProfileUrl: string;
  logDate: string;
  stepExitedAt?: string | null;
  files: LogFile[];
}

export interface StepWithLogs {
  stepId: number;
  stepName: string;
  stageId: number;
  stageName: string;
  stepOrderPosition?: number;
  stepEnteredAt?: string | null;
  stepExitedAt?: string | null;
  isCompleted: boolean;
  isCurrent?: boolean;
  logs: ActivityLog[];
}

export interface StepOption {
  id: number;
  stageId: number;
  name: string;
  orderPosition: number;
  description: string;
  isFinalStep: boolean;
  isActive: boolean;
  stageName: string;
}

export interface NoteDetailResponse {
  id: number;
  date: string;
  title: string;
  description: string;
  userFullName: string;
  userProfileFileId: number | null;
  userProfileUrl: string | null;
  createdAt: string;
  clientName: string;
  providerName: string;
  files: { id: number; name: string; url?: string }[];
}

// Dashboard Summary Types
export interface StageSummary {
  name: string;
  percent: number;
}

export interface OrdersSummaryResponse {
  total: number;
  inProgress: number;
  canceled: number;
  completed: number;
  suspended: number;
  stageSummaries: StageSummary[];
}

// Extra Providers
export interface ExtraProviderRecord {
  id: number;
  providerId: number;
  providerName: string;
  amount: number;
  currency: string;
  totalBalance?: number;
  paidAmount?: number;
  remainingAmount?: number;
}

export interface ExtraProviderCreateDTO {
  providerId: number;
  amount: number;
  currency: string;
}

export interface ExtraProviderPaymentSummaryResponse {
  name: string;
  totalBalance: number;
  paidAmount: number;
  currentBalance: number;
  currency: string;
  transactions: PaymentTransaction[];
}

export interface ExtraProviderPaymentDetailResponse {
  id: number;
  amount: number;
  paymentType: string;
  description?: string;
  userFullName: string;
  userProfileFileId?: number;
  userProfileUrl?: string;
  createdAt: string;
  files: {
    id: number;
    name: string;
    url: string;
  }[];
}
