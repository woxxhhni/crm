'use client';

import { Spin } from 'antd';

export default function LoadingSpinnerComponent() {
    return (
        <div className="flex items-center justify-center h-full min-h-[200px]">
            <div className="flex flex-col items-center gap-3">
                <Spin spinning size="large" />
                <span className="text-sm" style={{ color: 'var(--text-muted)' }}>Loading...</span>
            </div>
        </div>
    );
}
