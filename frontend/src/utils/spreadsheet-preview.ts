type XlsxModule = typeof import('xlsx');

export type SpreadsheetPreviewSheet = {
  name: string;
  html: string;
  isEmpty: boolean;
};

function escapeHtml(value: string) {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function emptySheetHtml(message: string) {
  return `<div style="height: 260px; display: flex; align-items: center; justify-content: center; color: #64748B; font-size: 13px;">${escapeHtml(message)}</div>`;
}

export function buildSpreadsheetPreviewSheets(
  XLSX: XlsxModule,
  workbook: { SheetNames: string[]; Sheets: Record<string, unknown> },
  emptyMessage: string,
): SpreadsheetPreviewSheet[] {
  const sheetNames = workbook.SheetNames.length > 0 ? workbook.SheetNames : ['Sheet 1'];

  return sheetNames.map((name) => {
    const sheet = workbook.Sheets[name] as Record<string, unknown> | undefined;
    if (!sheet || !sheet['!ref']) {
      return { name, html: emptySheetHtml(emptyMessage), isEmpty: true };
    }

    try {
      return {
        name,
        html: XLSX.utils.sheet_to_html(sheet as any, { editable: false }),
        isEmpty: false,
      };
    } catch {
      return { name, html: emptySheetHtml(emptyMessage), isEmpty: true };
    }
  });
}
