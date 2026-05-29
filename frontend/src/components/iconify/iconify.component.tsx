import { forwardRef } from 'react';

// ----------------------------------------------------------------------

export interface IconifyProps {
    icon: string;
    className?: string;
    width?: number | string;
    height?: number | string;
    style?: React.CSSProperties;
    onClick?: () => void;
}

export const Icon = forwardRef<HTMLImageElement, IconifyProps>((props, ref) => {
    const {
        icon,
        className = '',
        width = 20,
        height = width,
        style,
        onClick,
        ...other
    } = props;

    return (
        <img
            ref={ref}
            src={`/assets/svg/${icon}.svg`}
            alt={icon}
            className={className}
            style={{
                width,
                height,
                display: 'inline-flex',
                flexShrink: 0,
                ...style,
            }}
            onClick={onClick}
            {...other}
        />
    );
});