export interface SiteContentUpdateRequest {
  section: string;
  contentType: 'text' | 'html' | 'json';
  content: string;
  isPublished: boolean;
}