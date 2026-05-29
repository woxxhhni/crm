import "reflect-metadata";
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "@/styles/antd-custom.css";
import "@/styles/globals.css";

import { I18nextProviderClient } from "@/locales/i18-provider";
import { LocalizationProvider } from "@/locales/localization-provider";
import { ReactQueryProvider } from "@/services/query-client-provider";
import { App } from "antd";
import { AuthProvider } from "@/features/auth/sign-in/context/jwt";
import { PermissionProvider } from "@/features/permissions";
import { ThemeProvider } from "@/config/theme-provider";

const inter = Inter({
    subsets: ["latin"],
    display: "swap",
    variable: "--font-inter",
});

export const metadata: Metadata = {
    title: "CLS",
    description: "Canadian Logistics Solution",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en" dir="ltr" suppressHydrationWarning>
            <head />
            <body className={`${inter.variable} ${inter.className} antialiased`} suppressHydrationWarning>
                {/* eslint-disable-next-line @next/next/no-sync-scripts */}
                <script src="/__ENV.js" />
                <I18nextProviderClient>
                    <AuthProvider>
                        <PermissionProvider>
                            <ThemeProvider>
                                <App>
                                    <ReactQueryProvider>
                                        <LocalizationProvider>{children}</LocalizationProvider>
                                    </ReactQueryProvider>
                                </App>
                            </ThemeProvider>
                        </PermissionProvider>
                    </AuthProvider>
                </I18nextProviderClient>
            </body>
        </html>
    );
}
