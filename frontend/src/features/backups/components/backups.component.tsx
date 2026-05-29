'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { Table, Button, Tag, Space, Modal, Upload, message, Tooltip, Typography, Card, Empty } from 'antd';
import {
    CloudUploadOutlined,
    DownloadOutlined,
    DeleteOutlined,
    ReloadOutlined,
    UndoOutlined,
    ExclamationCircleFilled,
    PlusOutlined,
    UploadOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined,
    SyncOutlined,
    ClockCircleOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { BackupsRepository } from '@/features/backups/data/backups.api';
import { BackupJob, BackupJobStatus, BackupJobType } from '@/api/backups/types';
import { useTranslate } from '@/locales/use-locales';

const { Title, Text } = Typography;
const repo = new BackupsRepository();

export const BackupsComponent = () => {
    const { t, currentLang } = useTranslate('backups');
    const [jobs, setJobs] = useState<BackupJob[]>([]);
    const [loading, setLoading] = useState(false);
    const [creating, setCreating] = useState(false);
    const pollingRef = useRef<ReturnType<typeof setInterval> | null>(null);

    const fetchJobs = useCallback(async () => {
        try {
            setLoading(true);
            const data = await repo.fetchAll();
            setJobs(data);
        } catch {
            // silent
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchJobs();
    }, [fetchJobs]);

    // Poll when there are pending/in-progress jobs
    useEffect(() => {
        const hasActive = jobs.some(
            (j) => j.status === BackupJobStatus.Pending || j.status === BackupJobStatus.InProgress
        );

        if (hasActive && !pollingRef.current) {
            pollingRef.current = setInterval(fetchJobs, 4000);
        } else if (!hasActive && pollingRef.current) {
            clearInterval(pollingRef.current);
            pollingRef.current = null;
        }

        return () => {
            if (pollingRef.current) {
                clearInterval(pollingRef.current);
                pollingRef.current = null;
            }
        };
    }, [jobs, fetchJobs]);

    const handleCreate = async () => {
        try {
            setCreating(true);
            await repo.createBackup();
            message.success(t('backupJobCreated'));
            fetchJobs();
        } catch (err: any) {
            const msg = err?.response?.data?.message || t('failedToCreateBackup');
            message.error(msg);
        } finally {
            setCreating(false);
        }
    };

    const handleDownload = async (job: BackupJob) => {
        try {
            const { url, fileName } = await repo.getDownloadUrl(job.id);
            // Fetch the file as a blob and trigger a proper download with the correct filename
            const response = await fetch(url);
            const blob = await response.blob();
            const blobUrl = window.URL.createObjectURL(blob);
            const anchor = document.createElement('a');
            anchor.href = blobUrl;
            anchor.download = fileName || `backup_${job.id}.zip`;
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            window.URL.revokeObjectURL(blobUrl);
        } catch {
            message.error(t('failedToDownloadBackup'));
        }
    };

    const handleRestore = (job: BackupJob) => {
        Modal.confirm({
            title: t('restoreFromBackup'),
            icon: <ExclamationCircleFilled style={{ color: '#f59e0b' }} />,
            content: (
                <div>
                    <p style={{ margin: '8px 0', fontWeight: 600 }}>
                        {t('restoreWarningTitle')}
                    </p>
                    <p style={{ margin: '4px 0', color: '#6b7280', fontSize: 13 }}>
                        {t('restoreWarningDescription')} <strong>{job.fileName}</strong>
                    </p>
                    <p style={{ margin: '8px 0 0', color: '#ef4444', fontSize: 13 }}>
                        {t('restoreWarningCannotUndo')}
                    </p>
                </div>
            ),
            okText: t('yesRestore'),
            okButtonProps: { danger: true },
            cancelText: t('cancel'),
            async onOk() {
                try {
                    await repo.restoreFromBackup(job.id);
                    message.success(t('restoreJobCreated'));
                    fetchJobs();
                } catch (err: any) {
                    const msg = err?.response?.data?.message || t('failedToStartRestore');
                    message.error(msg);
                }
            },
        });
    };

    const handleUploadRestore = (file: File) => {
        Modal.confirm({
            title: t('uploadAndRestore'),
            icon: <ExclamationCircleFilled style={{ color: '#f59e0b' }} />,
            content: (
                <div>
                    <p style={{ margin: '8px 0', fontWeight: 600 }}>
                        {t('uploadRestoreWarningTitle')}
                    </p>
                    <p style={{ margin: '4px 0', color: '#6b7280', fontSize: 13 }}>
                        {t('file')}: <strong>{file.name}</strong> ({(file.size / 1024 / 1024).toFixed(2)} MB)
                    </p>
                    <p style={{ margin: '8px 0 0', color: '#ef4444', fontSize: 13 }}>
                        {t('autoBackupBeforeRestore')}
                    </p>
                </div>
            ),
            okText: t('uploadAndRestore'),
            okButtonProps: { danger: true },
            cancelText: t('cancel'),
            async onOk() {
                try {
                    await repo.uploadAndRestore(file);
                    message.success(t('fileUploadedRestoreWillBegin'));
                    fetchJobs();
                } catch (err: any) {
                    const msg = err?.response?.data?.message || t('failedToUploadAndRestore');
                    message.error(msg);
                }
            },
        });
        return false; // prevent default upload
    };

    const handleDelete = (job: BackupJob) => {
        Modal.confirm({
            title: t('deleteBackup'),
            icon: <ExclamationCircleFilled style={{ color: '#ef4444' }} />,
            content: t('areYouSureDeleteBackup', { fileName: job.fileName }),
            okText: t('delete'),
            okButtonProps: { danger: true },
            async onOk() {
                try {
                    await repo.deleteBackup(job.id);
                    message.success(t('backupDeleted'));
                    fetchJobs();
                } catch {
                    message.error(t('failedToDeleteBackup'));
                }
            },
        });
    };

    const statusTag = (status: BackupJobStatus) => {
        switch (status) {
            case BackupJobStatus.Pending:
                return <Tag icon={<ClockCircleOutlined spin />} color="default">{t('pending')}</Tag>;
            case BackupJobStatus.InProgress:
                return <Tag icon={<SyncOutlined spin />} color="processing">{t('inProgress')}</Tag>;
            case BackupJobStatus.Completed:
                return <Tag icon={<CheckCircleOutlined />} color="success">{t('completed')}</Tag>;
            case BackupJobStatus.Failed:
                return <Tag icon={<CloseCircleOutlined />} color="error">{t('failed')}</Tag>;
            default:
                return <Tag>{t('unknown')}</Tag>;
        }
    };

    const typeTag = (type: BackupJobType) => {
        switch (type) {
            case BackupJobType.Export:
                return <Tag color="blue">{t('export')}</Tag>;
            case BackupJobType.Import:
                return <Tag color="orange">{t('import')}</Tag>;
            default:
                return <Tag>{t('unknown')}</Tag>;
        }
    };

    const formatSize = (bytes: number | null) => {
        if (!bytes) return '—';
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
    };

    const formatDate = (iso: string | null) => {
        if (!iso) return '—';
        return new Date(iso).toLocaleString(currentLang.value === 'zh' ? 'zh-CN' : 'en-US');
    };

    const hasActiveJob = jobs.some(
        (j) => j.status === BackupJobStatus.Pending || j.status === BackupJobStatus.InProgress
    );

    const columns: ColumnsType<BackupJob> = [
        {
            title: t('status'),
            dataIndex: 'status',
            key: 'status',
            width: 130,
            render: (_, r) => statusTag(r.status),
        },
        {
            title: t('type'),
            dataIndex: 'type',
            key: 'type',
            width: 100,
            render: (_, r) => typeTag(r.type),
        },
        {
            title: t('file'),
            dataIndex: 'fileName',
            key: 'fileName',
            ellipsis: true,
            render: (v: string | null) => v || <Text type="secondary">—</Text>,
        },
        {
            title: t('size'),
            dataIndex: 'fileSizeBytes',
            key: 'fileSizeBytes',
            width: 100,
            render: (v: number | null) => formatSize(v),
        },
        {
            title: t('created'),
            dataIndex: 'createdAt',
            key: 'createdAt',
            width: 180,
            render: (v: string) => formatDate(v),
        },
        {
            title: t('completed'),
            dataIndex: 'completedAt',
            key: 'completedAt',
            width: 180,
            render: (v: string | null) => formatDate(v),
        },
        {
            title: t('actions'),
            key: 'actions',
            width: 150,
            render: (_, r) => (
                <Space size="small">
                    {r.status === BackupJobStatus.Completed && r.type === BackupJobType.Export && (
                        <>
                            <Tooltip title={t('download')}>
                                <Button
                                    size="small"
                                    type="text"
                                    icon={<DownloadOutlined />}
                                    onClick={() => handleDownload(r)}
                                />
                            </Tooltip>
                            <Tooltip title={t('restoreFromThisBackup')}>
                                <Button
                                    size="small"
                                    type="text"
                                    icon={<UndoOutlined />}
                                    disabled={hasActiveJob}
                                    onClick={() => handleRestore(r)}
                                />
                            </Tooltip>
                        </>
                    )}
                    {r.status === BackupJobStatus.Failed && r.errorMessage && (
                        <Tooltip title={r.errorMessage}>
                            <Button size="small" type="text" danger icon={<ExclamationCircleFilled />} />
                        </Tooltip>
                    )}
                    {r.status !== BackupJobStatus.InProgress && (
                        <Tooltip title={t('delete')}>
                            <Button
                                size="small"
                                type="text"
                                danger
                                icon={<DeleteOutlined />}
                                onClick={() => handleDelete(r)}
                            />
                        </Tooltip>
                    )}
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                <div>
                    <Title level={4} style={{ margin: 0 }}>{t('backups')}</Title>
                    <Text type="secondary" style={{ fontSize: 13 }}>
                        {t('backupsPageSubtitle')}
                    </Text>
                </div>
                <Space>
                    <Upload
                        accept=".zip"
                        showUploadList={false}
                        beforeUpload={(file) => {
                            handleUploadRestore(file as File);
                            return false;
                        }}
                        disabled={hasActiveJob}
                    >
                        <Button icon={<UploadOutlined />} disabled={hasActiveJob}>
                            {t('importFromFile')}
                        </Button>
                    </Upload>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={handleCreate}
                        loading={creating}
                        disabled={hasActiveJob}
                    >
                        {t('createBackup')}
                    </Button>
                    <Button icon={<ReloadOutlined />} onClick={fetchJobs} loading={loading} />
                </Space>
            </div>

            <Card styles={{ body: { padding: 0 } }}>
                <Table
                    columns={columns}
                    dataSource={jobs}
                    rowKey="id"
                    loading={loading}
                    pagination={false}
                    size="middle"
                    locale={{
                        emptyText: (
                            <Empty
                                image={Empty.PRESENTED_IMAGE_SIMPLE}
                                description={t('noBackupsYet')}
                            />
                        ),
                    }}
                />
            </Card>
        </div>
    );
};
