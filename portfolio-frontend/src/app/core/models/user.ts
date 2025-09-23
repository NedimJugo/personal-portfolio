export interface User {
  id: number;
  email: string;
  fullName: string;
  isActive: boolean;
  roles: string[];
  lastLoginAt?: Date;
}