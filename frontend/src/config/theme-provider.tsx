"use client";

import { ConfigProvider } from "antd";
import { themeConfig } from "@/config/theme";

/**
 * ThemeProvider — wraps the app with Ant Design ConfigProvider
 * using our light-mode design tokens.
 */
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  return (
    <ConfigProvider theme={themeConfig}>
      {children}
    </ConfigProvider>
  );
}
