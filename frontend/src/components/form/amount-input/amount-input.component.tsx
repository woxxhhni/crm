'use client';

import { Input, InputProps, InputRef } from 'antd';
import { forwardRef, useCallback } from 'react';

export function formatWithSeparator(value: string | number | undefined | null): string {
    if (value === undefined || value === null || value === '') return '';
    const numStr = String(value).replace(/,/g, '');
    if (!/^\d*\.?\d*$/.test(numStr)) return String(value);
    const parts = numStr.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    return parts.join('.');
}

export function parseFormattedNumber(value: string): string {
    return value.replace(/,/g, '');
}

interface AppInputProps extends Omit<InputProps, 'onChange' | 'value'> {
    value?: string | number;
    onChange?: (value: string) => void;
    /** Only accept numeric characters (digits only, no letters or special chars) */
    number?: boolean;
    /** Add thousand separators (comma every 3 digits) */
    separate?: boolean;
}

export const AppInput = forwardRef<InputRef, AppInputProps>(
    ({ value, onChange, number = false, separate = false, ...props }, ref) => {
        const handleChange = useCallback(
            (e: React.ChangeEvent<HTMLInputElement>) => {
                let rawValue = e.target.value;

                // Remove separators if they exist
                if (separate) {
                    rawValue = rawValue.replace(/,/g, '');
                }

                // If number mode, only allow digits
                if (number) {
                    // Allow digits, one optional decimal point, and up to 2 decimal places
                    if (rawValue !== '' && !/^\d*\.?\d{0,2}$/.test(rawValue)) {
                        return; // Don't update if non-numeric or more than 2 decimals
                    }
                }

                onChange?.(rawValue);
            },
            [onChange, number, separate]
        );

        const displayValue = separate ? formatWithSeparator(value) : value;

        return (
            <Input
                {...props}
                ref={ref}
                value={displayValue}
                onChange={handleChange}
            />
        );
    }
);

AppInput.displayName = 'AppInput';

// Keep backward compatibility
export const AmountInput = forwardRef<InputRef, Omit<AppInputProps, 'number' | 'separate'> & { allowDecimal?: boolean }>(
    ({ allowDecimal, ...props }, ref) => {
        return <AppInput {...props} ref={ref} number separate />;
    }
);

AmountInput.displayName = 'AmountInput';
