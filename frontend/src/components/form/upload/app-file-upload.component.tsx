'use client';

import React, { useState, useCallback, useMemo } from 'react';
import { UploadOutlined, EyeOutlined, DeleteOutlined, FileImageOutlined, FilePdfOutlined, FileOutlined, FileWordOutlined, FileExcelOutlined, ReadOutlined } from '@ant-design/icons';
import { Upload, Button, Tooltip, Image, Modal, Spin, Typography } from 'antd';
import clsx from 'clsx';
import { useTranslate } from '@/locales/use-locales';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { fetchPreviewArrayBuffer } from '@/utils/file-preview-fetch';
import { buildSpreadsheetPreviewSheets, type SpreadsheetPreviewSheet } from '@/utils/spreadsheet-preview';

const { Text } = Typography;

/* ─── File type helpers ─── */
const IMAGE_EXTENSIONS = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp', 'ico'];
const PDF_EXTENSIONS = ['pdf'];
const WORD_EXTENSIONS = ['doc', 'docx', 'pages'];
const EXCEL_EXTENSIONS = ['xls', 'xlsx', 'csv', 'numbers'];
const EPUB_EXTENSIONS = ['epub'];

function getFileExtension(filename: string): string {
  return (filename.split('.').pop() || '').toLowerCase();
}

function isImageFile(filename: string): boolean {
  return IMAGE_EXTENSIONS.includes(getFileExtension(filename));
}

function isPdfFile(filename: string): boolean {
  return PDF_EXTENSIONS.includes(getFileExtension(filename));
}

function isWordFile(filename: string): boolean {
  return WORD_EXTENSIONS.includes(getFileExtension(filename));
}

function isExcelFile(filename: string): boolean {
  return EXCEL_EXTENSIONS.includes(getFileExtension(filename));
}

function isEpubFile(filename: string): boolean {
  return EPUB_EXTENSIONS.includes(getFileExtension(filename));
}

function isPreviewable(filename: string): boolean {
  const ext = getFileExtension(filename);
  return [...IMAGE_EXTENSIONS, ...PDF_EXTENSIONS, ...WORD_EXTENSIONS, ...EXCEL_EXTENSIONS, ...EPUB_EXTENSIONS].includes(ext);
}

function getFileIcon(filename: string, size = 20) {
  const ext = getFileExtension(filename);
  if (IMAGE_EXTENSIONS.includes(ext)) return <FileImageOutlined style={{ fontSize: size, color: '#8b5cf6' }} />;
  if (PDF_EXTENSIONS.includes(ext)) return <FilePdfOutlined style={{ fontSize: size, color: '#ef4444' }} />;
  if (WORD_EXTENSIONS.includes(ext)) return <FileWordOutlined style={{ fontSize: size, color: '#3b82f6' }} />;
  if (EXCEL_EXTENSIONS.includes(ext)) return <FileExcelOutlined style={{ fontSize: size, color: '#22c55e' }} />;
  if (EPUB_EXTENSIONS.includes(ext)) return <ReadOutlined style={{ fontSize: size, color: '#a855f7' }} />;
  return <FileOutlined style={{ fontSize: size, color: '#6b7280' }} />;
}

function getFileTypeLabel(filename: string): string {
  const ext = getFileExtension(filename);
  if (IMAGE_EXTENSIONS.includes(ext)) return ext.toUpperCase();
  if (PDF_EXTENSIONS.includes(ext)) return 'PDF';
  if (WORD_EXTENSIONS.includes(ext)) return 'Word';
  if (EXCEL_EXTENSIONS.includes(ext)) return 'Excel';
  if (EPUB_EXTENSIONS.includes(ext)) return 'EPUB';
  return ext.toUpperCase() || 'FILE';
}

function getFileTypeColor(filename: string): string {
  const ext = getFileExtension(filename);
  if (IMAGE_EXTENSIONS.includes(ext)) return '#8b5cf6';
  if (PDF_EXTENSIONS.includes(ext)) return '#ef4444';
  if (WORD_EXTENSIONS.includes(ext)) return '#3b82f6';
  if (EXCEL_EXTENSIONS.includes(ext)) return '#22c55e';
  if (EPUB_EXTENSIONS.includes(ext)) return '#a855f7';
  return '#6b7280';
}

/* ─── Document preview modal content (inline) ─── */
function DocumentPreviewContent({ url, fileName, fileId }: { url: string; fileName: string; fileId?: number | string }) {
  if (isPdfFile(fileName)) {
    return <iframe src={url} style={{ width: '100%', height: '100%', border: 'none' }} title="PDF Preview" />;
  }

  if (isWordFile(fileName)) {
    return <WordPreviewContent url={url} fileId={fileId} />;
  }

  if (isExcelFile(fileName)) {
    return <ExcelPreviewContent url={url} fileId={fileId} />;
  }

  if (isEpubFile(fileName)) {
    return <EpubPreviewContent url={url} fileId={fileId} />;
  }

  return null;
}

function WordPreviewContent({ url, fileId }: { url: string; fileId?: number | string }) {
  const { t } = useTranslate('common');
  const containerRef = React.useRef<HTMLDivElement>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;
    async function render() {
      try {
        setLoading(true);
        const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);
        const blob = new Blob([arrayBuffer]);
        const { renderAsync } = await import('docx-preview');
        if (cancelled || !containerRef.current) return;
        containerRef.current.innerHTML = '';
        await renderAsync(blob, containerRef.current, undefined, {
          className: 'docx-preview-wrapper', inWrapper: true, ignoreWidth: false,
          ignoreHeight: false, ignoreFonts: false, breakPages: true, useBase64URL: true,
        });
        setLoading(false);
      } catch { if (!cancelled) { setError(t('filePreviewWordError')); setLoading(false); } }
    }
    render();
    return () => { cancelled = true; };
  }, [url, fileId]);

  if (error) return <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}><Text type="secondary">{error}</Text></div>;
  return (
    <div style={{ width: '100%', height: '100%', overflow: 'auto' }}>
      {loading && <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}><Spin size="large" /></div>}
      <div ref={containerRef} style={{ display: loading ? 'none' : 'block' }} />
    </div>
  );
}

function ExcelPreviewContent({ url, fileId }: { url: string; fileId?: number | string }) {
  const { t } = useTranslate('common');
  const [loading, setLoading] = useState(true);
  const [sheets, setSheets] = useState<SpreadsheetPreviewSheet[]>([]);
  const [activeSheet, setActiveSheet] = useState(0);
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;
    async function render() {
      try {
        setLoading(true);
        const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);
        const XLSX = await import('xlsx');
        if (cancelled) return;
        const workbook = XLSX.read(new Uint8Array(arrayBuffer), { type: 'array' });
        const parsed = buildSpreadsheetPreviewSheets(XLSX, workbook, t('emptySpreadsheetSheet'));
        setSheets(parsed);
        const firstSheetWithData = parsed.findIndex((sheet) => !sheet.isEmpty);
        setActiveSheet(firstSheetWithData >= 0 ? firstSheetWithData : 0);
        setLoading(false);
      } catch { if (!cancelled) { setError(t('filePreviewSpreadsheetError')); setLoading(false); } }
    }
    render();
    return () => { cancelled = true; };
  }, [url, fileId]);

  if (error) return <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}><Text type="secondary">{error}</Text></div>;
  if (loading) return <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}><Spin size="large" /></div>;

  return (
    <div style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
      {sheets.length > 1 && (
        <div style={{ display: 'flex', gap: 0, borderBottom: '2px solid #E2E8F0', background: '#F8FAFC', padding: '0 16px', flexShrink: 0 }}>
          {sheets.map((s, i) => (
            <button key={s.name} onClick={() => setActiveSheet(i)}
              style={{ padding: '8px 16px', border: 'none', borderBottom: i === activeSheet ? '2px solid #2563EB' : '2px solid transparent',
                background: 'transparent', color: i === activeSheet ? '#2563EB' : '#64748B',
                fontWeight: i === activeSheet ? 600 : 400, fontSize: 13, cursor: 'pointer', marginBottom: -2 }}>
              {s.name}
            </button>
          ))}
        </div>
      )}
      <div className="excel-preview-content" style={{ flex: 1, overflow: 'auto', padding: 16 }}
        dangerouslySetInnerHTML={{ __html: sheets[activeSheet]?.html || '' }} />
      <style>{`
        .excel-preview-content table { border-collapse: collapse; width: 100%; font-size: 13px; }
        .excel-preview-content td, .excel-preview-content th { border: 1px solid #E2E8F0; padding: 6px 10px; text-align: left; white-space: nowrap; }
        .excel-preview-content tr:first-child td { background: #F1F5F9; font-weight: 600; color: #334155; }
        .excel-preview-content tr:nth-child(even) td { background: #FAFBFC; }
      `}</style>
    </div>
  );
}

function EpubPreviewContent({ url, fileId }: { url: string; fileId?: number | string }) {
  const { t } = useTranslate('common');
  const viewerRef = React.useRef<HTMLDivElement>(null);
  const renditionRef = React.useRef<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;
    let book: any = null;
    async function render() {
      try {
        setLoading(true);
        const ePub = (await import('epubjs')).default;
        if (cancelled || !viewerRef.current) return;
        const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);
        book = ePub(arrayBuffer);
        const rendition = book.renderTo(viewerRef.current, { width: '100%', height: '100%', spread: 'auto', flow: 'paginated' });
        renditionRef.current = rendition;
        rendition.themes.default({ body: { 'font-family': '-apple-system, BlinkMacSystemFont, sans-serif !important', 'line-height': '1.7 !important', 'padding': '20px 40px !important' } });
        await rendition.display();
        if (!cancelled) setLoading(false);
      } catch { if (!cancelled) { setError(t('filePreviewEpubError')); setLoading(false); } }
    }
    render();
    return () => { cancelled = true; if (renditionRef.current) try { renditionRef.current.destroy(); } catch {} if (book) try { book.destroy(); } catch {} };
  }, [url, fileId]);

  if (error) return <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}><Text type="secondary">{error}</Text></div>;

  return (
    <div style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column', position: 'relative' }}>
      {loading && <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 10, background: '#fff' }}><Spin size="large" /></div>}
      <div ref={viewerRef} style={{ flex: 1, overflow: 'hidden' }} />
      {!loading && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: 16, padding: '10px 0', borderTop: '1px solid #E2E8F0', background: '#F8FAFC' }}>
          <Button size="small" onClick={() => renditionRef.current?.prev()}>← {t('previous')}</Button>
          <Button size="small" onClick={() => renditionRef.current?.next()}>{t('next')} →</Button>
        </div>
      )}
    </div>
  );
}

/* ─── File Preview Thumbnail Card ─── */
function FilePreviewThumbnail({ file, onDelete }: { file: any; onDelete: (file: any) => void }) {
  const { t } = useTranslate('common');
  const [imagePreviewOpen, setImagePreviewOpen] = useState(false);
  const [docPreviewOpen, setDocPreviewOpen] = useState(false);

  const isImage = isImageFile(file.name);
  const canPreviewDoc = !isImage && isPreviewable(file.name);
  const fileId = file.fileId ?? file.id;

  const thumbnailUrl = useMemo(() => {
    if (isImage && file.url) return file.url;
    if (isImage && file.originFileObj) return URL.createObjectURL(file.originFileObj);
    return null;
  }, [isImage, file.url, file.originFileObj]);

  const previewUrl = file.url || (file.originFileObj ? URL.createObjectURL(file.originFileObj) : null);

  const handlePreview = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (isImage) setImagePreviewOpen(true);
    else if (canPreviewDoc && previewUrl) setDocPreviewOpen(true);
  };

  return (
    <>
      <div
        style={{
          width: 110, border: '1px solid #E5E7EB', borderRadius: 10,
          overflow: 'hidden', background: '#fff', transition: 'box-shadow 0.2s, border-color 0.2s',
          position: 'relative', flexShrink: 0,
        }}
        onMouseEnter={(e) => { e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.08)'; e.currentTarget.style.borderColor = '#93c5fd'; }}
        onMouseLeave={(e) => { e.currentTarget.style.boxShadow = 'none'; e.currentTarget.style.borderColor = '#E5E7EB'; }}
      >
        <div
          style={{
            height: 80, display: 'flex', alignItems: 'center', justifyContent: 'center',
            background: isImage ? '#f8fafc' : '#f9fafb', position: 'relative', overflow: 'hidden',
            cursor: isImage || canPreviewDoc ? 'pointer' : 'default',
          }}
          onClick={handlePreview}
        >
          {thumbnailUrl ? (
            <img src={thumbnailUrl} alt={file.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }}
              onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = 'none'; }} />
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
              {getFileIcon(file.name, 24)}
              <span style={{ fontSize: 9, fontWeight: 600, color: getFileTypeColor(file.name), textTransform: 'uppercase', letterSpacing: 0.5 }}>
                {getFileTypeLabel(file.name)}
              </span>
            </div>
          )}

          <div
            style={{
              position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.45)',
              display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
              opacity: 0, transition: 'opacity 0.2s',
            }}
            onMouseEnter={(e) => { e.currentTarget.style.opacity = '1'; }}
            onMouseLeave={(e) => { e.currentTarget.style.opacity = '0'; }}
          >
            {(isImage || canPreviewDoc) && (
              <Tooltip title={t('preview')}>
                <Button type="primary" shape="circle" size="small" icon={<EyeOutlined />} onClick={handlePreview} />
              </Tooltip>
            )}
            <Tooltip title="Remove">
              <Button shape="circle" size="small" danger icon={<DeleteOutlined />}
                onClick={(e) => { e.stopPropagation(); onDelete(file); }} style={{ background: 'white' }} />
            </Tooltip>
          </div>
        </div>

        <Tooltip title={file.name}>
          <div style={{
            padding: '5px 8px', borderTop: '1px solid #f3f4f6', fontSize: 10,
            color: '#374151', overflow: 'hidden', textOverflow: 'ellipsis',
            whiteSpace: 'nowrap', textAlign: 'center',
          }}>
            {file.name}
          </div>
        </Tooltip>
      </div>

      {isImage && thumbnailUrl && (
        <Image src={thumbnailUrl} alt={file.name} style={{ display: 'none' }}
          preview={{ visible: imagePreviewOpen, onVisibleChange: setImagePreviewOpen, src: file.url || thumbnailUrl }} />
      )}

      {canPreviewDoc && previewUrl && (
        <Modal open={docPreviewOpen} onCancel={() => setDocPreviewOpen(false)} footer={null}
          width="85vw" centered destroyOnHidden
          title={
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              {getFileIcon(file.name, 22)}
              <div>
                <div style={{ fontSize: 14, fontWeight: 600, color: '#0F172A' }}>{file.name}</div>
                <div style={{ fontSize: 11, color: '#94A3B8', fontWeight: 400 }}>{getFileTypeLabel(file.name)} Document</div>
              </div>
            </div>
          }
          styles={{ body: { height: '75vh', padding: 0, overflow: 'hidden' }, content: { borderRadius: 16, overflow: 'hidden' } }}
        >
          <div style={{ height: '100%', overflow: 'auto' }}>
            <DocumentPreviewContent url={previewUrl} fileName={file.name} fileId={fileId} />
          </div>
        </Modal>
      )}
    </>
  );
}

/* ─── Props ─── */
interface AppFileUploadProps {
  fileList: any[];
  onChange: (info: any) => void;
  onRemove?: (file: any) => boolean | Promise<boolean>;
  multiple?: boolean;
  className?: string;
  disabled?: boolean;
  accept?: string;
  showDeleteConfirm?: boolean;
  onDeleteExistingFile?: (file: any) => Promise<void>;
}

/* ─── Main Component ─── */
export default function AppFileUploadComponent({
  fileList,
  onChange,
  onRemove,
  multiple = true,
  className = '',
  disabled = false,
  accept,
  showDeleteConfirm = false,
  onDeleteExistingFile,
}: AppFileUploadProps) {
  const { t } = useTranslate('common');
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [fileToDelete, setFileToDelete] = useState<any | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const uploadProps = {
    multiple, disabled, fileList, accept,
    beforeUpload: () => false,
    onChange,
    showUploadList: false,
  };

  const handleDeleteClick = useCallback(async (file: any) => {
    setFileToDelete(file);
    setDeleteModalOpen(true);
  }, []);

  const handleConfirmDelete = useCallback(async () => {
    if (fileToDelete) {
      setIsDeleting(true);
      try {
        const isExistingFile = fileToDelete.url && !fileToDelete.originFileObj;
        if (isExistingFile && onDeleteExistingFile) {
          await onDeleteExistingFile(fileToDelete);
        }
        if (onRemove) {
          await onRemove(fileToDelete);
        }
        const newFileList = fileList.filter((f: any) => f.uid !== fileToDelete.uid);
        onChange({ fileList: newFileList });
        setDeleteModalOpen(false);
        setFileToDelete(null);
      } finally {
        setIsDeleting(false);
      }
    }
  }, [fileToDelete, onDeleteExistingFile, onRemove, fileList, onChange]);

  const handleCancelDelete = useCallback(() => {
    setDeleteModalOpen(false);
    setFileToDelete(null);
  }, []);

  return (
    <div className={clsx('flex flex-col gap-3', className)}>
      <Upload {...uploadProps}>
        <div
          className='flex items-center justify-center gap-2 h-[40px] px-4 border border-dashed cursor-pointer hover:border-[#3B82F6] transition-colors'
          style={{ borderColor: '#D9D9D9', backgroundColor: '#FAFAFA', borderRadius: '8px' }}>
          <UploadOutlined style={{ color: '#666' }} />
          <span className='text-[12px] text-gray-600'>{t('clickOrDragToUpload')}</span>
        </div>
      </Upload>

      {fileList.length > 0 && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 10 }}>
          {fileList.map((file: any) => (
            <FilePreviewThumbnail key={file.uid} file={file} onDelete={handleDeleteClick} />
          ))}
        </div>
      )}

      <ConfirmDeleteModalComponent
        open={deleteModalOpen} onCancel={handleCancelDelete} onConfirm={handleConfirmDelete}
        title={t('deleteFile')} loading={isDeleting}
        description={<>{t('areYouSureDeleteFile')} <strong>&quot;{fileToDelete?.name}&quot;</strong>?</>}
      />
    </div>
  );
}
