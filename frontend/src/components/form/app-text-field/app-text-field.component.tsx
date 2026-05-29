'use client';

import { Controller, useFormContext } from 'react-hook-form';
import { Input } from 'antd';
import React from "react";

type TextFieldType = 'text' | 'password' | 'textarea';

interface AppTextFieldProps {
    name: string;
    label?: string;
    placeholder?: string;
    type?: TextFieldType;
}

export function AppTextField({
                                 name,
                                 label,
                                 placeholder,
                                 type = 'text',
                             }: AppTextFieldProps) {
    const { control, formState: { errors } } = useFormContext();
    const error = errors[name]?.message as string | undefined;

    const renderInput = () => {
        if (type === 'password') {
            return <Input.Password placeholder={placeholder} className={error ? 'border-red-500' : ''} />;
        }
        if (type === 'textarea') {
            return <Input.TextArea rows={3} placeholder={placeholder} className={error ? 'border-red-500' : ''} />;
        }
        return <Input placeholder={placeholder} className={error ? 'border-red-500' : ''} />;
    };

    return (
        <div className="w-full">
            {label && (
                <label className="block mb-1 text-sm font-medium text-gray-700">
                    {label}
                </label>
            )}
            <Controller
                name={name}
                control={control}
                render={({ field }) => {
                    const Comp = renderInput();
                    return React.cloneElement(Comp, {
                        ...field,
                    });
                }}
            />
            {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
        </div>
    );
}
