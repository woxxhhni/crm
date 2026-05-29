import { Button, ButtonProps } from "antd";
import clsx from "clsx";

export type AppButtonVariant = 'primary' | 'secondary' | 'danger';

export interface IAppButtonProps extends ButtonProps {
    appVariant?: AppButtonVariant;
    className?: string;
    children?: React.ReactNode;
}

export const AppButton = ({ className, children, appVariant = 'primary', style, ...props }: IAppButtonProps) => {
    const variantStyles: Record<AppButtonVariant, React.CSSProperties> = {
        primary: {
            background: '#3B82F6',
            borderColor: '#3B82F6',
            color: '#FFFFFF',
            boxShadow: '0 1px 3px rgba(59,130,246,0.3)',
        },
        secondary: {
            background: 'transparent',
            borderColor: '#CBD5E1',
            color: '#334155',
        },
        danger: {
            background: '#EF4444',
            borderColor: '#EF4444',
            color: '#FFFFFF',
        },
    };

    return (
        <Button
            className={clsx(
                'transition-all duration-200 ease-out font-medium',
                className
            )}
            style={{
                borderRadius: 10,
                height: 40,
                paddingInline: 20,
                fontSize: 13,
                ...variantStyles[appVariant],
                ...style,
            }}
            onMouseEnter={(e) => {
                if (appVariant === 'primary') {
                    e.currentTarget.style.background = '#2563EB';
                } else if (appVariant === 'secondary') {
                    e.currentTarget.style.background = '#F1F5F9';
                }
            }}
            onMouseLeave={(e) => {
                e.currentTarget.style.background = variantStyles[appVariant].background as string || '';
            }}
            {...props}
        >
            {children}
        </Button>
    );
};
