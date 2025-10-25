import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip, forkJoin } from 'rxjs';
import { ProjectResponse } from '../../../../models/project/project-response.model';
import { ProjectInsertRequest } from '../../../../models/project/project-insert-request.model';
import { ProjectUpdateRequest } from '../../../../models/project/project-update-request.model';
import { ProjectSearchObject } from '../../../../models/project/project-search.model';
import { ProjectService } from '../../../../services/project.service';
import { TagService } from '../../../../services/tag.service';
import { TechService } from '../../../../services/tech.service';
import { MediaService } from '../../../../services/media.service';
import { TagResponse } from '../../../../models/tag/tag-response.model';
import { TechResponse } from '../../../../models/tech/tech-response.model';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { ProjectWithImage } from '../../../../models/project-image/project-with-image.model';


@Component({
  selector: 'app-project-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './project-CRUD.component.html',
  styleUrls: ['./project-CRUD.component.scss']
})
export class ProjectCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  projectForm!: FormGroup;

  private destroy$ = new Subject<void>();

  projects: ProjectWithImage[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  // Reference data
  availableTags: TagResponse[] = [];
  availableTechs: TechResponse[] = [];
  mediaList: MediaResponse[] = [];
  filteredMediaList: MediaResponse[] = [];


  showTagDropdown = false;
  showTechDropdown = false;
  tagSearchQuery = '';
  techSearchQuery = '';

  searchParams: ProjectSearchObject = {
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
  isLoadingMedia = false;
  showModal = false;
  showDeleteModal = false;
  showMediaSelectorModal = false;
  isEditMode = false;
  formError: string | null = null;

  currentProjectId: string | null = null;
  projectToDelete: ProjectResponse | null = null;
  selectedMediaId: string | null = null;
  mediaSearchQuery = '';

  // Current user ID (should come from auth service in real app)
  currentUserId = 1;

  constructor(
    private projectService: ProjectService,
    private tagService: TagService,
    private techService: TechService,
    private mediaService: MediaService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initProjectForm();
    this.loadReferenceData();
    this.loadProjects();

    document.addEventListener('click', () => {
      this.showTagDropdown = false;
      this.showTechDropdown = false;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      title: [''],
      status: [''],
      projectType: [''],
      isFeatured: [''],
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

  private initProjectForm(): void {
    this.projectForm = this.fb.group({
      title: ['', Validators.required],
      slug: ['', Validators.required],
      shortDescription: ['', Validators.required],
      fullDescription: ['', Validators.required],
      projectType: ['web', Validators.required],
      status: ['completed', Validators.required],
      isFeatured: [false],
      featuredMediaId: [''],
      repoUrl: [''],
      liveUrl: [''],
      startDate: [''],
      endDate: [''],
      isPublished: [false],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      tagIds: [[]],
      techIds: [[]]
    });

    // Auto-generate slug from title
    this.projectForm.get('title')?.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300)
      )
      .subscribe(title => {
        if (title && !this.isEditMode) {
          const slug = this.generateSlug(title);
          this.projectForm.patchValue({ slug }, { emitEvent: false });
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

  private loadReferenceData(): void {
    forkJoin({
      tags: this.tagService.get({ page: 0, pageSize: 100, retrieveAll: true }),
      techs: this.techService.get({ page: 0, pageSize: 100, retrieveAll: true })
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.availableTags = result.tags.items;
          this.availableTechs = result.techs.items;
        },
        error: (error) => {
          console.error('Failed to load reference data:', error);
        }
      });
  }

  loadProjects(): void {
    this.isLoading = true;

    this.projectService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.projects = result.items as ProjectWithImage[];
          this.totalCount = result.totalCount;
          this.loadFeaturedImages();
        },
        error: (error) => {
          console.error('Failed to load projects:', error);
        }
      });
  }

  private loadFeaturedImages(): void {
    this.projects.forEach(project => {
      if (project.featuredMediaId) {
        this.mediaService.getById(project.featuredMediaId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              project.heroImageUrl = media.fileUrl;
              project.heroImageAlt = media.altText || project.title;
            },
            error: (error) => {
              console.error(`Failed to load featured image for ${project.title}:`, error);
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
      title: formValue.title?.trim() || undefined,
      status: formValue.status || undefined,
      projectType: formValue.projectType || undefined,
      isFeatured: isFeaturedValue,
      isPublished: isPublishedValue,
      sortBy: formValue.sortBy || 'displayOrder',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadProjects();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.title ||
      formValue.status ||
      formValue.projectType ||
      formValue.isFeatured !== '' ||
      formValue.isPublished !== ''
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      title: '',
      status: '',
      projectType: '',
      isFeatured: '',
      isPublished: '',
      sortBy: 'displayOrder',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadProjects();
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
    this.currentProjectId = null;
    this.formError = null;
    this.projectForm.reset({
      projectType: 'web',
      status: 'completed',
      isFeatured: false,
      isPublished: false,
      displayOrder: this.projects.length,
      tagIds: [],
      techIds: []
    });
    this.showModal = true;
  }

  openEditModal(project: ProjectResponse): void {
    this.isEditMode = true;
    this.currentProjectId = project.id;
    this.formError = null;

    this.projectForm.patchValue({
      title: project.title,
      slug: project.slug,
      shortDescription: project.shortDescription,
      fullDescription: project.fullDescription,
      projectType: project.projectType,
      status: project.status,
      isFeatured: project.isFeatured,
      featuredMediaId: project.featuredMediaId,
      repoUrl: project.repoUrl,
      liveUrl: project.liveUrl,
      startDate: project.startDate ? this.formatDateForInput(project.startDate) : null,
      endDate: project.endDate ? this.formatDateForInput(project.endDate) : null,
      isPublished: project.isPublished,
      displayOrder: project.displayOrder,
      tagIds: project.tagIds || [],
      techIds: project.techIds || []
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.projectForm.reset();
    this.currentProjectId = null;
    this.formError = null;
  }

submitForm(): void {
  if (this.projectForm.invalid) {
    this.projectForm.markAllAsTouched();
    return;
  }

  this.isSaving = true;
  this.formError = null;

  const formValue = this.projectForm.value;

  if (this.isEditMode && this.currentProjectId) {
    // UPDATE: Only send fields that are in ProjectUpdateRequest
    const updatePayload: ProjectUpdateRequest = {
      title: formValue.title,
      slug: formValue.slug,
      shortDescription: formValue.shortDescription,
      fullDescription: formValue.fullDescription,
      projectType: formValue.projectType,
      status: formValue.status,
      isFeatured: formValue.isFeatured,
      featuredMediaId: formValue.featuredMediaId || undefined,
      repoUrl: formValue.repoUrl || undefined,
      liveUrl: formValue.liveUrl || undefined,
      startDate: formValue.startDate || undefined,
      endDate: formValue.endDate || undefined,
      isPublished: formValue.isPublished,
      displayOrder: formValue.displayOrder,
      updatedById: this.currentUserId,
      tagIds: formValue.tagIds || [],
      techIds: formValue.techIds || []
    };

    console.log('=== UPDATE PAYLOAD ===');
    console.log('Project ID:', this.currentProjectId);
    console.log('Payload:', JSON.stringify(updatePayload, null, 2));
    console.log('TagIds type:', typeof updatePayload.tagIds, Array.isArray(updatePayload.tagIds));
    console.log('TechIds type:', typeof updatePayload.techIds, Array.isArray(updatePayload.techIds));
    console.log('=====================');

    this.projectService.update(this.currentProjectId, updatePayload)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          console.log('Update successful!');
          this.closeModal();
          this.loadProjects();
        },
        error: (error) => {
          console.error('=== UPDATE ERROR ===');
          console.error('Full error object:', error);
          console.error('Error originalError:', error.originalError);
          if (error.originalError?.error) {
            console.error('Backend error details:', error.originalError.error);
          }
          console.error('===================');
          this.formError = error.message || 'An error occurred while updating';
        }
      });
  } else {
    // CREATE: Send all required fields for ProjectInsertRequest
    const createPayload: ProjectInsertRequest = {
      title: formValue.title,
      slug: formValue.slug,
      shortDescription: formValue.shortDescription,
      fullDescription: formValue.fullDescription,
      projectType: formValue.projectType,
      status: formValue.status,
      isFeatured: formValue.isFeatured,
      featuredMediaId: formValue.featuredMediaId || undefined,
      repoUrl: formValue.repoUrl || undefined,
      liveUrl: formValue.liveUrl || undefined,
      startDate: formValue.startDate || undefined,
      endDate: formValue.endDate || undefined,
      isPublished: formValue.isPublished,
      displayOrder: formValue.displayOrder,
      createdById: this.currentUserId,
      updatedById: this.currentUserId,
      images: [],
      tagIds: formValue.tagIds || [],
      techIds: formValue.techIds || []
    };

    console.log('=== CREATE PAYLOAD ===');
    console.log('Payload:', JSON.stringify(createPayload, null, 2));
    console.log('=====================');

    this.projectService.create(createPayload)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          console.log('Create successful!');
          this.closeModal();
          this.loadProjects();
        },
        error: (error) => {
          console.error('=== CREATE ERROR ===');
          console.error('Full error object:', error);
          console.error('Error originalError:', error.originalError);
          if (error.originalError?.error) {
            console.error('Backend error details:', error.originalError.error);
          }
          console.error('===================');
          this.formError = error.message || 'An error occurred while creating';
        }
      });
  }
}

  isFieldInvalid(fieldName: string): boolean {
    const field = this.projectForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(project: ProjectResponse): void {
    this.projectToDelete = project;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.projectToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.projectToDelete) return;

    this.isDeleting = true;

    this.projectService.delete(this.projectToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadProjects();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
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

  openMediaSelector(): void {
    this.showMediaSelectorModal = true;
    this.selectedMediaId = this.projectForm.get('featuredMediaId')?.value || null;
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
      this.projectForm.patchValue({ featuredMediaId: this.selectedMediaId });
    }
    this.closeMediaSelector();
  }

  clearMediaSelection(): void {
    this.projectForm.patchValue({ featuredMediaId: null });
    this.selectedMediaId = null;
  }

  getSelectedMediaPreview(): MediaResponse | null {
    const mediaId = this.projectForm.get('featuredMediaId')?.value;
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

  getStatusBadgeClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'completed': 'badge-success',
      'in-progress': 'badge-warning',
      'planned': 'badge-info',
      'archived': 'badge-outline'
    };
    return statusMap[status] || 'badge-outline';
  }

  // Tag management
  toggleTagDropdown(): void {
    this.showTagDropdown = !this.showTagDropdown;
    this.showTechDropdown = false;
    this.tagSearchQuery = '';
  }

  getSelectedTags(): TagResponse[] {
    const selectedIds = this.projectForm.get('tagIds')?.value || [];
    return this.availableTags.filter(tag => selectedIds.includes(tag.id));
  }

  getFilteredTags(): TagResponse[] {
    if (!this.tagSearchQuery.trim()) {
      return this.availableTags;
    }
    const query = this.tagSearchQuery.toLowerCase();
    return this.availableTags.filter(tag =>
      tag.name.toLowerCase().includes(query)
    );
  }

  isTagSelected(tagId: string): boolean {
    const selectedIds = this.projectForm.get('tagIds')?.value || [];
    return selectedIds.includes(tagId);
  }

  toggleTag(tagId: string): void {
    const currentIds = this.projectForm.get('tagIds')?.value || [];
    const index = currentIds.indexOf(tagId);

    if (index > -1) {
      currentIds.splice(index, 1);
    } else {
      currentIds.push(tagId);
    }

    this.projectForm.patchValue({ tagIds: [...currentIds] });
  }

  removeTag(tagId: string): void {
    const currentIds = this.projectForm.get('tagIds')?.value || [];
    const filtered = currentIds.filter((id: string) => id !== tagId);
    this.projectForm.patchValue({ tagIds: filtered });
  }

  // Tech management
  toggleTechDropdown(): void {
    this.showTechDropdown = !this.showTechDropdown;
    this.showTagDropdown = false;
    this.techSearchQuery = '';
  }

  getSelectedTechs(): TechResponse[] {
    const selectedIds = this.projectForm.get('techIds')?.value || [];
    return this.availableTechs.filter(tech => selectedIds.includes(tech.id));
  }

  getFilteredTechs(): TechResponse[] {
    if (!this.techSearchQuery.trim()) {
      return this.availableTechs;
    }
    const query = this.techSearchQuery.toLowerCase();
    return this.availableTechs.filter(tech =>
      tech.name.toLowerCase().includes(query)
    );
  }

  isTechSelected(techId: string): boolean {
    const selectedIds = this.projectForm.get('techIds')?.value || [];
    return selectedIds.includes(techId);
  }

  toggleTech(techId: string): void {
    const currentIds = this.projectForm.get('techIds')?.value || [];
    const index = currentIds.indexOf(techId);

    if (index > -1) {
      currentIds.splice(index, 1);
    } else {
      currentIds.push(techId);
    }

    this.projectForm.patchValue({ techIds: [...currentIds] });
  }

  removeTech(techId: string): void {
    const currentIds = this.projectForm.get('techIds')?.value || [];
    const filtered = currentIds.filter((id: string) => id !== techId);
    this.projectForm.patchValue({ techIds: filtered });
  }
}