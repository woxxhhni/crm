import {LayoutComponent} from "@/components/layout/layout.component";
import {AuthGuard} from "@/features/auth/sign-in/guard";


export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
      <AuthGuard>
          <LayoutComponent>
              {children}
          </LayoutComponent>
      </AuthGuard>
  );
}
