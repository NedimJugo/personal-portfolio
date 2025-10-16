import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { SkillResponse } from '../../../../models/skill/skill-response.model';
import { SkillService } from '../../../../services/skill.service';
import { MediaService } from '../../../../services/media.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { SkillInsertRequest } from '../../../../models/skill/skill-insert-request.model';
import { SkillUpdateRequest } from '../../../../models/skill/skill-update-request.model';
import { SkillSearchObject } from '../../../../models/skill/skill-search.model';

interface SkillWithIcon extends SkillResponse {
  iconUrl?: string;
}

@Component({
  selector: 'app-skill-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './skill-CRUD.component.html',
  styleUrls: ['./skill-CRUD.component.scss']
})
export class SkillCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  skillForm!: FormGroup;

  private destroy$ = new Subject<void>();

  skills: SkillWithIcon[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: SkillSearchObject = {
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

  currentSkillId: string | null = null;
  skillToDelete: SkillResponse | null = null;

  showMediaSelectorModal = false;
  mediaList: MediaResponse[] = [];
  isLoadingMedia = false;
  selectedMediaId: string | null = null;
  mediaSearchQuery = '';
  filteredMediaList: MediaResponse[] = [];

  // Skill-specific constants
  proficiencyLevels = [
    { value: 1, label: 'Beginner' },
    { value: 2, label: 'Basic' },
    { value: 3, label: 'Intermediate' },
    { value: 4, label: 'Advanced' },
    { value: 5, label: 'Expert' }
  ];

  skillCategories = [
    'Frontend',
    'Backend',
    'Full Stack',
    'DevOps',
    'Data Science',
    'Mobile',
    'Design',
    'Other'
  ];

  constructor(
    private skillService: SkillService,
    private mediaService: MediaService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initSkillForm();
    this.loadSkills();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      name: [''],
      category: [''],
      isFeatured: [''],
      minProficiency: [''],
      maxProficiency: [''],
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

  private initSkillForm(): void {
    this.skillForm = this.fb.group({
      name: ['', Validators.required],
      category: ['', Validators.required],
      proficiencyLevel: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      yearsExperience: [0, [Validators.required, Validators.min(0)]],
      isFeatured: [false],
      color: ['#4ECDC4'],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      iconMediaId: ['']
    });
  }

  loadSkills(): void {
    this.isLoading = true;

    this.skillService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.skills = result.items as SkillWithIcon[];
          this.totalCount = result.totalCount;
          this.loadIcons();
        },
        error: (error) => {
          console.error('Failed to load skills:', error);
        }
      });
  }

  private loadIcons(): void {
    this.skills.forEach(skill => {
      if (skill.iconMediaId) {
        this.mediaService.getById(skill.iconMediaId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              skill.iconUrl = media.fileUrl;
            },
            error: (error) => {
              console.error(`Failed to load icon for ${skill.name}:`, error);
            }
          });
      }
    });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    let isFeaturedValue: boolean | undefined;
    if (formValue.isFeatured === true || formValue.isFeatured === 'true') {
      isFeaturedValue = true;
    } else if (formValue.isFeatured === false || formValue.isFeatured === 'false') {
      isFeaturedValue = false;
    } else {
      isFeaturedValue = undefined;
    }

    this.searchParams = {
      ...this.searchParams,
      name: formValue.name?.trim() || undefined,
      category: formValue.category || undefined,
      isFeatured: isFeaturedValue,
      minProficiency: formValue.minProficiency ? parseInt(formValue.minProficiency) : undefined,
      maxProficiency: formValue.maxProficiency ? parseInt(formValue.maxProficiency) : undefined,
      sortBy: formValue.sortBy || 'displayOrder',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadSkills();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.name ||
      formValue.category ||
      formValue.isFeatured !== '' ||
      formValue.minProficiency ||
      formValue.maxProficiency
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      name: '',
      category: '',
      isFeatured: '',
      minProficiency: '',
      maxProficiency: '',
      sortBy: 'displayOrder',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadSkills();
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
    this.currentSkillId = null;
    this.formError = null;
    this.skillForm.reset({
      isFeatured: false,
      proficiencyLevel: 3,
      yearsExperience: 0,
      color: '#4ECDC4',
      displayOrder: this.skills.length
    });
    this.showModal = true;
  }

  openEditModal(skill: SkillResponse): void {
    this.isEditMode = true;
    this.currentSkillId = skill.id;
    this.formError = null;

    this.skillForm.patchValue({
      name: skill.name,
      category: skill.category,
      proficiencyLevel: skill.proficiencyLevel,
      yearsExperience: skill.yearsExperience,
      isFeatured: skill.isFeatured,
      color: skill.color || '#4ECDC4',
      displayOrder: skill.displayOrder,
      iconMediaId: skill.iconMediaId
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.skillForm.reset();
    this.currentSkillId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.skillForm.invalid) {
      this.skillForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.skillForm.value;

    const payload = {
      ...formValue,
      color: formValue.color || undefined,
      iconMediaId: formValue.iconMediaId || undefined
    };

    const request$ = this.isEditMode && this.currentSkillId
      ? this.skillService.update(this.currentSkillId, payload as SkillUpdateRequest)
      : this.skillService.create(payload as SkillInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadSkills();
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.skillForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(skill: SkillResponse): void {
    this.skillToDelete = skill;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.skillToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.skillToDelete) return;

    this.isDeleting = true;

    this.skillService.delete(this.skillToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadSkills();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  openMediaSelector(): void {
    this.showMediaSelectorModal = true;
    this.selectedMediaId = this.skillForm.get('iconMediaId')?.value || null;
    this.mediaSearchQuery = '';
    this.loadMediaList();
  }

  closeMediaSelector(): void {
    this.showMediaSelectorModal = false;
    this.mediaSearchQuery = '';
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
      this.skillForm.patchValue({ iconMediaId: this.selectedMediaId });
    }
    this.closeMediaSelector();
  }

  clearMediaSelection(): void {
    this.skillForm.patchValue({ iconMediaId: null });
    this.selectedMediaId = null;
  }

  getSelectedMediaPreview(): MediaResponse | null {
    const mediaId = this.skillForm.get('iconMediaId')?.value;
    if (!mediaId) return null;

    return this.mediaList.find(m => m.id === mediaId) ||
      this.filteredMediaList.find(m => m.id === mediaId) ||
      null;
  }

  getProficiencyLabel(level: number): string {
    const proficiency = this.proficiencyLevels.find(p => p.value === level);
    return proficiency ? proficiency.label : 'Unknown';
  }
}