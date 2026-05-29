'use client';

import { useEffect } from 'react';
import { Row, Col, Button } from 'antd';
import { FormProvider, useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslate } from '@/locales/use-locales';
import AppModal from '@/components/modal/app-modal.component';
import { AppTextField } from '@/components/form/app-text-field/app-text-field.component';

const filterSchema = z.object({
    name: z.string().optional(),
    email: z.string().optional(),
    website: z.string().optional(),
    phone: z.string().optional(),
    address: z.string().optional(),
});

export type ClientsFilterForm = z.infer<typeof filterSchema>;

interface Props {
    open: boolean;
    onClose: () => void;
    onApply: (filters: ClientsFilterForm) => void;
    currentFilters?: ClientsFilterForm;
}


export default function ClientsFiltersModalComponent({ open, onClose, onApply,currentFilters }: Props) {
    const { t } = useTranslate('clients');

    const methods = useForm<ClientsFilterForm>({
        resolver: zodResolver(filterSchema),
        defaultValues: {
            name: '',
            email: '',
            website: '',
            phone: '',
            address: '',
        },
    });

    const { handleSubmit, reset } = methods;

    const handleReset = () => {
        reset({});
        onApply({});
        onClose();
    };

    const handleApply = (values: ClientsFilterForm) => {
        onApply(values);
        onClose();
    };

    useEffect(() => {
        if (!open) reset();
    }, [open]);

    useEffect(() => {
        if (open && currentFilters) {
            reset(currentFilters);
        }
    }, [open, currentFilters, reset]);


    return (
        <AppModal
            open={open}
            onClose={onClose}
            title={t('filters') || 'Filters'}
            width={520}
            submitText={t("apply")}
            onSubmit={handleSubmit(handleApply)}
            cancelText={t("reset")}
            onReset={handleReset}
        >
            <FormProvider {...methods}>
                <form onSubmit={handleSubmit(handleApply)} className="space-y-4">
                    <AppTextField name="name" label={t('name')} placeholder={t('enterYourName')} />
                    <AppTextField name="email" label={t('email')} placeholder={t('enterYourEmail')} />
                    <AppTextField name="website" label={t('website')} placeholder={t('enterYourWebsite')} />
                    <AppTextField name="phone" label={t('phone')} placeholder={t('enterYourNumber')} />
                    <AppTextField name="address" label={t('address')} placeholder={t('primaryAddress')} />
                </form>
            </FormProvider>
        </AppModal>
    );
}
