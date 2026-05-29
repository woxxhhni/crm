"use client";

import { Layout, Drawer } from "antd";
import { useState } from "react";
import { SidebarComponent } from "./sidebar.component";
import { HeaderComponent } from "./header.component";
import { useWindowSize } from "@/hooks/useWindowSize";
import { usePathname } from "next/navigation";

const { Sider } = Layout;

const SIDEBAR_WIDTH = 250;
const HEADER_HEIGHT = 64;

export const LayoutComponent = ({ children }: { children: React.ReactNode }) => {
    const { isMobile } = useWindowSize();
    const [drawerOpen, setDrawerOpen] = useState(false);
    const pathname = usePathname();

    const openDrawer = () => setDrawerOpen(true);
    const closeDrawer = () => setDrawerOpen(false);
    const isOrderDetails = pathname.startsWith("/panel/orders/details/");

    return (
        <div className="min-h-screen" style={{ background: 'var(--bg)' }}>
            {/* Fixed Sidebar */}
            {!isMobile && (
                <Sider
                    width={SIDEBAR_WIDTH}
                    className="!fixed left-0 top-0 bottom-0 z-20 overflow-y-auto"
                    style={{
                        background: 'var(--sidebar-bg)',
                        borderRight: '1px solid rgba(255,255,255,0.06)',
                    }}
                >
                    <SidebarComponent />
                </Sider>
            )}

            {/* Fixed Header */}
            <div
                className="fixed top-0 right-0 z-10"
                style={{ left: isMobile ? 0 : SIDEBAR_WIDTH }}
            >
                <HeaderComponent onMenu={openDrawer} />
            </div>

            {/* Main Content */}
            <main
                className="min-h-screen"
                style={{
                    marginLeft: isMobile ? 0 : SIDEBAR_WIDTH,
                    paddingTop: HEADER_HEIGHT,
                }}
            >
                <div className={`${isOrderDetails ? '' : 'p-6'}`}>
                    {children}
                </div>
            </main>

            {/* Mobile Drawer */}
            {isMobile && (
                <Drawer
                    placement="left"
                    open={drawerOpen}
                    onClose={closeDrawer}
                    width={250}
                    maskClosable={true}
                    styles={{
                        body: { padding: 0, background: 'var(--sidebar-bg)' },
                        header: { display: 'none' },
                    }}
                >
                    <SidebarComponent />
                </Drawer>
            )}
        </div>
    );
};
