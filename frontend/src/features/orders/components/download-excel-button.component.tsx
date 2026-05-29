'use client';

import { useState } from 'react';
import { App } from 'antd';
import { AppButton } from '@/components/button/app-button.component';
import { useTranslate } from '@/locales/use-locales';
import { HttpClientInstance } from '@/services/http-client.service';
import { OrdersEndpoints } from '@/api/orders/endpoints';

interface Props {
  orderId: string | number;
  type: 'client' | 'provider';
}

interface DownloadResponse {
  contentBase64: string;
  fileName?: string;
  contentType?: string;
}

// Convert base64 to Blob
function base64ToBlob(base64: string, contentType: string): Blob {
  const byteCharacters = atob(base64);
  const byteNumbers = new Array(byteCharacters.length);
  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
  }
  const byteArray = new Uint8Array(byteNumbers);
  return new Blob([byteArray], { type: contentType });
}

export default function DownloadExcelButtonComponent({ orderId, type }: Props) {
  const [isDownloading, setIsDownloading] = useState(false);
  const { t } = useTranslate('orders');
  const { notification } = App.useApp();

  const handleDownload = async () => {
    try {
      setIsDownloading(true);

      const endpoint = type === 'client' ? `${OrdersEndpoints.clientPayments(orderId)}?downloadExcel=true` : `${OrdersEndpoints.providerPayments(orderId)}?downloadExcel=true`;

      const response = await HttpClientInstance.get<DownloadResponse>(endpoint);

      const contentType = response.data.contentType || 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
      const fileName = response.data.fileName || `${type}-payments-${orderId}.xlsx`;

      const blob = base64ToBlob(response.data.contentBase64, contentType);

      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch {
      // Error handled by global error handler
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <AppButton appVariant='secondary' onClick={handleDownload} loading={isDownloading}>
      {t('download')}
    </AppButton>
  );
}
