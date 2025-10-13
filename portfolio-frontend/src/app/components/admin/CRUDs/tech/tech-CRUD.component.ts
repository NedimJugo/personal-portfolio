import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { TechResponse } from '../../../../models/tech/tech-response.model';
import { TechService } from '../../../../services/tech.service';
import { MediaService } from '../../../../services/media.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { TechSearchObject } from '../../../../models/tech/tech-search.model';
import { TechInsertRequest } from '../../../../models/tech/tech-insert-request.model';
import { TechUpdateRequest } from '../../../../models/tech/tech-update-request.model';

interface TechWithIcon extends TechResponse {
    iconUrl?: string;
}

@Component({
    selector: 'app-tech-crud',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './tech-CRUD.component.html',
    styleUrls: ['./tech-CRUD.component.scss']
})
export class TechCrudComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;
    techForm!: FormGroup;

    private destroy$ = new Subject<void>();

    techs: TechWithIcon[] = [];
    totalCount: number | undefined;
    currentPage = 0;

    searchParams: TechSearchObject = {
        page: 0,
        pageSize: 20,
        sortBy: 'name',
        desc: false,
        includeTotalCount: true,
        retrieveAll: false
    };

    // Available categories for filtering and form
    categories = [
        'Frontend',
        'Backend',
        'Database',
        'DevOps',
        'Cloud',
        'Mobile',
        'Testing',
        'Design',
        'Other'
    ];

    isLoading = false;
    isSaving = false;
    isDeleting = false;
    showModal = false;
    showDeleteModal = false;
    isEditMode = false;
    formError: string | null = null;

    currentTechId: string | null = null;
    techToDelete: TechResponse | null = null;

    // Media selector state
    showMediaSelectorModal = false;
    mediaList: MediaResponse[] = [];
    isLoadingMedia = false;
    selectedMediaId: string | null = null;
    mediaSearchQuery = '';
    filteredMediaList: MediaResponse[] = [];

    constructor(
        private techService: TechService,
        private mediaService: MediaService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.initTechForm();
        this.loadTechs();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // ============================================
    // INITIALIZATION
    // ============================================

    private initSearchForm(): void {
        this.searchForm = this.fb.group({
            name: [''],
            category: [''],
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

    private initTechForm(): void {
        this.techForm = this.fb.group({
            name: ['', [Validators.required, Validators.maxLength(100)]],
            slug: ['', [Validators.required, Validators.maxLength(100)]],
            category: ['', Validators.required],
            iconMediaId: ['']
        });

        // Auto-generate slug from name
        this.techForm.get('name')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(name => {
                if (name && !this.isEditMode) {
                    const slug = this.generateSlug(name);
                    this.techForm.patchValue({ slug }, { emitEvent: false });
                }
            });
    }

    // ============================================
    // DATA LOADING
    // ============================================

    loadTechs(): void {
        this.isLoading = true;

        this.techService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.techs = result.items as TechWithIcon[];
                    this.totalCount = result.totalCount;
                    this.loadIcons();
                },
                error: (error) => {
                    console.error('Failed to load technologies:', error);
                }
            });
    }

    private loadIcons(): void {
        this.techs.forEach(tech => {
            if (tech.iconMediaId) {
                this.mediaService.getById(tech.iconMediaId)
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                        next: (media: MediaResponse) => {
                            tech.iconUrl = media.fileUrl;
                        },
                        error: (error) => {
                            console.error(`Failed to load icon for ${tech.name}:`, error);
                        }
                    });
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

    // ============================================
    // SEARCH & FILTERING
    // ============================================

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

        this.searchParams = {
            ...this.searchParams,
            name: formValue.name?.trim() || undefined,
            category: formValue.category || undefined,
            sortBy: formValue.sortBy || 'name',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadTechs();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(
            formValue.name ||
            formValue.category
        );
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            name: '',
            category: '',
            sortBy: 'name',
            desc: false
        });
    }

    // ============================================
    // PAGINATION
    // ============================================

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadTechs();
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

    // ============================================
    // MODAL MANAGEMENT
    // ============================================

    openCreateModal(): void {
        this.isEditMode = false;
        this.currentTechId = null;
        this.formError = null;
        this.techForm.reset({
            category: this.categories[0]
        });
        this.showModal = true;
    }

    openEditModal(tech: TechResponse): void {
        this.isEditMode = true;
        this.currentTechId = tech.id;
        this.formError = null;

        this.techForm.patchValue({
            name: tech.name,
            slug: tech.slug,
            category: tech.category,
            iconMediaId: tech.iconMediaId
        });

        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.techForm.reset();
        this.currentTechId = null;
        this.formError = null;
    }

    // ============================================
    // FORM SUBMISSION
    // ============================================

    submitForm(): void {
        if (this.techForm.invalid) {
            this.techForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        this.formError = null;

        const formValue = this.techForm.value;

        // Create the payload without undefined values
        const payload: any = {
            name: formValue.name.trim(),
            slug: formValue.slug.trim(),
            category: formValue.category
        };

        // Only include iconMediaId if it has a value (not null or empty string)
        if (formValue.iconMediaId) {
            payload.iconMediaId = formValue.iconMediaId;
        }

        console.log('Form Values:', formValue);
        console.log('Payload being sent:', payload);

        const request$ = this.isEditMode && this.currentTechId
            ? this.techService.update(this.currentTechId, payload as TechUpdateRequest)
            : this.techService.create(payload as TechInsertRequest);

        request$
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isSaving = false)
            )
            .subscribe({
                next: () => {
                    this.closeModal();
                    this.loadTechs();
                },
                error: (error) => {
                    console.error('Save failed with details:', {
                        error,
                        status: error.status,
                        statusText: error.statusText,
                        errorResponse: error.error,
                        url: error.url,
                        payload: payload
                    });

                    // Handle duplicate data specifically
                    if (this.isDuplicateError(error)) {
                        this.formError = this.getDuplicateErrorMessage(error, formValue);
                    } else {
                        this.formError = error.message || 'An error occurred while saving';
                    }
                }
            });
    }

    // Helper method to detect duplicate errors
    private isDuplicateError(error: any): boolean {
        // Check for 409 Conflict status (standard for duplicates)
        if (error.status === 409) {
            return true;
        }

        // Check for 400 Bad Request that indicates duplicate
        if (error.status === 400) {
            const errorMessage = (error.error?.message || error.message || '').toLowerCase();
            const duplicateKeywords = ['duplicate', 'already exists', 'already in use', 'unique constraint'];
            return duplicateKeywords.some(keyword => errorMessage.includes(keyword));
        }

        // Check if the original error indicates a database unique constraint violation
        if (error.originalError) {
            const originalMessage = (error.originalError.message || '').toLowerCase();
            const dbDuplicateKeywords = ['unique constraint', 'duplicate entry', 'violates unique constraint'];
            return dbDuplicateKeywords.some(keyword => originalMessage.includes(keyword));
        }

        return false;
    }

    // Helper method to generate user-friendly duplicate error messages
    private getDuplicateErrorMessage(error: any, formValue: any): string {
        const errorMessage = (error.error?.message || error.message || '').toLowerCase();

        // Try to detect which field caused the duplicate
        if (errorMessage.includes('name') || errorMessage.includes('technology name')) {
            return `A technology with the name "${formValue.name}" already exists. Please choose a different name.`;
        }

        if (errorMessage.includes('slug')) {
            return `The slug "${formValue.slug}" is already in use. Please choose a different slug.`;
        }

        // Generic duplicate message
        return 'This technology already exists in the system. Please check the name and slug for duplicates.';
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.techForm.get(fieldName);
        return !!(field && field.invalid && field.touched);
    }

    // ============================================
    // DELETE OPERATIONS
    // ============================================

    confirmDelete(tech: TechResponse): void {
        this.techToDelete = tech;
        this.showDeleteModal = true;
    }

    cancelDelete(): void {
        this.techToDelete = null;
        this.showDeleteModal = false;
    }

    executeDelete(): void {
        if (!this.techToDelete) return;

        this.isDeleting = true;

        this.techService.delete(this.techToDelete.id)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isDeleting = false)
            )
            .subscribe({
                next: () => {
                    this.cancelDelete();
                    this.loadTechs();
                },
                error: (error) => {
                    console.error('Delete failed:', error);
                }
            });
    }

    // ============================================
    // MEDIA SELECTOR
    // ============================================

    openMediaSelector(): void {
        this.showMediaSelectorModal = true;
        this.selectedMediaId = this.techForm.get('iconMediaId')?.value || null;
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
            this.techForm.patchValue({ iconMediaId: this.selectedMediaId });
        }
        this.closeMediaSelector();
    }

    clearMediaSelection(): void {
        this.techForm.patchValue({ iconMediaId: null });
        this.selectedMediaId = null;
    }

    getSelectedMediaPreview(): MediaResponse | null {
        const mediaId = this.techForm.get('iconMediaId')?.value;
        if (!mediaId) return null;

        return this.mediaList.find(m => m.id === mediaId) ||
            this.filteredMediaList.find(m => m.id === mediaId) ||
            null;
    }

    // ============================================
    // UTILITY METHODS
    // ============================================

    private generateSlug(name: string): string {
        return name
            .toLowerCase()
            .trim()
            .replace(/[^\w\s-]/g, '')
            .replace(/[\s_-]+/g, '-')
            .replace(/^-+|-+$/g, '');
    }

    getCategoryBadgeColor(category: string): string {
        const colorMap: { [key: string]: string } = {
            'Frontend': '#4ECDC4',
            'Backend': '#FF6B6B',
            'Database': '#A78BFA',
            'DevOps': '#ED8936',
            'Cloud': '#4299E1',
            'Mobile': '#48BB78',
            'Testing': '#F56565',
            'Design': '#FFD93D',
            'Other': '#718096'
        };
        return colorMap[category] || '#718096';
    }
}