import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { MediaService } from '../../../../services/media.service';
import { MediaSearchObject } from '../../../../models/media/media-search.model';
import { MediaUpdateRequest } from '../../../../models/media/media-update-request.model';

@Component({
  selector: 'app-media-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './media-CRUD.component.html',
  styleUrls: ['./media-CRUD.component.scss']
})
export class MediaCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  mediaForm!: FormGroup;

  private destroy$ = new Subject<void>();

  mediaList: MediaResponse[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: MediaSearchObject = {
    page: 0,
    pageSize: 24,
    sortBy: 'uploadedAt',
    desc: true,
    includeTotalCount: true,
    retrieveAll: false
  };

  isLoading = false;
  isSaving = false;
  isDeleting = false;
  isUploading = false;
  uploadProgress = 0;
  
  showModal = false;
  showDeleteModal = false;
  showUploadModal = false;
  showBulkUploadModal = false;
  
  isEditMode = false;
  formError: string | null = null;

  currentMediaId: string | null = null;
  mediaToDelete: MediaResponse | null = null;

  // Upload handling
  selectedFile: File | null = null;
  selectedFiles: File[] = [];
  filePreviewUrl: string | null = null;
  filePreviewUrls: string[] = [];

  // Bulk selection
  selectedMediaIds: Set<string> = new Set();
  bulkActionMode = false;

  // Available file type filters
  fileTypes = ['image', 'video', 'audio', 'document', 'other'];
  
  // View mode
  viewMode: 'grid' | 'list' = 'grid';

  constructor(
    private mediaService: MediaService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initMediaForm();
    this.loadMedia();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      fileName: [''],
      fileType: [''],
      storageProvider: [''],
      sortBy: ['uploadedAt'],
      desc: [true]
    });

    this.searchForm.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        skip(1)
      )
      .subscribe(() => {
        this.onSearchChange();
      });
  }

  private initMediaForm(): void {
    this.mediaForm = this.fb.group({
      fileName: ['', Validators.required],
      originalFileName: ['', Validators.required],
      altText: [''],
      caption: [''],
      folder: ['']
    });
  }

  loadMedia(): void {
    this.isLoading = true;

    this.mediaService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.mediaList = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => {
          console.error('Failed to load media:', error);
          this.formError = 'Failed to load media files';
        }
      });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    this.searchParams = {
      ...this.searchParams,
      fileName: formValue.fileName?.trim() || undefined,
      fileType: formValue.fileType || undefined,
      storageProvider: formValue.storageProvider || undefined,
      sortBy: formValue.sortBy || 'uploadedAt',
      desc: formValue.desc !== false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadMedia();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.fileName ||
      formValue.fileType ||
      formValue.storageProvider
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      fileName: '',
      fileType: '',
      storageProvider: '',
      sortBy: 'uploadedAt',
      desc: true
    });
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  }

  // Pagination
  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadMedia();
  }

  getTotalPages(): number {
    if (!this.totalCount) return 1;
    return Math.ceil(this.totalCount / (this.searchParams.pageSize || 24));
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(0, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible);

    if (end - start < maxVisible) {
      start = Math.max(0, end - maxVisible);
    }

    for (let i = start; i < end; i++) {
      pages.push(i);
    }

    return pages;
  }

  // Upload Modal
  openUploadModal(): void {
    this.showUploadModal = true;
    this.selectedFile = null;
    this.filePreviewUrl = null;
    this.formError = null;
    this.mediaForm.reset();
  }

  closeUploadModal(): void {
    this.showUploadModal = false;
    this.selectedFile = null;
    this.filePreviewUrl = null;
    this.mediaForm.reset();
  }

  openBulkUploadModal(): void {
    this.showBulkUploadModal = true;
    this.selectedFiles = [];
    this.filePreviewUrls = [];
    this.formError = null;
  }

  closeBulkUploadModal(): void {
    this.showBulkUploadModal = false;
    this.selectedFiles = [];
    this.filePreviewUrls = [];
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      
      // Auto-populate form fields
      this.mediaForm.patchValue({
        fileName: this.selectedFile.name,
        originalFileName: this.selectedFile.name
      });

      // Generate preview for images
      if (this.selectedFile.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.filePreviewUrl = e.target?.result as string;
        };
        reader.readAsDataURL(this.selectedFile);
      } else {
        this.filePreviewUrl = null;
      }
    }
  }

  onMultipleFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFiles = Array.from(input.files);
      this.filePreviewUrls = [];

      // Generate previews for images
      this.selectedFiles.forEach((file) => {
        if (file.type.startsWith('image/')) {
          const reader = new FileReader();
          reader.onload = (e) => {
            this.filePreviewUrls.push(e.target?.result as string);
          };
          reader.readAsDataURL(file);
        }
      });
    }
  }

  removeSelectedFile(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.filePreviewUrls.splice(index, 1);
  }

  uploadFile(): void {
    if (!this.selectedFile || this.mediaForm.invalid) {
      this.mediaForm.markAllAsTouched();
      return;
    }

    this.isUploading = true;
    this.formError = null;

    const formValue = this.mediaForm.value;

    this.mediaService.uploadFile(
      this.selectedFile,
      formValue.altText || undefined,
      formValue.caption || undefined,
      formValue.folder || undefined
    )
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isUploading = false)
      )
      .subscribe({
        next: () => {
          this.closeUploadModal();
          this.loadMedia();
        },
        error: (error) => {
          this.formError = error.message || 'Failed to upload file';
          console.error('Upload failed:', error);
        }
      });
  }

  uploadMultipleFiles(): void {
    if (this.selectedFiles.length === 0) {
      this.formError = 'Please select at least one file';
      return;
    }

    this.isUploading = true;
    this.formError = null;

    const folder = this.mediaForm.get('folder')?.value || undefined;

    this.mediaService.uploadMultipleFiles(this.selectedFiles, folder)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isUploading = false)
      )
      .subscribe({
        next: () => {
          this.closeBulkUploadModal();
          this.loadMedia();
        },
        error: (error) => {
          this.formError = error.message || 'Failed to upload files';
          console.error('Bulk upload failed:', error);
        }
      });
  }

  // Edit Modal
  openEditModal(media: MediaResponse): void {
    this.isEditMode = true;
    this.currentMediaId = media.id;
    this.formError = null;

    this.mediaForm.patchValue({
      fileName: media.fileName,
      originalFileName: media.originalFileName,
      altText: media.altText,
      caption: media.caption,
      folder: media.folder
    });

    this.filePreviewUrl = media.fileUrl;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.mediaForm.reset();
    this.currentMediaId = null;
    this.formError = null;
    this.filePreviewUrl = null;
  }

  submitForm(): void {
    if (this.mediaForm.invalid) {
      this.mediaForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.mediaForm.value;

    const payload: MediaUpdateRequest = {
      fileName: formValue.fileName,
      originalFileName: formValue.originalFileName,
      altText: formValue.altText || undefined,
      caption: formValue.caption || undefined,
      folder: formValue.folder || undefined
    };

    if (!this.currentMediaId) {
      this.formError = 'No media selected for update';
      this.isSaving = false;
      return;
    }

    this.mediaService.update(this.currentMediaId, payload)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadMedia();
        },
        error: (error) => {
          this.formError = error.message || 'Failed to update media';
          console.error('Update failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.mediaForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  // Delete
  confirmDelete(media: MediaResponse): void {
    this.mediaToDelete = media;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.mediaToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.mediaToDelete) return;

    this.isDeleting = true;

    this.mediaService.delete(this.mediaToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadMedia();
        },
        error: (error) => {
          console.error('Delete failed:', error);
          this.formError = 'Failed to delete media';
        }
      });
  }

  // Bulk Selection
  toggleBulkMode(): void {
    this.bulkActionMode = !this.bulkActionMode;
    if (!this.bulkActionMode) {
      this.selectedMediaIds.clear();
    }
  }

  toggleMediaSelection(mediaId: string): void {
    if (this.selectedMediaIds.has(mediaId)) {
      this.selectedMediaIds.delete(mediaId);
    } else {
      this.selectedMediaIds.add(mediaId);
    }
  }

  selectAll(): void {
    this.mediaList.forEach(media => this.selectedMediaIds.add(media.id));
  }

  deselectAll(): void {
    this.selectedMediaIds.clear();
  }

  deleteBulk(): void {
    if (this.selectedMediaIds.size === 0) return;

    if (!confirm(`Delete ${this.selectedMediaIds.size} media files? This cannot be undone.`)) {
      return;
    }

    this.isDeleting = true;
    const idsToDelete = Array.from(this.selectedMediaIds);

    this.mediaService.deleteBulk(idsToDelete)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.selectedMediaIds.clear();
          this.bulkActionMode = false;
          this.loadMedia();
        },
        error: (error) => {
          console.error('Bulk delete failed:', error);
          this.formError = 'Failed to delete selected media';
        }
      });
  }

  // Download
  downloadMedia(media: MediaResponse): void {
    this.mediaService.downloadFile(media.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = media.originalFileName;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Download failed:', error);
        }
      });
  }

  // Utility
  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getFileTypeIcon(fileType: string): string {
    switch (fileType) {
      case 'image': return 'image';
      case 'video': return 'video';
      case 'audio': return 'music';
      case 'document': return 'file-text';
      default: return 'file';
    }
  }
}