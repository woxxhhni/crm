import { useState, useEffect } from "react";

export interface WindowSize {
    width: number;
    height: number;
    isMobile: boolean;
}

export function useWindowSize(): WindowSize {
    const [size, setSize] = useState<WindowSize>({
        width: 0,
        height: 0,
        isMobile: false,
    });

    useEffect(() => {
        if (typeof window === "undefined") return;

        const handleResize = () => {
            const width = window.innerWidth;
            const height = window.innerHeight;

            setSize({
                width,
                height,
                isMobile: width < 992,
            });
        };
        handleResize();
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

    return size;
}
