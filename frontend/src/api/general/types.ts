export type UserRole = 'admin' | 'manager' | 'employee';

export interface CurrentUser {
  id: number;
  name: string;
  email: string;
  phone: string;
  role: UserRole;
  profileUrl: string;
}
