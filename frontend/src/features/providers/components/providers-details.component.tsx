'use client';
import 'reflect-metadata';

import { useState, useMemo, useCallback } from 'react';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { Avatar, Button, Card, Divider, Typography, App, Space, Tag, Row, Col, Tooltip } from 'antd';
import {
  PhoneOutlined,
  MailOutlined,
  GlobalOutlined,
  EnvironmentOutlined,
  FileTextOutlined,
  EditOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import LoadingSpinnerComponent from '@/components/loading/loading-spinner.component';
import { AppButton } from '@/components/button/app-button.component';
import { Icon } from '@/components/iconify/iconify.component';
import { FilePreviewCard } from '@/components/file-preview/file-preview-card.component';
import ProvidersFormModalComponent from '@/features/providers/components/providers-form-modal.component';
import ProvidersAddFileModalComponent from '@/features/providers/components/providers-add-file-modal.component';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';
import { useTranslate } from '@/locales/use-locales';
import { useParams } from 'next/navigation';
import { container } from 'tsyringe';
import { ProvidersService } from '@/features/providers/services/providers.service';
import { usePermissionContext, PERMISSIONS } from '@/features/permissions';

const { Title, Text } = Typography;

interface FileModalState {
  isOpen: boolean;
  mode: 'add' | 'editTitle' | 'editFiles';
  id?: number | string;
  title?: string;
  files?: any[];
}

/* ─── Detail row helper ─── */
function InfoRow({ icon, label, value }: { icon: React.ReactNode; label: string; value?: string | null }) {
  return (
    <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '12px 0' }}>
      <span style={{ color: '#64748B', fontSize: 16, marginTop: 2 }}>{icon}</span>
      <div style={{ minWidth: 120 }}>
        <Text style={{ color: '#94A3B8', fontSize: 13, display: 'block' }}>{label}</Text>
        <Text style={{ fontSize: 14, color: value ? '#0F172A' : '#CBD5E1', fontWeight: 500 }}>
          {value || '—'}
        </Text>
      </div>
    </div>
  );
}

export default function ProvidersDetailsComponent() {
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [fileModal, setFileModal] = useState<FileModalState>({ isOpen: false, mode: 'add' });
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteFileInfo, setDeleteFileInfo] = useState<{ fileId: number; fileName: string; sectionName: string }>({
    fileId: 0,
    fileName: '',
    sectionName: '',
  });
  const [isDeleteGroupOpen, setIsDeleteGroupOpen] = useState(false);
  const [deleteGroupInfo, setDeleteGroupInfo] = useState<{ groupId: number; groupName: string }>({
    groupId: 0,
    groupName: '',
  });

  const queryClient = useQueryClient();
  const { t } = useTranslate('providers');
  const params = useParams();
  const id = Number(params.id);
  const service = useMemo(() => container.resolve(ProvidersService), []);
  const { notification } = App.useApp();
  const { hasPermission } = usePermissionContext();

  // Permission check for file group deletion
  const canDeleteFileGroup = hasPermission(PERMISSIONS.FILE_GROUP_DELETE);

  const {
    data: provider,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['provider-details', id],
    queryFn: () => service.getDetails(id),
  });

  const { data: fileGroups = [] } = useQuery({
    queryKey: ['provider-file-groups', id],
    queryFn: () => service.getFileGroups(id),
  });

  const deleteFileGroupMutation = useMutation({
    mutationFn: (fileGroupId: number) => service.deleteFileGroup(id, fileGroupId),
    onSuccess: async () => {
      notification.success({ message: t('fileGroupDeletedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['provider-file-groups', id] });
      setIsDeleteGroupOpen(false);
    },
  });

  const handleDeleteGroupClick = (groupId: number, groupName: string) => {
    setDeleteGroupInfo({ groupId, groupName });
    setIsDeleteGroupOpen(true);
  };

  const handleDeleteGroup = () => {
    deleteFileGroupMutation.mutate(deleteGroupInfo.groupId);
  };

  const openAddFileModal = useCallback(() => {
    setFileModal({ isOpen: true, mode: 'add' });
  }, []);

  const openEditTitleModal = useCallback((title: string, groupId: number | string) => {
    setFileModal({ isOpen: true, mode: 'editTitle', id: groupId, title });
  }, []);

  const openEditFilesModal = useCallback((title: string, files: any[], groupId: string | number) => {
    const mappedFiles = files.map((file: any) => ({
      uid: String(file.id ?? file.url),
      id: file.id,
      fileId: file.fileId,
      name: file.fileName,
      status: 'done' as const,
      url: file.url,
    }));
    setFileModal({ isOpen: true, mode: 'editFiles', id: groupId, title, files: mappedFiles });
  }, []);

  const closeFileModal = useCallback(() => {
    setFileModal({ isOpen: false, mode: 'add' });
  }, []);

  const handleDeleteClick = (file: any, sectionName: string) => {
    setDeleteFileInfo({ fileId: file.id, fileName: file.fileName, sectionName });
    setIsDeleteOpen(true);
  };

  const handleDeleteFile = async () => {
    setIsDeleting(true);
    try {
      await service.deleteFileFromGroup(id, deleteFileInfo.fileId);
      notification.success({ message: t('fileDeletedSuccessfully') });
      await queryClient.invalidateQueries({ queryKey: ['provider-file-groups', id] });
      setIsDeleteOpen(false);
    } catch (error) {
      console.error(error);
    } finally {
      setIsDeleting(false);
    }
  };

  const fileModalInitialValues = useMemo(() => {
    if (fileModal.mode === 'editTitle') {
      return { title: fileModal.title, id: fileModal.id };
    }
    if (fileModal.mode === 'editFiles') {
      return { files: fileModal.files, id: fileModal.id, title: fileModal.title };
    }
    return undefined;
  }, [fileModal.mode, fileModal.title, fileModal.id, fileModal.files]);

  if (isLoading) return <LoadingSpinnerComponent />;

  if (isError) return <Typography.Text type='danger'>{t('errorLoadingProviderData')}</Typography.Text>;

  if (!provider) return null;

  /* ─── Initials for Avatar fallback ─── */
  const initials = (provider.name || '')
    .split(' ')
    .map((n: string) => n[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
        {/* ── Profile Card ── */}
        <Card
          styles={{ body: { padding: 0 } }}
          style={{ borderRadius: 16, overflow: 'hidden', border: '1px solid #E2E8F0' }}
        >
          {/* Header banner */}
          <div
            style={{
              background: 'linear-gradient(135deg, #1B3A5C 0%, #2D5A8E 100%)',
              padding: '32px 32px 24px',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
              <Avatar
                src={provider?.profileUrl || undefined}
                size={72}
                style={{
                  backgroundColor: '#10B981',
                  fontSize: 24,
                  fontWeight: 700,
                  border: '3px solid rgba(255,255,255,0.3)',
                }}
              >
                {initials}
              </Avatar>
              <div>
                <Title level={4} style={{ color: '#fff', margin: 0 }}>
                  {provider.name}
                </Title>
                <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 13 }}>
                  {t('provider')}
                </Text>
              </div>
            </div>
            <Button
              icon={<EditOutlined />}
              onClick={() => setIsEditOpen(true)}
              style={{
                background: 'rgba(255,255,255,0.15)',
                borderColor: 'rgba(255,255,255,0.3)',
                color: '#fff',
                borderRadius: 10,
                height: 40,
                paddingInline: 20,
                fontWeight: 500,
              }}
            >
              {t('editProvider')}
            </Button>
          </div>

          {/* Info fields */}
          <div style={{ padding: '24px 32px 28px' }}>
            <Row gutter={[48, 0]}>
              <Col xs={24} md={12}>
                <InfoRow icon={<PhoneOutlined />} label={t('phone')} value={provider.phone} />
                <InfoRow icon={<MailOutlined />} label={t('email')} value={provider.email} />
                <InfoRow icon={<GlobalOutlined />} label={t('website')} value={provider.website} />
              </Col>
              <Col xs={24} md={12}>
                <InfoRow icon={<PhoneOutlined />} label={t('secondPhone')} value={provider.secondPhone} />
                <InfoRow icon={<FileTextOutlined />} label={t('description')} value={provider.description} />
                <InfoRow icon={<EnvironmentOutlined />} label={t('address')} value={provider.address} />
              </Col>
            </Row>
          </div>
        </Card>

        {/* ── File Groups Card ── */}
        <Card
          style={{ borderRadius: 16, border: '1px solid #E2E8F0' }}
          title={
            <Text strong style={{ fontSize: 16, color: '#0F172A' }}>
              {t('fileGroups')}
            </Text>
          }
          extra={
            <Button
              icon={<PlusOutlined />}
              onClick={openAddFileModal}
              style={{
                borderColor: '#CBD5E1',
                borderRadius: 10,
                height: 40,
                paddingInline: 20,
                fontWeight: 500,
                color: '#334155',
              }}
            >
              {t('addFileSet')}
            </Button>
          }
          styles={{
            header: { padding: '16px 24px', borderBottom: '1px solid #F1F5F9' },
            body: { padding: 24 },
          }}
        >
          {fileGroups?.length === 0 && (
            <div style={{ textAlign: 'center', padding: '32px 0', color: '#94A3B8' }}>
              <FileTextOutlined style={{ fontSize: 32, marginBottom: 8, display: 'block' }} />
              <Text style={{ color: '#94A3B8' }}>{t('noFileGroupsYet')}</Text>
            </div>
          )}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {fileGroups?.map((group) => (
              <div
                key={group.label}
                style={{
                  border: '1px solid #E2E8F0',
                  borderRadius: 12,
                  padding: 16,
                  background: '#FAFBFC',
                }}
              >
                {/* Header: Title + Actions */}
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <Tag color="green" style={{ borderRadius: 6, margin: 0 }}>{t('title')}</Tag>
                    <Text strong style={{ fontSize: 14, color: '#1E293B' }}>{group.label}</Text>
                  </div>
                  <Space size={4}>
                    <Tooltip title={t('editFiles')}>
                      <Button type='text' size='small' aria-label={t('editFiles')} icon={<Icon icon='upload' />} style={{ color: '#64748B' }} onClick={() => openEditFilesModal(group.label, group.groupItems, group.id)} />
                    </Tooltip>
                    <Tooltip title={t('editTitle')}>
                      <Button type='text' size='small' aria-label={t('editTitle')} icon={<Icon icon='edit' />} style={{ color: '#64748B' }} onClick={() => openEditTitleModal(group.label, group.id)} />
                    </Tooltip>
                    {canDeleteFileGroup && (
                      <Tooltip title={t('deleteFileGroup')}>
                        <Button type='text' size='small' aria-label={t('deleteFileGroup')} icon={<Icon icon='trash' />} style={{ color: '#EF4444' }} onClick={() => handleDeleteGroupClick(group.id, group.label)} />
                      </Tooltip>
                    )}
                  </Space>
                </div>

                {/* File preview cards */}
                {group?.groupItems?.length > 0 && (
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
                    {group.groupItems.map((file) => (
                      <FilePreviewCard
                        key={file.url}
                        file={file}
                        canDelete={canDeleteFileGroup}
                        onDelete={() => handleDeleteClick(file, group.label)}
                      />
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </Card>
      </div>

      <ConfirmDeleteModalComponent
        open={isDeleteOpen}
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={handleDeleteFile}
        title={t('deleteFile')}
        loading={isDeleting}
        description={
          <>
            {t('areYouSureDeleteFile')} <strong>&quot;{deleteFileInfo.fileName}&quot;</strong> {t('fromSection')} <strong>&quot;{deleteFileInfo.sectionName}&quot;</strong>?
          </>
        }
      />

      <ConfirmDeleteModalComponent
        open={isDeleteGroupOpen}
        onCancel={() => setIsDeleteGroupOpen(false)}
        onConfirm={handleDeleteGroup}
        title={t('deleteFileGroup')}
        loading={deleteFileGroupMutation.isPending}
        description={
          <>
            {t('areYouSureDeleteFileGroup')} <strong>&quot;{deleteGroupInfo.groupName}&quot;</strong>?
          </>
        }
      />

      {isEditOpen && <ProvidersFormModalComponent open={isEditOpen} onClose={() => setIsEditOpen(false)} initialValues={provider} />}

      {fileModal.isOpen && <ProvidersAddFileModalComponent mode={fileModal.mode} open={fileModal.isOpen} onClose={closeFileModal} initialValues={fileModalInitialValues} />}
    </>
  );
}
