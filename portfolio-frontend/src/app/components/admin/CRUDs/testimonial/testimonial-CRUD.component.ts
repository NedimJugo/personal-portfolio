import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { TestimonialResponse } from '../../../../models/testimonial/testimonial-response.model';

import { ProjectService } from '../../../../services/project.service';
import { TestimonialSearchObject } from '../../../../models/testimonial/testimonial-search.model';
import { TestimonialInsertRequest } from '../../../../models/testimonial/testimonial-insert-request.model';
import { TestimonialUpdateRequest } from '../../../../models/testimonial/testimonial-update-request.model';
import { ProjectResponse } from '../../../../models/project/project-response.model';
import { TestimonialService } from '../../../../services/testimonal.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { MediaService } from '../../../../services/media.service';

@Component({
    selector: 'app-testimonial-crud',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './testimonial-CRUD.component.html',
    styleUrls: ['./testimonial-CRUD.component.scss']
})
export class TestimonialCrudComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;
    testimonialForm!: FormGroup;

    private destroy$ = new Subject<void>();

    testimonials: TestimonialResponse[] = [];
    projects: ProjectResponse[] = [];
    totalCount: number | undefined;
    currentPage = 0;

    searchParams: TestimonialSearchObject = {
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
    isLoadingProjects = false;
    showModal = false;
    showDeleteModal = false;
    isEditMode = false;
    formError: string | null = null;

    currentTestimonialId: string | null = null;
    testimonialToDelete: TestimonialResponse | null = null;

    // Star rating array for display
    stars = [1, 2, 3, 4, 5];

    showMediaSelectorModal = false;
    mediaList: MediaResponse[] = [];
    isLoadingMedia = false;
    selectedMediaId: string | null = null;
    mediaSearchQuery = '';
    filteredMediaList: MediaResponse[] = [];

    constructor(
        private testimonialService: TestimonialService,
        private projectService: ProjectService,
        private mediaService: MediaService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.initTestimonialForm();
        this.loadTestimonials();
        this.loadProjects();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initSearchForm(): void {
        this.searchForm = this.fb.group({
            clientName: [''],
            isApproved: [''],
            isFeatured: [''],
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

    private initTestimonialForm(): void {
        this.testimonialForm = this.fb.group({
            clientName: ['', Validators.required],
            clientTitle: ['', Validators.required],
            clientCompany: ['', Validators.required],
            clientAvatar: [''],
            content: ['', [Validators.required, Validators.minLength(10)]],
            rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
            isApproved: [true],
            isFeatured: [false],
            displayOrder: [0, [Validators.required, Validators.min(0)]],
            projectId: ['']
        });
    }

    loadProjects(): void {
        this.isLoadingProjects = true;

        this.projectService.get({
            page: 0,
            pageSize: 100,
            isPublished: true,
            sortBy: 'title',
            desc: false
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoadingProjects = false)
            )
            .subscribe({
                next: (result) => {
                    this.projects = result.items;
                },
                error: (error) => {
                    console.error('Failed to load projects:', error);
                    this.projects = [];
                }
            });
    }

    loadTestimonials(): void {
        this.isLoading = true;

        this.testimonialService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.testimonials = result.items;
                    this.totalCount = result.totalCount;
                },
                error: (error) => {
                    console.error('Failed to load testimonials:', error);
                }
            });
    }

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

        // Normalize boolean filters
        let isApprovedValue: boolean | undefined;
        if (formValue.isApproved === true || formValue.isApproved === 'true') {
            isApprovedValue = true;
        } else if (formValue.isApproved === false || formValue.isApproved === 'false') {
            isApprovedValue = false;
        } else {
            isApprovedValue = undefined;
        }

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
            clientName: formValue.clientName?.trim() || undefined,
            isApproved: isApprovedValue,
            isFeatured: isFeaturedValue,
            sortBy: formValue.sortBy || 'displayOrder',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadTestimonials();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(
            formValue.clientName ||
            formValue.isApproved !== '' ||
            formValue.isFeatured !== ''
        );
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            clientName: '',
            isApproved: '',
            isFeatured: '',
            sortBy: 'displayOrder',
            desc: false
        });
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadTestimonials();
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
        this.currentTestimonialId = null;
        this.formError = null;
        this.testimonialForm.reset({
            rating: 5,
            isApproved: true,
            isFeatured: false,
            displayOrder: this.testimonials.length
        });
        this.showModal = true;
    }

    openEditModal(testimonial: TestimonialResponse): void {
        this.isEditMode = true;
        this.currentTestimonialId = testimonial.id;
        this.formError = null;

        this.testimonialForm.patchValue({
            clientName: testimonial.clientName,
            clientTitle: testimonial.clientTitle,
            clientCompany: testimonial.clientCompany,
            clientAvatar: testimonial.clientAvatar,
            content: testimonial.content,
            rating: testimonial.rating,
            isApproved: testimonial.isApproved,
            isFeatured: testimonial.isFeatured,
            displayOrder: testimonial.displayOrder,
            projectId: testimonial.projectId || ''
        });

        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.testimonialForm.reset();
        this.currentTestimonialId = null;
        this.formError = null;
    }

    submitForm(): void {
        if (this.testimonialForm.invalid) {
            this.testimonialForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        this.formError = null;

        const formValue = this.testimonialForm.value;

        const payload = {
            ...formValue,
            clientAvatar: formValue.clientAvatar || undefined,
            projectId: formValue.projectId || undefined
        };

        const request$ = this.isEditMode && this.currentTestimonialId
            ? this.testimonialService.update(this.currentTestimonialId, payload as TestimonialUpdateRequest)
            : this.testimonialService.create(payload as TestimonialInsertRequest);

        request$
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isSaving = false)
            )
            .subscribe({
                next: () => {
                    this.closeModal();
                    this.loadTestimonials();
                },
                error: (error) => {
                    this.formError = error.message || 'An error occurred while saving';
                    console.error('Save failed:', error);
                }
            });
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.testimonialForm.get(fieldName);
        return !!(field && field.invalid && field.touched);
    }

    confirmDelete(testimonial: TestimonialResponse): void {
        this.testimonialToDelete = testimonial;
        this.showDeleteModal = true;
    }

    cancelDelete(): void {
        this.testimonialToDelete = null;
        this.showDeleteModal = false;
    }

    executeDelete(): void {
        if (!this.testimonialToDelete) return;

        this.isDeleting = true;

        this.testimonialService.delete(this.testimonialToDelete.id)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isDeleting = false)
            )
            .subscribe({
                next: () => {
                    this.cancelDelete();
                    this.loadTestimonials();
                },
                error: (error) => {
                    console.error('Delete failed:', error);
                }
            });
    }

    // Helper method to get project name by ID
    getProjectName(projectId?: string): string {
        if (!projectId) return 'No Project';
        const project = this.projects.find(p => p.id === projectId);
        return project ? project.title : 'Unknown Project';
    }

    // Helper method to generate star rating array
    getRatingStars(rating: number): boolean[] {
        return this.stars.map(star => star <= rating);
    }

    // Helper method to format date
    formatDate(dateString: string | Date | undefined): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    openMediaSelector(): void {
        this.showMediaSelectorModal = true;
        this.selectedMediaId = this.testimonialForm.get('clientAvatar')?.value || null;
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
            const selectedMedia = this.mediaList.find(m => m.id === this.selectedMediaId);
            if (selectedMedia) {
                this.testimonialForm.patchValue({ clientAvatar: selectedMedia.fileUrl });
            }
        }
        this.closeMediaSelector();
    }

    clearMediaSelection(): void {
        this.testimonialForm.patchValue({ clientAvatar: null });
        this.selectedMediaId = null;
    }

    getSelectedMediaPreview(): MediaResponse | null {
        const avatarUrl = this.testimonialForm.get('clientAvatar')?.value;
        if (!avatarUrl) return null;

        return this.mediaList.find(m => m.fileUrl === avatarUrl) || null;
    }
}