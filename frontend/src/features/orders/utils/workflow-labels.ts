type TranslateFn = (key: string, options?: Record<string, unknown>) => string;

const WORKFLOW_LABEL_KEYS: Record<string, string> = {
  'order placement': 'workflow.stages.orderPlacement',
  'pre-production': 'workflow.stages.preProduction',
  manufacturing: 'workflow.stages.manufacturing',
  'post-production': 'workflow.stages.postProduction',
  'shipping & delivery': 'workflow.stages.shippingDelivery',

  'confirming product specifications': 'workflow.steps.confirmingProductSpecifications',
  'purchase order sending': 'workflow.steps.purchaseOrderSending',
  'price, quantity, and lead time negotiation': 'workflow.steps.priceQuantityLeadTimeNegotiation',
  'getting order confirmation from factory': 'workflow.steps.gettingOrderConfirmationFromFactory',
  'pre-production sample approval': 'workflow.steps.preProductionSampleApproval',
  'raw materials availability and colour swatches confirmation': 'workflow.steps.rawMaterialsAvailabilityColourSwatchesConfirmation',
  'receiving production timeline from factory': 'workflow.steps.receivingProductionTimelineFromFactory',
  'initial deposit payment': 'workflow.steps.initialDepositPayment',
  'raw material procurement': 'workflow.steps.rawMaterialProcurement',
  'initial production stage': 'workflow.steps.initialProductionStage',
  'mid-production': 'workflow.steps.midProduction',
  'mid-production inspection': 'workflow.steps.midProductionInspection',
  'final production': 'workflow.steps.finalProduction',
  'final production quality control': 'workflow.steps.finalProductionQualityControl',
  '3rd party inspection': 'workflow.steps.thirdPartyInspection',
  'final product approval': 'workflow.steps.finalProductApproval',
  'receive production completion notice': 'workflow.steps.receiveProductionCompletionNotice',
  'arrange shipment logistics': 'workflow.steps.arrangeShipmentLogistics',
  'get packing list & invoice': 'workflow.steps.getPackingListInvoice',
  'book freight forwarder / shipping agent': 'workflow.steps.bookFreightForwarderShippingAgent',
  'goods loaded & shipped': 'workflow.steps.goodsLoadedShipped',
  'receive tracking / bill of lading': 'workflow.steps.receiveTrackingBillOfLading',
  'customs clearance': 'workflow.steps.customsClearance',
  'final delivery to your customer': 'workflow.steps.finalDeliveryToYourCustomer',
};

const KNOWN_LOG_TITLE_KEYS: Record<string, string> = {
  'order created': 'orderCreated',
  'order completed': 'orderCompleted',
  'order canceled': 'orderCanceled',
  'order suspended': 'orderSuspended',
  'order returned to progress': 'orderReturnedToProgress',
  'order edited': 'orderEdited',
  'step completed': 'stepCompleted',
  'step forward': 'stepForward',
  'step backward': 'stepBackward',
  'note added': 'noteAdded',
  'note edited': 'noteEdited',
  'note removed': 'noteRemoved',
  'employee assigned': 'employeeAssigned',
  'employee removed': 'employeeRemoved',
};

function normalizeLabel(value?: string | null) {
  return value?.trim().replace(/\s+/g, ' ').toLowerCase() ?? '';
}

export function translateWorkflowLabel(value: string | null | undefined, t: TranslateFn) {
  if (!value) return value ?? '';
  const key = WORKFLOW_LABEL_KEYS[normalizeLabel(value)];
  return key ? t(key) : value;
}

export function translateKnownLogTitle(value: string | null | undefined, t: TranslateFn) {
  if (!value) return value ?? '';

  const movedToStepMatch = value.match(/^Moved to step:\s*(.+)$/i);
  if (movedToStepMatch) {
    return `${t('logMovedToStep')}: ${translateWorkflowLabel(movedToStepMatch[1], t)}`;
  }

  const orderLogMatch = value.match(/^Order\s+(Created|Completed|Canceled|Suspended|Returned to Progress|Edited)\s+for order\s+"(.+)"$/i);
  if (orderLogMatch) {
    const key = `logOrder${orderLogMatch[1].replace(/\s+/g, '')}ForOrder`;
    return t(key, { title: orderLogMatch[2] });
  }

  const employeeAssignedTitleMatch = value.match(/^Employee Assigned:\s*(.+)$/i);
  if (employeeAssignedTitleMatch) {
    return t('logEmployeeAssigned', { name: employeeAssignedTitleMatch[1] });
  }

  const employeeAssignedForOrderMatch = value.match(/^Employee Assigned for order\s+"(.+)"\s+[—-]\s+(.+)$/i);
  if (employeeAssignedForOrderMatch) {
    return t('logEmployeeAssignedForOrder', { title: employeeAssignedForOrderMatch[1], name: employeeAssignedForOrderMatch[2] });
  }

  const employeeRemovedForOrderMatch = value.match(/^Employee Removed from order\s+"(.+)"\s+[—-]\s+(.+)$/i);
  if (employeeRemovedForOrderMatch) {
    return t('logEmployeeRemovedFromOrder', { title: employeeRemovedForOrderMatch[1], name: employeeRemovedForOrderMatch[2] });
  }

  const key = KNOWN_LOG_TITLE_KEYS[normalizeLabel(value)];
  return key ? t(key) : value;
}
