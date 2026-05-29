'use client';

import { useEffect, useMemo, useState } from 'react';
import { Input, Upload, Row, Col, Avatar, App, Select, Radio } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, FormProvider, Controller } from 'react-hook-form';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';
import AppModal from '@/components/modal/app-modal.component';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { EmployeesService } from '@/features/employees/services/employees.service';
import { container } from '@/services/di-container';
import type { EmployeeServiceDTO } from '@/features/employees/dto/employees.service.dto';
import { defaultSelectFilter } from '@/utils/select-utils';

const createSchema = (t: (key: string, options?: Record<string, unknown>) => string) => z.object({
  name: z.string().min(1, t('fieldIsRequired', { field: t('name') })),
  role: z.enum(['manager', 'employee'], {
    message: t('fieldIsRequired', { field: t('role') }),
  }),
  email: z.string().email(t('invalidEmail')),
  phone: z.string().min(1, t('fieldIsRequired', { field: t('phone') })),
  address: z.string().min(1, t('fieldIsRequired', { field: t('address') })),
  description: z.union([z.string().max(100, t('maxCharacters', { count: 100 })), z.null(), z.undefined()]).optional(),
  isActive: z.boolean().optional(),
});

export type EditEmployeeForm = z.infer<ReturnType<typeof createSchema>>;

interface Props {
  open: boolean;
  onClose: () => void;
  initialValues?: any;
  isCreateForm?: boolean;
}

export default function EmployeesFormModalComponent({ open, onClose, initialValues, isCreateForm = false }: Props) {
  const [avatarFile, setAvatarFile] = useState<File | null>(null);

  const { t } = useTranslate('employees');
  const schema = useMemo(() => createSchema(t), [t]);
  const queryClient = useQueryClient();
  const service = container.resolve(EmployeesService);
  const { notification } = App.useApp();

  const methods = useForm<EditEmployeeForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: '',
      role: 'employee',
      email: '',
      phone: '',
      address: '',
      description: '',
      isActive: true,
      ...initialValues,
    },
  });

  const {
    data: employee,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['employee-edit-details', initialValues?.id],
    queryFn: () => service.getDetails(initialValues?.id),
    enabled: !!initialValues?.id,
  });

  const createMutation = useMutation({
    mutationFn: async (data: Omit<EmployeeServiceDTO, 'id'> & { file?: File }) => {
      await service.create(data);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['employees-list'] });
      notification.success({ message: t('employeeAddedSuccessfully') });
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (data: Omit<EmployeeServiceDTO, 'id'> & { file?: File }) => {
      if (!initialValues?.id) throw new Error('Missing employee ID for update');
      await service.update(initialValues.id, data);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['employees-list'] });
      await queryClient.invalidateQueries({ queryKey: ['employee-details'] });
      notification.success({ message: t('employeeUpdatedSuccessfully') });
    },
  });

  const { reset, setValue, handleSubmit, control, watch } = methods;
  const isActiveValue = watch('isActive');

  useEffect(() => {
    if (isCreateForm) {
      // Reset form to empty state for create mode
      reset({
        name: '',
        role: 'employee',
        email: '',
        phone: '',
        address: '',
        description: '',
        isActive: true,
      });
      setAvatarFile(null);
    } else if (employee) {
      // Populate form with employee data for edit mode
      reset({
        name: employee.name,
        role: employee.role?.toLowerCase() as 'manager' | 'employee',
        email: employee.email,
        phone: employee.phone,
        address: employee.address,
        description: employee.description,
        isActive: employee.isActive,
      });
    }
  }, [employee, reset, isCreateForm]);

  const handleAvatarChange = (file: File) => {
    setAvatarFile(file);
    return false;
  };

  const submitForm = async (values: EditEmployeeForm) => {
    const payload: Omit<EmployeeServiceDTO, 'id'> & { file?: File } = {
      ...values,
      description: values.description === null ? undefined : values.description,
      file: avatarFile ?? undefined,
    };

    if (isCreateForm) {
      await createMutation.mutateAsync(payload);
    } else {
      await updateMutation.mutateAsync(payload);
    }

    onClose();
  };

  return (
    <AppModal
      loading={updateMutation.isPending || createMutation.isPending}
      open={open}
      onClose={onClose}
      title={isCreateForm ? t('addNewEmployee') : t('editEmployeeDetails')}
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
                <Avatar size={64} src={avatarFile ? URL.createObjectURL(avatarFile) : employee?.userProfileUrl} />
                <Upload beforeUpload={handleAvatarChange} showUploadList={false} accept='.jpg,.jpeg,.png,.pdf'>
                  <div className='absolute -right-1.5 -bottom-1.5 bg-[#3B82F6] flex items-center justify-center !text-white rounded-full w-8 h-8 cursor-pointer hover:bg-[#2563EB] transition-colors'>
                    <EditOutlined style={{ color: 'white', fontSize: 14 }} />
                  </div>
                </Upload>
              </div>
            </div>
          )}
          <Row gutter={16}>
            <Col span={12}>
              <AppTextField name='name' label={t('name')} placeholder={t('enterName')} />
            </Col>

            <Col span={12}>
              <Controller
                name='role'
                control={control}
                render={({ field, fieldState }) => (
                  <div>
                    <label className='block text-sm font-medium text-gray-700 mb-1'>{t('role')}</label>
                    <Select
                      {...field}
                      showSearch
                      filterOption={defaultSelectFilter}
                      options={[
                        { value: 'manager', label: t('manager') },
                        { value: 'employee', label: t('employee') },
                      ]}
                      placeholder={t('selectRole')}
                      className='w-full'
                      onChange={(value) => field.onChange(value)}
                      value={field.value}
                    />
                    {fieldState.error && <p className='text-red-600 text-xs mt-1'>{fieldState.error.message}</p>}
                  </div>
                )}
              />
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <AppTextField name='phone' label={t('phone')} placeholder={t('enterPhoneNumber')} />
            </Col>
            <Col span={12}>
              <AppTextField name='email' label={t('email')} placeholder={t('enterEmail')} />
            </Col>
          </Row>

          <AppTextField name='address' label={t('address')} placeholder={t('enterAddress')} />

          <AppTextField name='description' type='textarea' label={t('description')} placeholder={t('enterDescription')} />

          <div>
            <label className='block text-sm font-medium text-gray-700 mb-2'>{t('status')}</label>
            <Radio.Group value={isActiveValue} onChange={(e) => setValue('isActive', e.target.value)}>
              <Radio value={true}>{t('active')}</Radio>
              <Radio value={false}>{t('deactive')}</Radio>
            </Radio.Group>
          </div>
        </form>
      </FormProvider>
    </AppModal>
  );
}
