'use client';

import { useMemo } from 'react';
import { App } from 'antd';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, FormProvider } from 'react-hook-form';
import { useMutation } from '@tanstack/react-query';
import { useTranslate } from '@/locales/use-locales';
import AppModal from '@/components/modal/app-modal.component';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';
import { container } from '@/services/di-container';
import { EmployeesService } from '@/features/employees/services/employees.service';

const createSchema = (t: (key: string, options?: Record<string, unknown>) => string) => z.object({
    newPassword: z.string().min(1, t('fieldIsRequired', { field: t('newPassword') })),
    confirmPassword: z.string().min(1, t('fieldIsRequired', { field: t('confirmPassword') })),
}).refine((data) => data.newPassword === data.confirmPassword, {
    message: t('passwordsDoNotMatch'),
    path: ['confirmPassword'],
});

type FormValues = z.infer<ReturnType<typeof createSchema>>;

interface Props {
    open: boolean;
    employeeId: number;
    onClose: () => void;
}

export default function EmployeesResetPasswordModalComponent({
                                                                 open,
                                                                 employeeId,
                                                                 onClose,
                                                             }: Props) {
    const { t } = useTranslate('employees');
    const schema = useMemo(() => createSchema(t), [t]);
    const { notification } = App.useApp();
    const service = container.resolve(EmployeesService);

    const methods = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            newPassword: '',
            confirmPassword: '',
        },
        mode: 'onTouched',
    });

    const { handleSubmit, reset } = methods;

    const resetMutation = useMutation(
        {mutationKey:['reset-password'],mutationFn:(data: FormValues) =>
            service.resetPassword(employeeId, data.newPassword, data.confirmPassword),
            onSuccess: () => {
                notification.success({ message: t('passwordUpdatedSuccessfully') });
                reset();
                onClose();
            },

        }
    );

    const onSubmit = (values: FormValues) => {
        resetMutation.mutate(values);
    };

    return (
        <AppModal
            loading={resetMutation.isPending}
            open={open}
            onClose={onClose}
            title={t('resetPassword')}
            onSubmit={handleSubmit(onSubmit)}
            submitText={t('save')}
            cancelText={t('cancel')}
            width={480}
        >
            <FormProvider {...methods}>
                <form className="space-y-6">
                    <AppTextField
                        name="newPassword"
                        type="password"
                        label={t('newPassword')}
                        placeholder={t('enterNewPassword')}
                    />

                    <AppTextField
                        name="confirmPassword"
                        type="password"
                        label={t('confirmPassword')}
                        placeholder={t('enterConfirmPassword')}
                    />
                </form>
            </FormProvider>
        </AppModal>
    );
}
