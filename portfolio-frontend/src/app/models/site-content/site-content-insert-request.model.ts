export interface SiteContentInsertRequest {
  section: string;
  contentType: 'text' | 'html' | 'json';
  content: string;
  isPublished: boolean;
}