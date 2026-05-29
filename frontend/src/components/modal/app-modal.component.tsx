'use client';

import { Modal, Button } from 'antd';
import { CloseOutlined } from '@ant-design/icons';
import { ReactNode } from 'react';
import { useTranslate } from '@/locales/use-locales';

interface AppModalProps {
    open: boolean;
    title?: ReactNode;
    onClose: () => void;
    onSubmit?: (data?: any) => void | Promise<void>;
    children?: ReactNode;
    width?: number | string;
    submitText?: string;
    cancelText?: string;
    loading?: boolean;
    onReset?: () => void;
    hideFooter?: boolean;
    disableSubmit?: boolean;
}

export default function AppModalComponent({
    open,
    title,
    onClose,
    onSubmit,
    onReset,
    children,
    width = 480,
    submitText,
    cancelText,
    loading = false,
    hideFooter = false,
    disableSubmit = false,
}: AppModalProps) {
    const { t } = useTranslate('common');

    return (
        <Modal
            open={open}
            title={title}
            onCancel={onClose}
            closeIcon={<CloseOutlined style={{ color: 'var(--text-muted)', fontSize: 14 }} />}
            footer={
                hideFooter ? null : (
                    <div className="flex justify-end gap-3 pt-2">
                        <Button
                            onClick={onReset ? onReset : onClose}
                            style={{
                                borderColor: 'var(--border)',
                                color: 'var(--text-muted)',
                            }}
                        >
                            {cancelText || t('cancel')}
                        </Button>
                        {onSubmit && (
                            <Button
                                type="primary"
                                onClick={onSubmit}
                                loading={loading}
                                disabled={disableSubmit}
                            >
                                {submitText || t('save')}
                            </Button>
                        )}
                    </div>
                )
            }
            width={width}
            styles={{
                body: { paddingTop: 16, paddingBottom: 8 },
            }}
        >
            {children}
        </Modal>
    );
}
