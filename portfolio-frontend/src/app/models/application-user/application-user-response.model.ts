export interface ApplicationUserResponse {
  id: number; // from IdentityUser<int>
  userName: string;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;   // ISO date string from backend
  updatedAt: string;   // ISO date string from backend
  lastLoginAt?: string; // nullable date
}