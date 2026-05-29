'use client';

import { useEffect, useMemo, useState } from 'react';
import { Input, Upload, Row, Col, Avatar, App } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, FormProvider } from 'react-hook-form';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';
import AppModal from '@/components/modal/app-modal.component';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ClientsService } from '@/features/clients/services/clients.service';
import { container } from '@/services/di-container';

const createSchema = (t: (key: string, options?: Record<string, unknown>) => string) => z.object({
  name: z.string().min(1, t('fieldIsRequired', { field: t('name') })),
  email: z.string().email(t('invalidEmail')).optional().or(z.literal('')),
  website: z.string().optional(),
  phone: z.string().optional(),
  secondPhone: z.string().optional(),
  address: z.string().optional(),
  description: z.string().max(100, t('maxCharacters', { count: 100 })).optional(),
  avatarFile: z.any().optional(),
});

export type EditClientForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  initialValues?: any;
  isCreateForm?: boolean;
}

export default function ClientsFormModalComponent({ open, onClose, initialValues, isCreateForm = false }: Props) {
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [fileList, setFileList] = useState<any[]>([]);

  const { t } = useTranslate('clients');
  const schema = useMemo(() => createSchema(t), [t]);
  const queryClient = useQueryClient();
  const service = container.resolve(ClientsService);
  const { notification } = App.useApp();

  const methods = useForm<EditClientForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: '',
      email: '',
      website: '',
      phone: '',
      secondPhone: '',
      address: '',
      description: '',
      avatarFile: undefined,
    },
  });

  const { data: clientDetails, isLoading: isDetailsLoading } = useQuery({
    queryKey: ['client-edit-details', initialValues?.id],
    queryFn: () => service.getDetails(initialValues!.id!),
    enabled: !!initialValues?.id && !isCreateForm && open,
  });

  const createMutation = useMutation({
    mutationFn: async (data: EditClientForm) => {
      await service.create(data);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['clients-list'] });
      notification.success({ message: t('clientCreatedSuccessfully') });
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (data: EditClientForm) => {
      if (!initialValues?.id) throw new Error('Missing client ID for update');
      await service.update(initialValues.id, data);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['clients-list'] });
      await queryClient.invalidateQueries({ queryKey: ['client-details'] });
      notification.success({ message: t('clientUpdatedSuccessfully') });
    },
  });

  const { reset, setValue, handleSubmit } = methods;

  useEffect(() => {
    if (open && !isCreateForm && clientDetails) {
      reset({
        name: clientDetails?.name ?? '',
        email: clientDetails?.email ?? '',
        website: clientDetails?.website ?? '',
        phone: clientDetails?.phone ?? '',
        secondPhone: clientDetails?.secondPhone ?? '',
        address: clientDetails?.address ?? '',
        description: clientDetails?.description ?? '',
        avatarFile: clientDetails?.profileUrl,
      });
      setAvatarFile(null);
    }
  }, [open, clientDetails, isCreateForm, reset]);

  const handleAvatarChange = (file: File) => {
    setAvatarFile(file);
    setValue('avatarFile', file);
    return false;
  };

  const handleUploadChange = ({ fileList }: any) => setFileList(fileList);

  const submitForm = async (values: EditClientForm) => {
    const payload = { ...values, file: avatarFile ?? undefined };

    if (isCreateForm) {
      await createMutation.mutateAsync(payload);
    } else {
      await updateMutation.mutateAsync(payload);
    }

    onClose();
  };

  return (
    <AppModal
      loading={updateMutation.isPending || createMutation.isPending || (!isCreateForm && isDetailsLoading)}
      open={open}
      onClose={onClose}
      title={isCreateForm ? t('addNewClient') : t('editClientDetails')}
      onSubmit={handleSubmit(submitForm)}
      submitText={t('save')}
      width={600}>
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(submitForm)} className='space-y-4'>
          {isCreateForm ? (
            <div className='flex items-center gap-3 mb-6'>
              <Upload beforeUpload={handleAvatarChange} showUploadList={false} accept='.jpg,.jpeg,.png,.pdf'>
                {avatarFile ? (
                  <Avatar size={80} src={URL.createObjectURL(avatarFile)} className='cursor-pointer border border-gray-300' />
                ) : (
                  <div className='rounded-full border-dashed border-2 border-gray-300 w-20 h-20 flex flex-col justify-center items-center cursor-pointer hover:bg-gray-50 transition'>
                    <Icon icon='plus' className='text-2xl text-gray-500' />
                    <span className='text-xs text-gray-400 mt-1'>{t('upload')}</span>
                  </div>
                )}
              </Upload>

              <p className='text-xs text-gray-400 mt-2'>{t('recommendedResolutionIs640×640WithFileSizeLessThan2MB,KeepVisualElementsCentered')}</p>
            </div>
          ) : (
            <div className='flex items-center mb-4'>
              <div className='relative'>
                <Avatar size={64} src={avatarFile ? URL.createObjectURL(avatarFile) : (clientDetails?.profileUrl || undefined)} />
                <Upload beforeUpload={handleAvatarChange} showUploadList={false} accept='.jpg,.jpeg,.png,.pdf'>
                  <div className='absolute -right-1.5 -bottom-1.5 bg-[#3B82F6] flex items-center justify-center !text-white rounded-full w-8 h-8 cursor-pointer hover:bg-[#2563EB] transition-colors'>
                    <EditOutlined style={{ color: 'white', fontSize: 14 }} />
                  </div>
                </Upload>
              </div>
            </div>
          )}

          <AppTextField name='name' label={t('name')} placeholder={t('enterYourName')} />

          <Row gutter={16}>
            <Col span={12}>
              <AppTextField name='email' label={t('email')} placeholder={t('enterYourEmail')} />
            </Col>
            <Col span={12}>
              <AppTextField name='website' label={t('website')} placeholder={t('enterYourWebsite')} />
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <AppTextField name='phone' label={t('Phone')} placeholder={t('enterYourNumber')} />
            </Col>
            <Col span={12}>
              <AppTextField name='secondPhone' label={t('secondPhone')} placeholder={t('enterYourSecondNumber')} />
            </Col>
          </Row>

          <AppTextField name='address' label={t('address')} placeholder={t('primaryAddress')} />
          <AppTextField name='description' type='textarea' label={t('description')} />

          {/*{isCreateForm && (*/}
          {/*    <div>*/}
          {/*        <AppUploadDraggerComponent fileList={fileList} onChange={handleUploadChange} />*/}
          {/*    </div>*/}
          {/*)}*/}
        </form>
      </FormProvider>
    </AppModal>
  );
}
