'use client';

import { Modal } from 'antd';
import type { ReactNode } from 'react';
import { Icon } from '@/components/iconify/iconify.component';
import { useTranslate } from '@/locales/use-locales';

interface ConfirmDeleteModalProps {
    open: boolean;
    onCancel: () => void;
    onConfirm: () => void;

    title?: string;
    description?: ReactNode;
    okText?: string;
    cancelText?: string;
    icon?: string;
    danger?: boolean;
    centered?: boolean;
    loading?: boolean;
}

export const ConfirmDeleteModalComponent = ({
                                       open,
                                       onCancel,
                                       onConfirm,
                                       title,
                                       description,
                                       okText,
                                       cancelText,
                                       icon = 'closeCircle',
                                       danger = true,
                                       centered = true,
                                       loading = false,
                                   }: ConfirmDeleteModalProps) => {
    const { t } = useTranslate('common');
    const modalTitle = title || t('delete');

    const header: ReactNode = (
        <div className="flex items-center gap-2">
            <Icon icon={icon} width={22} className="text-red-500" />
            <p className="text-[16px] font-medium text-gray-800">{modalTitle}</p>
        </div>
    );

    return (
        <Modal
            open={open}
            title={header}
            okText={okText || t('delete')}
            cancelText={cancelText || t('cancel')}
            okButtonProps={{ danger, loading }}
            onCancel={onCancel}
            onOk={onConfirm}
            centered={centered}
            styles={{
                body: { paddingBottom: 16 }
            }}
        >
            <div className="text-gray-700 text-[15px] leading-relaxed">{description}</div>
        </Modal>
    );
};
