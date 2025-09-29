export interface SocialLinkInsertRequest {
  platform: string;
  displayName: string;
  url: string;
  iconClass?: string;
  color?: string;
  isVisible: boolean;
  displayOrder: number;
}