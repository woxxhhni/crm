'use client';

import { useTranslate } from '@/locales/use-locales';
import { ConfirmDeleteModalComponent } from '@/components/modal/confirm-delete-modal.component';

interface Props {
    employee: any;
    onConfirm: () => void;
    onCancel: () => void;
    isLoading: boolean;
}

export default function EmployeesDeleteModalComponent({
    employee,
    onConfirm,
    onCancel,
    isLoading,
}: Props) {
    const { t } = useTranslate('employees');

    return (
        <ConfirmDeleteModalComponent
            open={true}
            onCancel={onCancel}
            onConfirm={onConfirm}
            title={t('deleteEmployee')}
            description={
                <>
                    {t('areYouSureYouWantToDeleteThisEmployee')} <strong>{employee?.name}</strong>?
                    <br />
                    <br />
                    {t('deletingThisEmployeeWillAutomaticallyUnassignAllOrders')}
                </>
            }
            okText={t('delete')}
            cancelText={t('cancel')}
            loading={isLoading}
        />
    );
}
