'use client';

import { useState, useRef, useEffect, useCallback } from 'react';
import { Modal, Button, Tooltip, Image, Spin, Typography } from 'antd';
import {
    FileImageOutlined,
    FilePdfOutlined,
    FileWordOutlined,
    FileExcelOutlined,
    FileZipOutlined,
    FileTextOutlined,
    FileOutlined,
    ReadOutlined,
    DownloadOutlined,
    EyeOutlined,
    CloseOutlined,
    LeftOutlined,
    RightOutlined,
    ExpandOutlined,
} from '@ant-design/icons';
import { useTranslate } from '@/locales/use-locales';
import { fetchPreviewArrayBuffer } from '@/utils/file-preview-fetch';
import { buildSpreadsheetPreviewSheets, type SpreadsheetPreviewSheet } from '@/utils/spreadsheet-preview';

const { Text } = Typography;

interface FileItem {
    id?: number;
    fileId?: number;
    fileName: string;
    url: string;
}

interface FilePreviewCardProps {
    file: FileItem;
    onDelete?: () => void;
    canDelete?: boolean;
}

const IMAGE_EXTENSIONS = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp', 'ico'];
const PDF_EXTENSIONS = ['pdf'];
const WORD_EXTENSIONS = ['doc', 'docx'];
const EXCEL_EXTENSIONS = ['xls', 'xlsx'];
const CSV_EXTENSIONS = ['csv'];
const EPUB_EXTENSIONS = ['epub'];
const ARCHIVE_EXTENSIONS = ['zip', 'rar', '7z', 'tar', 'gz'];

// Files that can be previewed in the browser
const PREVIEWABLE_EXTENSIONS = [...IMAGE_EXTENSIONS, ...PDF_EXTENSIONS, ...WORD_EXTENSIONS, ...EXCEL_EXTENSIONS, ...CSV_EXTENSIONS, ...EPUB_EXTENSIONS];

function getFileExtension(fileName: string): string {
    return (fileName.split('.').pop() || '').toLowerCase();
}

function isImageFile(fileName: string): boolean {
    return IMAGE_EXTENSIONS.includes(getFileExtension(fileName));
}

function isPdfFile(fileName: string): boolean {
    return PDF_EXTENSIONS.includes(getFileExtension(fileName));
}

function isWordFile(fileName: string): boolean {
    return WORD_EXTENSIONS.includes(getFileExtension(fileName));
}

function isExcelFile(fileName: string): boolean {
    return EXCEL_EXTENSIONS.includes(getFileExtension(fileName));
}

function isCsvFile(fileName: string): boolean {
    return CSV_EXTENSIONS.includes(getFileExtension(fileName));
}

function isEpubFile(fileName: string): boolean {
    return EPUB_EXTENSIONS.includes(getFileExtension(fileName));
}

function isPreviewable(fileName: string): boolean {
    return PREVIEWABLE_EXTENSIONS.includes(getFileExtension(fileName));
}

function getFileIcon(fileName: string) {
    const ext = getFileExtension(fileName);
    if (IMAGE_EXTENSIONS.includes(ext)) return <FileImageOutlined style={{ fontSize: 28, color: '#8b5cf6' }} />;
    if (PDF_EXTENSIONS.includes(ext)) return <FilePdfOutlined style={{ fontSize: 28, color: '#ef4444' }} />;
    if (WORD_EXTENSIONS.includes(ext)) return <FileWordOutlined style={{ fontSize: 28, color: '#3b82f6' }} />;
    if (EXCEL_EXTENSIONS.includes(ext)) return <FileExcelOutlined style={{ fontSize: 28, color: '#22c55e' }} />;
    if (EPUB_EXTENSIONS.includes(ext)) return <ReadOutlined style={{ fontSize: 28, color: '#a855f7' }} />;
    if (ARCHIVE_EXTENSIONS.includes(ext)) return <FileZipOutlined style={{ fontSize: 28, color: '#f59e0b' }} />;
    if (ext === 'txt' || ext === 'csv') return <FileTextOutlined style={{ fontSize: 28, color: '#6b7280' }} />;
    return <FileOutlined style={{ fontSize: 28, color: '#6b7280' }} />;
}

function getFileTypeLabel(fileName: string): string {
    const ext = getFileExtension(fileName);
    if (WORD_EXTENSIONS.includes(ext)) return 'Word Document';
    if (EXCEL_EXTENSIONS.includes(ext)) return 'Excel Spreadsheet';
    if (CSV_EXTENSIONS.includes(ext)) return 'CSV File';
    if (PDF_EXTENSIONS.includes(ext)) return 'PDF Document';
    if (EPUB_EXTENSIONS.includes(ext)) return 'EPUB eBook';
    return 'Document';
}

/* ─── Word Document Preview ─── */
function WordPreview({ url, fileName, fileId }: { url: string; fileName: string; fileId?: number }) {
    const { t } = useTranslate('common');
    const containerRef = useRef<HTMLDivElement>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        async function renderDoc() {
            try {
                setLoading(true);
                setError(null);
                const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);
                const blob = new Blob([arrayBuffer]);

                // Dynamic import to avoid SSR issues
                const { renderAsync } = await import('docx-preview');

                if (cancelled || !containerRef.current) return;
                containerRef.current.innerHTML = '';
                await renderAsync(blob, containerRef.current, undefined, {
                    className: 'docx-preview-wrapper',
                    inWrapper: true,
                    ignoreWidth: false,
                    ignoreHeight: false,
                    ignoreFonts: false,
                    breakPages: true,
                    useBase64URL: true,
                    renderHeaders: true,
                    renderFooters: true,
                    renderFootnotes: true,
                    renderEndnotes: true,
                });
                setLoading(false);
            } catch (err) {
                if (!cancelled) {
                    setError(t('filePreviewWordError'));
                    setLoading(false);
                }
            }
        }
        renderDoc();
        return () => { cancelled = true; };
    }, [url, fileId]);

    return (
        <div style={{ width: '100%', height: '100%', overflow: 'auto' }}>
            {loading && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: 300, gap: 12 }}>
                    <Spin size="large" />
                    <Text style={{ color: '#64748B' }}>{t('renderingDocument')}</Text>
                </div>
            )}
            {error && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: 300, gap: 12 }}>
                    <FileWordOutlined style={{ fontSize: 48, color: '#94A3B8' }} />
                    <Text style={{ color: '#64748B' }}>{error}</Text>
                    <Button type="primary" icon={<DownloadOutlined />} onClick={() => window.open(url, '_blank')}>
                        {t('downloadFile')}
                    </Button>
                </div>
            )}
            <div ref={containerRef} style={{ display: loading || error ? 'none' : 'block' }} />
        </div>
    );
}

/* ─── Excel/CSV Preview ─── */
function ExcelPreview({ url, fileName, fileId }: { url: string; fileName: string; fileId?: number }) {
    const { t } = useTranslate('common');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [sheets, setSheets] = useState<SpreadsheetPreviewSheet[]>([]);
    const [activeSheet, setActiveSheet] = useState(0);

    useEffect(() => {
        let cancelled = false;
        async function renderSheet() {
            try {
                setLoading(true);
                setError(null);
                const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);

                // Dynamic import to avoid SSR issues
                const XLSX = await import('xlsx');

                if (cancelled) return;

                const workbook = XLSX.read(new Uint8Array(arrayBuffer), { type: 'array' });
                const parsedSheets = buildSpreadsheetPreviewSheets(XLSX, workbook, t('emptySpreadsheetSheet'));

                setSheets(parsedSheets);
                const firstSheetWithData = parsedSheets.findIndex((sheet) => !sheet.isEmpty);
                setActiveSheet(firstSheetWithData >= 0 ? firstSheetWithData : 0);
                setLoading(false);
            } catch (err) {
                if (!cancelled) {
                    setError(t('filePreviewSpreadsheetError'));
                    setLoading(false);
                }
            }
        }
        renderSheet();
        return () => { cancelled = true; };
    }, [url, fileId]);

    if (loading) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: 300, gap: 12 }}>
                <Spin size="large" />
                <Text style={{ color: '#64748B' }}>{t('loadingSpreadsheet')}</Text>
            </div>
        );
    }

    if (error) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: 300, gap: 12 }}>
                <FileExcelOutlined style={{ fontSize: 48, color: '#94A3B8' }} />
                <Text style={{ color: '#64748B' }}>{error}</Text>
                <Button type="primary" icon={<DownloadOutlined />} onClick={() => window.open(url, '_blank')}>
                    {t('downloadFile')}
                </Button>
            </div>
        );
    }

    return (
        <div style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
            {/* Sheet tabs */}
            {sheets.length > 1 && (
                <div style={{
                    display: 'flex', gap: 0, borderBottom: '2px solid #E2E8F0',
                    background: '#F8FAFC', padding: '0 16px', flexShrink: 0,
                }}>
                    {sheets.map((sheet, idx) => (
                        <button
                            key={sheet.name}
                            onClick={() => setActiveSheet(idx)}
                            style={{
                                padding: '10px 20px',
                                border: 'none',
                                borderBottom: idx === activeSheet ? '2px solid #2563EB' : '2px solid transparent',
                                background: 'transparent',
                                color: idx === activeSheet ? '#2563EB' : '#64748B',
                                fontWeight: idx === activeSheet ? 600 : 400,
                                fontSize: 13,
                                cursor: 'pointer',
                                marginBottom: -2,
                                transition: 'all 0.15s',
                            }}
                        >
                            {sheet.name}
                        </button>
                    ))}
                </div>
            )}

            {/* Sheet content */}
            <div
                style={{ flex: 1, overflow: 'auto', padding: 16 }}
                dangerouslySetInnerHTML={{ __html: sheets[activeSheet]?.html || '' }}
            />

            {/* Excel table styles */}
            <style>{`
                .excel-preview-content table {
                    border-collapse: collapse;
                    width: 100%;
                    font-size: 13px;
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                }
                .excel-preview-content td, .excel-preview-content th {
                    border: 1px solid #E2E8F0;
                    padding: 8px 12px;
                    text-align: left;
                    white-space: nowrap;
                }
                .excel-preview-content th, .excel-preview-content tr:first-child td {
                    background: #F1F5F9;
                    font-weight: 600;
                    color: #334155;
                    position: sticky;
                    top: 0;
                    z-index: 1;
                }
                .excel-preview-content tr:nth-child(even) td {
                    background: #FAFBFC;
                }
                .excel-preview-content tr:hover td {
                    background: #EFF6FF;
                }
            `}</style>
        </div>
    );
}

/* ─── PDF Preview ─── */
function PdfPreview({ url }: { url: string }) {
    return (
        <iframe
            src={url}
            style={{ width: '100%', height: '100%', border: 'none' }}
            title="PDF Preview"
        />
    );
}

/* ─── EPUB Preview ─── */
function EpubPreview({ url, fileName, fileId }: { url: string; fileName: string; fileId?: number }) {
    const { t } = useTranslate('common');
    const viewerRef = useRef<HTMLDivElement>(null);
    const renditionRef = useRef<any>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [currentChapter, setCurrentChapter] = useState('');

    useEffect(() => {
        let cancelled = false;
        let book: any = null;

        async function renderEpub() {
            try {
                setLoading(true);
                setError(null);

                const ePub = (await import('epubjs')).default;

                if (cancelled || !viewerRef.current) return;

                // Fetch the file as ArrayBuffer
                const arrayBuffer = await fetchPreviewArrayBuffer(url, fileId);

                book = ePub(arrayBuffer);

                const rendition = book.renderTo(viewerRef.current, {
                    width: '100%',
                    height: '100%',
                    spread: 'auto',
                    flow: 'paginated',
                });

                renditionRef.current = rendition;

                // Style the epub content
                rendition.themes.default({
                    body: {
                        'font-family': '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif !important',
                        'line-height': '1.7 !important',
                        'color': '#1E293B !important',
                        'padding': '20px 40px !important',
                    },
                    'p': { 'margin-bottom': '0.8em !important' },
                    'h1, h2, h3, h4': { 'color': '#0F172A !important' },
                    'img': { 'max-width': '100% !important', 'height': 'auto !important' },
                });

                rendition.on('relocated', (location: any) => {
                    if (book.navigation) {
                        const chapter = book.navigation.toc.find(
                            (t: any) => t.href && location.start?.href?.includes(t.href.split('#')[0])
                        );
                        if (chapter) setCurrentChapter(chapter.label?.trim() || '');
                    }
                });

                await rendition.display();
                if (!cancelled) setLoading(false);
            } catch (err) {
                if (!cancelled) {
                    setError(t('filePreviewEpubError'));
                    setLoading(false);
                }
            }
        }

        renderEpub();

        return () => {
            cancelled = true;
            if (renditionRef.current) {
                try { renditionRef.current.destroy(); } catch (_) { /* */ }
            }
            if (book) {
                try { book.destroy(); } catch (_) { /* */ }
            }
        };
    }, [url, fileId]);

    const goNext = () => renditionRef.current?.next();
    const goPrev = () => renditionRef.current?.prev();

    // Keyboard navigation
    useEffect(() => {
        const handler = (e: KeyboardEvent) => {
            if (e.key === 'ArrowLeft') goPrev();
            if (e.key === 'ArrowRight') goNext();
        };
        window.addEventListener('keydown', handler);
        return () => window.removeEventListener('keydown', handler);
    }, []);

    if (error) {
        return (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: 300, gap: 12 }}>
                <ReadOutlined style={{ fontSize: 48, color: '#94A3B8' }} />
                <Text style={{ color: '#64748B' }}>{error}</Text>
                <Button type="primary" icon={<DownloadOutlined />} onClick={() => window.open(url, '_blank')}>
                    {t('downloadFile')}
                </Button>
            </div>
        );
    }

    return (
        <div style={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column', position: 'relative' }}>
            {loading && (
                <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 12, zIndex: 10, background: '#fff' }}>
                    <Spin size="large" />
                    <Text style={{ color: '#64748B' }}>{t('loadingBook')}</Text>
                </div>
            )}

            {/* Chapter indicator */}
            {currentChapter && !loading && (
                <div style={{
                    padding: '8px 20px', background: '#F8FAFC', borderBottom: '1px solid #E2E8F0',
                    fontSize: 13, color: '#64748B', fontWeight: 500, flexShrink: 0,
                }}>
                    {currentChapter}
                </div>
            )}

            {/* EPUB viewer area */}
            <div ref={viewerRef} style={{ flex: 1, overflow: 'hidden' }} />

            {/* Navigation buttons */}
            {!loading && (
                <div style={{
                    display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 24,
                    padding: '12px 0', borderTop: '1px solid #E2E8F0', background: '#F8FAFC', flexShrink: 0,
                }}>
                    <Button
                        icon={<LeftOutlined />}
                        onClick={goPrev}
                        style={{ borderRadius: 8 }}
                    >
                        {t('previous')}
                    </Button>
                    <Text style={{ color: '#94A3B8', fontSize: 12 }}>{t('arrowKeysToNavigate')}</Text>
                    <Button
                        icon={<RightOutlined />}
                        onClick={goNext}
                        style={{ borderRadius: 8 }}
                    >
                        {t('next')}
                    </Button>
                </div>
            )}
        </div>
    );
}

/* ─── Main File Preview Card ─── */
export const FilePreviewCard = ({ file, onDelete, canDelete = false }: FilePreviewCardProps) => {
    const { t } = useTranslate('common');
    const [previewOpen, setPreviewOpen] = useState(false);
    const isImage = isImageFile(file.fileName);
    const canPreview = isPreviewable(file.fileName);
    const fileId = file.fileId ?? file.id;
    const fileTypeLabel = (() => {
        const ext = getFileExtension(file.fileName);
        if (WORD_EXTENSIONS.includes(ext)) return t('wordDocument');
        if (EXCEL_EXTENSIONS.includes(ext)) return t('excelSpreadsheet');
        if (CSV_EXTENSIONS.includes(ext)) return t('csvFile');
        if (PDF_EXTENSIONS.includes(ext)) return t('pdfDocument');
        if (EPUB_EXTENSIONS.includes(ext)) return t('epubEbook');
        return t('document');
    })();

    const handlePreviewClick = useCallback(() => {
        if (isImage) {
            setPreviewOpen(true);
        } else if (canPreview) {
            setPreviewOpen(true);
        } else {
            window.open(file.url, '_blank');
        }
    }, [isImage, canPreview, file.url]);

    const renderDocumentPreview = () => {
        if (isPdfFile(file.fileName)) return <PdfPreview url={file.url} />;
        if (isWordFile(file.fileName)) return <WordPreview url={file.url} fileName={file.fileName} fileId={fileId} />;
        if (isExcelFile(file.fileName) || isCsvFile(file.fileName)) return <ExcelPreview url={file.url} fileName={file.fileName} fileId={fileId} />;
        if (isEpubFile(file.fileName)) return <EpubPreview url={file.url} fileName={file.fileName} fileId={fileId} />;
        return null;
    };

    return (
        <>
            <div
                style={{
                    width: 140,
                    border: '1px solid #e5e7eb',
                    borderRadius: 10,
                    overflow: 'hidden',
                    background: '#fff',
                    transition: 'box-shadow 0.2s, border-color 0.2s',
                    cursor: 'pointer',
                    position: 'relative',
                }}
                onMouseEnter={(e) => {
                    e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.08)';
                    e.currentTarget.style.borderColor = '#93c5fd';
                }}
                onMouseLeave={(e) => {
                    e.currentTarget.style.boxShadow = 'none';
                    e.currentTarget.style.borderColor = '#e5e7eb';
                }}
            >
                {/* Preview area */}
                <div
                    style={{
                        height: 100,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: isImage ? '#f8fafc' : '#f9fafb',
                        position: 'relative',
                        overflow: 'hidden',
                    }}
                    onClick={handlePreviewClick}
                >
                    {isImage ? (
                        <img
                            src={file.url}
                            alt={file.fileName}
                            style={{
                                width: '100%',
                                height: '100%',
                                objectFit: 'cover',
                            }}
                            onError={(e) => {
                                e.currentTarget.style.display = 'none';
                                const parent = e.currentTarget.parentElement;
                                if (parent) {
                                    const icon = document.createElement('div');
                                    icon.innerHTML = '🖼️';
                                    icon.style.fontSize = '32px';
                                    parent.appendChild(icon);
                                }
                            }}
                        />
                    ) : (
                        getFileIcon(file.fileName)
                    )}

                    {/* Hover overlay */}
                    <div
                        style={{
                            position: 'absolute',
                            inset: 0,
                            background: 'rgba(0,0,0,0.4)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            gap: 8,
                            opacity: 0,
                            transition: 'opacity 0.2s',
                        }}
                        onMouseEnter={(e) => { e.currentTarget.style.opacity = '1'; }}
                        onMouseLeave={(e) => { e.currentTarget.style.opacity = '0'; }}
                    >
                        {canPreview && (
                            <Tooltip title={t('preview')}>
                                <Button
                                    type="primary"
                                    shape="circle"
                                    size="small"
                                    icon={<EyeOutlined />}
                                    onClick={(e) => { e.stopPropagation(); handlePreviewClick(); }}
                                />
                            </Tooltip>
                        )}
                        <Tooltip title={t('download')}>
                            <Button
                                shape="circle"
                                size="small"
                                icon={<DownloadOutlined />}
                                onClick={(e) => { e.stopPropagation(); window.open(file.url, '_blank'); }}
                                style={{ background: 'white' }}
                            />
                        </Tooltip>
                    </div>
                </div>

                {/* File name + delete */}
                <div
                    style={{
                        padding: '6px 8px',
                        borderTop: '1px solid #f3f4f6',
                        display: 'flex',
                        alignItems: 'center',
                        gap: 4,
                    }}
                >
                    <Tooltip title={file.fileName}>
                        <span
                            style={{
                                fontSize: 11,
                                color: '#374151',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                                flex: 1,
                            }}
                        >
                            {file.fileName}
                        </span>
                    </Tooltip>
                    {canDelete && onDelete && (
                        <Button
                            type="text"
                            size="small"
                            icon={<CloseOutlined style={{ fontSize: 10 }} />}
                            onClick={(e) => { e.stopPropagation(); onDelete(); }}
                            style={{ color: '#9ca3af', minWidth: 20, height: 20, padding: 0 }}
                        />
                    )}
                </div>
            </div>

            {/* Image preview modal */}
            {isImage && (
                <Image
                    src={file.url}
                    alt={file.fileName}
                    style={{ display: 'none' }}
                    preview={{
                        visible: previewOpen,
                        onVisibleChange: setPreviewOpen,
                        src: file.url,
                    }}
                />
            )}

            {/* Document preview modal (PDF, Word, Excel) */}
            {!isImage && canPreview && (
                <Modal
                    open={previewOpen}
                    onCancel={() => setPreviewOpen(false)}
                    footer={null}
                    width="85vw"
                    centered
                    destroyOnHidden
                    title={
                        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                            {getFileIcon(file.fileName)}
                            <div>
                                <div style={{ fontSize: 15, fontWeight: 600, color: '#0F172A' }}>{file.fileName}</div>
                                <div style={{ fontSize: 12, color: '#94A3B8', fontWeight: 400 }}>{fileTypeLabel}</div>
                            </div>
                        </div>
                    }
                    styles={{
                        body: { height: '75vh', padding: 0, overflow: 'hidden' },
                        content: { borderRadius: 16, overflow: 'hidden' },
                    }}
                >
                    <div className="excel-preview-content" style={{ height: '100%', overflow: 'auto' }}>
                        {renderDocumentPreview()}
                    </div>
                </Modal>
            )}
        </>
    );
};
