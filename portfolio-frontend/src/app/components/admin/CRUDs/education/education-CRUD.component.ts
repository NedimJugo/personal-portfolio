import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { EducationResponse } from '../../../../models/education/education-response.model';
import { EducationService } from '../../../../services/education.service';
import { MediaService } from '../../../../services/media.service';
import { EducationWithLogo } from '../../../../models/education/education-with-logo.model';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { EducationSearchObject } from '../../../../models/education/education-search-object.model';
import { EducationInsertRequest } from '../../../../models/education/education-insert-request.model';
import { EducationUpdateRequest } from '../../../../models/education/education-update-request.model';

@Component({
  selector: 'app-education-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './education-CRUD.component.html',
  styleUrls: ['./education-CRUD.component.scss']
})
export class EducationCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  educationForm!: FormGroup;

  private destroy$ = new Subject<void>();

  educations: EducationWithLogo[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: EducationSearchObject = {
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

  currentEducationId: string | null = null;
  educationToDelete: EducationResponse | null = null;

  showMediaSelectorModal = false;
  mediaList: MediaResponse[] = [];
  isLoadingMedia = false;
  selectedMediaId: string | null = null;
  mediaSearchQuery = '';
  filteredMediaList: MediaResponse[] = [];

  constructor(
    private educationService: EducationService,
    private mediaService: MediaService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initEducationForm();
    this.loadEducations();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMediaList(): void {
    this.isLoadingMedia = true;
    this.filteredMediaList = []; // Clear previous results

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
          this.filteredMediaList = result.items; // Initialize filtered list
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
      institutionName: [''],
      educationType: [''],
      isCurrent: [''],
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

  private initEducationForm(): void {
    this.educationForm = this.fb.group({
      institutionName: ['', Validators.required],
      degree: ['', Validators.required],
      fieldOfStudy: [''],
      location: [''],
      startDate: ['', Validators.required],
      endDate: [''],
      isCurrent: [false],
      grade: [''],
      description: [''],
      educationType: ['', Validators.required],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      logoMediaId: ['']
    });

    this.educationForm.get('isCurrent')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(isCurrent => {
        if (isCurrent) {
          this.educationForm.patchValue({ endDate: null }, { emitEvent: false });
        }
      });
  }

  loadEducations(): void {
    this.isLoading = true;

    this.educationService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.educations = result.items as EducationWithLogo[];
          this.totalCount = result.totalCount;
          this.loadLogos();
        },
        error: (error) => {
          console.error('Failed to load educations:', error);
        }
      });
  }

  private loadLogos(): void {
    this.educations.forEach(education => {
      if (education.logoMediaId) {
        this.mediaService.getById(education.logoMediaId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              education.logoUrl = media.fileUrl;
            },
            error: (error) => {
              console.error(`Failed to load logo for ${education.institutionName}:`, error);
            }
          });
      }
    });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    // Normalize isCurrent properly
    let isCurrentValue: boolean | undefined;
    if (formValue.isCurrent === true || formValue.isCurrent === 'true') {
      isCurrentValue = true;
    } else if (formValue.isCurrent === false || formValue.isCurrent === 'false') {
      isCurrentValue = false;
    } else {
      isCurrentValue = undefined; // All Status
    }

    this.searchParams = {
      ...this.searchParams,
      institutionName: formValue.institutionName?.trim() || undefined,
      educationType: formValue.educationType || undefined,
      isCurrent: isCurrentValue, // ✅ properly handled
      sortBy: formValue.sortBy || 'displayOrder',
      desc: formValue.desc || false,
      page: 0
    };
  }



  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadEducations();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.institutionName ||
      formValue.educationType ||
      formValue.isCurrent !== ''
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      institutionName: '',
      educationType: '',
      isCurrent: '',
      sortBy: 'displayOrder',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadEducations();
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
    this.currentEducationId = null;
    this.formError = null;
    this.educationForm.reset({
      isCurrent: false,
      displayOrder: this.educations.length
    });
    this.showModal = true;
  }

  openEditModal(education: EducationResponse): void {
    this.isEditMode = true;
    this.currentEducationId = education.id;
    this.formError = null;

    this.educationForm.patchValue({
      institutionName: education.institutionName,
      degree: education.degree,
      fieldOfStudy: education.fieldOfStudy,
      location: education.location,
      startDate: this.formatDateForInput(education.startDate),
      endDate: education.endDate ? this.formatDateForInput(education.endDate) : null,
      isCurrent: education.isCurrent,
      grade: education.grade,
      description: education.description,
      educationType: education.educationType,
      displayOrder: education.displayOrder,
      logoMediaId: education.logoMediaId
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.educationForm.reset();
    this.currentEducationId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.educationForm.invalid) {
      this.educationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.educationForm.value;

    const payload = {
      ...formValue,
      fieldOfStudy: formValue.fieldOfStudy || undefined,
      location: formValue.location || undefined,
      endDate: formValue.endDate || undefined,
      grade: formValue.grade || undefined,
      description: formValue.description || undefined,
      logoMediaId: formValue.logoMediaId || undefined
    };

    const request$ = this.isEditMode && this.currentEducationId
      ? this.educationService.update(this.currentEducationId, payload as EducationUpdateRequest)
      : this.educationService.create(payload as EducationInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadEducations();
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.educationForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(education: EducationResponse): void {
    this.educationToDelete = education;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.educationToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.educationToDelete) return;

    this.isDeleting = true;

    this.educationService.delete(this.educationToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadEducations();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  openMediaSelector(): void {
    this.showMediaSelectorModal = true;
    this.selectedMediaId = this.educationForm.get('logoMediaId')?.value || null;
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
      this.educationForm.patchValue({ logoMediaId: this.selectedMediaId });
    }
    this.closeMediaSelector();
  }

  clearMediaSelection(): void {
    this.educationForm.patchValue({ logoMediaId: null });
    this.selectedMediaId = null;
  }

  getSelectedMediaPreview(): MediaResponse | null {
    const mediaId = this.educationForm.get('logoMediaId')?.value;
    if (!mediaId) return null;

    // First check the mediaList, then filteredMediaList as fallback
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
}