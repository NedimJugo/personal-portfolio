import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { CertificateResponse } from '../../../../models/certificate/certificate-response.model';
import { CertificateService } from '../../../../services/certificate.service';
import { MediaService } from '../../../../services/media.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { CertificateSearchObject } from '../../../../models/certificate/certificate-search-object.model';
import { CertificateInsertRequest } from '../../../../models/certificate/certificate-insert-request.model';
import { CertificateUpdateRequest } from '../../../../models/certificate/certificate-update-request.model';
import { CertificateWithLogos } from '../../../../models/certificate/certificate-with-logo.model';

@Component({
  selector: 'app-certificate-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './certificate-CRUD.component.html',
  styleUrls: ['./certificate-CRUD.component.scss']
})
export class CertificateCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  certificateForm!: FormGroup;

  private destroy$ = new Subject<void>();

  certificates: CertificateWithLogos[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: CertificateSearchObject = {
    page: 0,
    pageSize: 20,
    sortBy: 'displayOrder',
    desc: false,
    includeTotalCount: true,
    retrieveAll: false
  };

  isLoading = false;
  isSaving = false;
  isDeleting = false;
  showModal = false;
  showDeleteModal = false;
  isEditMode = false;
  formError: string | null = null;

  currentCertificateId: string | null = null;
  certificateToDelete: CertificateResponse | null = null;

  // Media selector state
  showMediaSelectorModal = false;
  mediaList: MediaResponse[] = [];
  isLoadingMedia = false;
  selectedMediaId: string | null = null;
  mediaSearchQuery = '';
  filteredMediaList: MediaResponse[] = [];
  currentMediaFieldType: 'logo' | 'certificate' = 'logo';

  constructor(
    private certificateService: CertificateService,
    private mediaService: MediaService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initCertificateForm();
    this.loadCertificates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMediaList(): void {
    this.isLoadingMedia = true;
    this.filteredMediaList = [];

    this.mediaService.get({
      page: 0,
      pageSize: 100,
      fileType: 'image',
      sortBy: 'uploadedAt',
      desc: true
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoadingMedia = false)
      )
      .subscribe({
        next: (result) => {
          this.mediaList = result.items;
          this.filteredMediaList = result.items;
        },
        error: (error) => {
          console.error('Failed to load media:', error);
          this.mediaList = [];
          this.filteredMediaList = [];
        }
      });
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      name: [''],
      issuingOrganization: [''],
      certificateType: [''],
      isActive: [''],
      isPublished: [''],
      sortBy: ['displayOrder'],
      desc: [false]
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

  private initCertificateForm(): void {
    this.certificateForm = this.fb.group({
      name: ['', Validators.required],
      issuingOrganization: ['', Validators.required],
      issueDate: ['', Validators.required],
      expirationDate: [''],
      credentialId: [''],
      credentialUrl: [''],
      description: [''],
      skills: [''],
      certificateType: ['', Validators.required],
      isActive: [true],
      isPublished: [true],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      logoMediaId: [''],
      certificateMediaId: ['']
    });
  }

  loadCertificates(): void {
    this.isLoading = true;

    this.certificateService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.certificates = result.items as CertificateWithLogos[];
          this.totalCount = result.totalCount;
          this.loadMediaAssets();
        },
        error: (error) => {
          console.error('Failed to load certificates:', error);
        }
      });
  }

  private loadMediaAssets(): void {
    this.certificates.forEach(certificate => {
      // Load logo
      if (certificate.logoMediaId) {
        this.mediaService.getById(certificate.logoMediaId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              certificate.logoUrl = media.fileUrl;
            },
            error: (error) => {
              console.error(`Failed to load logo for ${certificate.name}:`, error);
            }
          });
      }

      // Load certificate image
      if (certificate.certificateMediaId) {
        this.mediaService.getById(certificate.certificateMediaId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              certificate.certificateUrl = media.fileUrl;
            },
            error: (error) => {
              console.error(`Failed to load certificate image for ${certificate.name}:`, error);
            }
          });
      }
    });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    // Normalize boolean filters
    let isActiveValue: boolean | undefined;
    if (formValue.isActive === true || formValue.isActive === 'true') {
      isActiveValue = true;
    } else if (formValue.isActive === false || formValue.isActive === 'false') {
      isActiveValue = false;
    } else {
      isActiveValue = undefined;
    }

    let isPublishedValue: boolean | undefined;
    if (formValue.isPublished === true || formValue.isPublished === 'true') {
      isPublishedValue = true;
    } else if (formValue.isPublished === false || formValue.isPublished === 'false') {
      isPublishedValue = false;
    } else {
      isPublishedValue = undefined;
    }

    this.searchParams = {
      ...this.searchParams,
      name: formValue.name?.trim() || undefined,
      issuingOrganization: formValue.issuingOrganization?.trim() || undefined,
      certificateType: formValue.certificateType || undefined,
      isActive: isActiveValue,
      isPublished: isPublishedValue,
      sortBy: formValue.sortBy || 'displayOrder',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadCertificates();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.name ||
      formValue.issuingOrganization ||
      formValue.certificateType ||
      formValue.isActive !== '' ||
      formValue.isPublished !== ''
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      name: '',
      issuingOrganization: '',
      certificateType: '',
      isActive: '',
      isPublished: '',
      sortBy: 'displayOrder',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadCertificates();
  }

  getTotalPages(): number {
    if (!this.totalCount) return 1;
    return Math.ceil(this.totalCount / (this.searchParams.pageSize || 20));
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

  openCreateModal(): void {
    this.isEditMode = false;
    this.currentCertificateId = null;
    this.formError = null;
    this.certificateForm.reset({
      isActive: true,
      isPublished: true,
      displayOrder: this.certificates.length
    });
    this.showModal = true;
  }

  openEditModal(certificate: CertificateResponse): void {
    this.isEditMode = true;
    this.currentCertificateId = certificate.id;
    this.formError = null;

    this.certificateForm.patchValue({
      name: certificate.name,
      issuingOrganization: certificate.issuingOrganization,
      issueDate: this.formatDateForInput(certificate.issueDate),
      expirationDate: certificate.expirationDate ? this.formatDateForInput(certificate.expirationDate) : null,
      credentialId: certificate.credentialId,
      credentialUrl: certificate.credentialUrl,
      description: certificate.description,
      skills: certificate.skills,
      certificateType: certificate.certificateType,
      isActive: certificate.isActive,
      isPublished: certificate.isPublished,
      displayOrder: certificate.displayOrder,
      logoMediaId: certificate.logoMediaId,
      certificateMediaId: certificate.certificateMediaId
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.certificateForm.reset();
    this.currentCertificateId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.certificateForm.invalid) {
      this.certificateForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.certificateForm.value;

    const payload = {
      ...formValue,
      expirationDate: formValue.expirationDate || undefined,
      credentialId: formValue.credentialId || undefined,
      credentialUrl: formValue.credentialUrl || undefined,
      description: formValue.description || undefined,
      skills: formValue.skills || undefined,
      logoMediaId: formValue.logoMediaId || undefined,
      certificateMediaId: formValue.certificateMediaId || undefined
    };

    const request$ = this.isEditMode && this.currentCertificateId
      ? this.certificateService.update(this.currentCertificateId, payload as CertificateUpdateRequest)
      : this.certificateService.create(payload as CertificateInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadCertificates();
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.certificateForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(certificate: CertificateResponse): void {
    this.certificateToDelete = certificate;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.certificateToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.certificateToDelete) return;

    this.isDeleting = true;

    this.certificateService.delete(this.certificateToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadCertificates();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  openMediaSelector(fieldType: 'logo' | 'certificate'): void {
    this.currentMediaFieldType = fieldType;
    this.showMediaSelectorModal = true;
    
    const fieldName = fieldType === 'logo' ? 'logoMediaId' : 'certificateMediaId';
    this.selectedMediaId = this.certificateForm.get(fieldName)?.value || null;
    this.mediaSearchQuery = '';
    this.loadMediaList();
  }

  closeMediaSelector(): void {
    this.showMediaSelectorModal = false;
    this.mediaSearchQuery = '';
  }

  filterMedia(): void {
    if (!this.mediaSearchQuery.trim()) {
      this.filteredMediaList = [...this.mediaList];
      return;
    }

    const query = this.mediaSearchQuery.toLowerCase().trim();
    this.filteredMediaList = this.mediaList.filter(media =>
      media.originalFileName?.toLowerCase().includes(query) ||
      media.fileName?.toLowerCase().includes(query) ||
      (media.altText && media.altText.toLowerCase().includes(query))
    );
  }

  selectMedia(mediaId: string): void {
    this.selectedMediaId = this.selectedMediaId === mediaId ? null : mediaId;
  }

  confirmMediaSelection(): void {
    if (this.selectedMediaId) {
      const fieldName = this.currentMediaFieldType === 'logo' ? 'logoMediaId' : 'certificateMediaId';
      this.certificateForm.patchValue({ [fieldName]: this.selectedMediaId });
    }
    this.closeMediaSelector();
  }

  clearMediaSelection(fieldType: 'logo' | 'certificate'): void {
    const fieldName = fieldType === 'logo' ? 'logoMediaId' : 'certificateMediaId';
    this.certificateForm.patchValue({ [fieldName]: null });
  }

  getSelectedMediaPreview(fieldType: 'logo' | 'certificate'): MediaResponse | null {
    const fieldName = fieldType === 'logo' ? 'logoMediaId' : 'certificateMediaId';
    const mediaId = this.certificateForm.get(fieldName)?.value;
    if (!mediaId) return null;

    return this.mediaList.find(m => m.id === mediaId) ||
      this.filteredMediaList.find(m => m.id === mediaId) ||
      null;
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short' });
  }

  private formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  }

  hasExpired(certificate: CertificateResponse): boolean {
    if (!certificate.expirationDate) return false;
    return new Date(certificate.expirationDate) < new Date();
  }
}