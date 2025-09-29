export interface MediaInsertRequest {
  fileName: string;
  originalFileName: string;
  fileUrl: string;
  storageProvider: string;   // default: "Local"
  fileType: string;          // default: "image"
  fileSize: number;
  mimeType: string;
  width?: number;
  height?: number;
  altText?: string;
  caption?: string;
  folder?: string;
  uploadedById: number;
}
