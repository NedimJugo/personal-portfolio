import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { TagResponse } from '../../../../models/tag/tag-response.model';
import { TagService } from '../../../../services/tag.service';
import { TagSearchObject } from '../../../../models/tag/tag-search.model';
import { TagInsertRequest } from '../../../../models/tag/tag-insert-request.model';
import { TagUpdateRequest } from '../../../../models/tag/tag-update-request.model';

@Component({
  selector: 'app-tag-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tag-CRUD.component.html',
  styleUrls: ['./tag-CRUD.component.scss']
})
export class TagCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  tagForm!: FormGroup;

  private destroy$ = new Subject<void>();

  tags: TagResponse[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: TagSearchObject = {
    page: 0,
    pageSize: 20,
    sortBy: 'name',
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

  currentTagId: string | null = null;
  tagToDelete: TagResponse | null = null;

  constructor(
    private tagService: TagService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initTagForm();
    this.loadTags();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      name: [''],
      slug: [''],
      sortBy: ['name'],
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

  private initTagForm(): void {
    this.tagForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/), Validators.maxLength(50)]]
    });

    // Auto-generate slug from name
    this.tagForm.get('name')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(name => {
        if (name && !this.isEditMode) {
          const slug = this.generateSlug(name);
          this.tagForm.patchValue({ slug }, { emitEvent: false });
        }
      });
  }

  private generateSlug(text: string): string {
    return text
      .toLowerCase()
      .trim()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  loadTags(): void {
    this.isLoading = true;

    this.tagService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.tags = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => {
          console.error('Failed to load tags:', error);
        }
      });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    this.searchParams = {
      ...this.searchParams,
      name: formValue.name?.trim() || undefined,
      slug: formValue.slug?.trim() || undefined,
      sortBy: formValue.sortBy || 'name',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadTags();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(formValue.name || formValue.slug);
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      name: '',
      slug: '',
      sortBy: 'name',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadTags();
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
    this.currentTagId = null;
    this.formError = null;
    this.tagForm.reset();
    this.showModal = true;
  }

  openEditModal(tag: TagResponse): void {
    this.isEditMode = true;
    this.currentTagId = tag.id;
    this.formError = null;

    this.tagForm.patchValue({
      name: tag.name,
      slug: tag.slug
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.tagForm.reset();
    this.currentTagId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.tagForm.invalid) {
      this.tagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.tagForm.value;

    const request$ = this.isEditMode && this.currentTagId
      ? this.tagService.update(this.currentTagId, formValue as TagUpdateRequest)
      : this.tagService.create(formValue as TagInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadTags();
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.tagForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.tagForm.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    if (field.errors['required']) return `${fieldName} is required`;
    if (field.errors['minlength']) return `${fieldName} must be at least ${field.errors['minlength'].requiredLength} characters`;
    if (field.errors['maxlength']) return `${fieldName} must be at most ${field.errors['maxlength'].requiredLength} characters`;
    if (field.errors['pattern']) return 'Slug must be lowercase letters, numbers, and hyphens only';

    return 'Invalid value';
  }

  confirmDelete(tag: TagResponse): void {
    this.tagToDelete = tag;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.tagToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.tagToDelete) return;

    this.isDeleting = true;

    this.tagService.delete(this.tagToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadTags();
        },
        error: (error) => {
          console.error('Delete failed:', error);
          this.formError = error.message || 'Failed to delete tag';
        }
      });
  }
}