import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { SiteContentResponse } from '../../../../models/site-content/site-content-response.model';
import { SiteContentService } from '../../../../services/site-content.service';
import { SiteContentSearchObject } from '../../../../models/site-content/site-content-search.model';
import { SiteContentUpdateRequest } from '../../../../models/site-content/site-content-update-request.model';
import { SiteContentInsertRequest } from '../../../../models/site-content/site-content-insert-request.model';
import { MediaService } from '../../../../services/media.service';
import { MediaResponse } from '../../../../models/media/media-response.model';

@Component({
    selector: 'app-site-content-crud',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './site-content-CRUD.component.html',
    styleUrls: ['./site-content-CRUD.component.scss']
})
export class SiteContentCrudComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;
    contentForm!: FormGroup;

    private destroy$ = new Subject<void>();

    contents: SiteContentResponse[] = [];
    totalCount: number | undefined;
    currentPage = 0;

    searchParams: SiteContentSearchObject = {
        page: 0,
        pageSize: 20,
        sortBy: 'section',
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

    currentContentId: string | null = null;
    contentToDelete: SiteContentResponse | null = null;

    selectedMediaUrl: string = '';

    showContentPreview = false;
    lastSavedContent: string | null = null;
    autoSaveTimeout: any;
    hasUnsavedChanges = false;

    showMediaBrowser = false;
    mediaList: MediaResponse[] = [];
    filteredMediaList: MediaResponse[] = [];
    isLoadingMedia = false;
    mediaSearchQuery = '';
    selectedMediaForInsert: MediaResponse | null = null;

    editorOptions = {
        theme: 'vs-dark',
        language: 'json',
        automaticLayout: true,
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        wordWrap: 'on',
        lineNumbers: 'on',
        formatOnPaste: true,
        formatOnType: true
    };

    quillModules = {
        toolbar: [
            ['bold', 'italic', 'underline', 'strike'],
            ['blockquote', 'code-block'],
            [{ 'header': 1 }, { 'header': 2 }],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'color': [] }, { 'background': [] }],
            ['link', 'image'],
            ['clean']
        ]
    };

    constructor(
        private contentService: SiteContentService,
        private mediaService: MediaService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.initContentForm();
        this.loadContents();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initSearchForm(): void {
        this.searchForm = this.fb.group({
            section: [''],
            contentType: [''],
            isPublished: [''],
            sortBy: ['section'],
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

    private initContentForm(): void {
        this.contentForm = this.fb.group({
            section: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
            contentType: ['json', Validators.required],
            content: ['', Validators.required],
            isPublished: [false]
        });

        // Watch for content type changes with confirmation
        this.contentForm.get('contentType')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(type => {
                const currentContent = this.contentForm.get('content')?.value;
                if (currentContent && currentContent.trim() !== '' && currentContent !== this.lastSavedContent) {
                    if (confirm('Changing content type will clear your current content. Continue?')) {
                        this.contentForm.patchValue({ content: type === 'json' ? '{}' : '' }, { emitEvent: false });
                        this.lastSavedContent = null;
                    } else {
                        // Revert to previous type
                        const previousType = type === 'json' ? 'html' : type === 'html' ? 'text' : 'json';
                        this.contentForm.patchValue({ contentType: previousType }, { emitEvent: false });
                    }
                }
                this.validateContentFormat(type);
            });

        setTimeout(() => {
            this.contentForm.valueChanges
                .pipe(
                    takeUntil(this.destroy$),
                    debounceTime(500),
                    skip(1)
                )
                .subscribe(() => {
                    if (this.contentForm.dirty) {
                        this.hasUnsavedChanges = true;
                    }
                });
        }, 100);
    }


    togglePreview(): void {
        this.showContentPreview = !this.showContentPreview;
    }
    loadContents(): void {
        this.isLoading = true;

        this.contentService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.contents = result.items;
                    this.totalCount = result.totalCount;
                },
                error: (error) => {
                    console.error('Failed to load site content:', error);
                    this.formError = error.message || 'Failed to load content';
                }
            });
    }

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

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
            section: formValue.section?.trim() || undefined,
            contentType: formValue.contentType || undefined,
            isPublished: isPublishedValue,
            sortBy: formValue.sortBy || 'section',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadContents();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(
            formValue.section ||
            formValue.contentType ||
            formValue.isPublished !== ''
        );
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            section: '',
            contentType: '',
            isPublished: '',
            sortBy: 'section',
            desc: false
        });
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadContents();
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
        this.currentContentId = null;
        this.formError = null;
        this.hasUnsavedChanges = false;
        this.lastSavedContent = null;

        this.contentForm.reset({
            contentType: 'json',
            isPublished: false,
            content: '{}'
        });

        this.contentForm.markAsPristine();

        this.showModal = true;
    }

    openEditModal(content: SiteContentResponse): void {
        this.isEditMode = true;
        this.currentContentId = content.id;
        this.formError = null;
        this.hasUnsavedChanges = false;

        let formattedContent = content.content;
        if (content.contentType === 'json') {
            try {
                const parsed = JSON.parse(content.content);
                formattedContent = JSON.stringify(parsed, null, 2);
            } catch (e) {
                console.warn('Invalid JSON in content');
            }
        }

        this.contentForm.patchValue({
            section: content.section,
            contentType: content.contentType,
            content: formattedContent,
            isPublished: content.isPublished
        });

        this.lastSavedContent = formattedContent;
        this.contentForm.markAsPristine();
        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.contentForm.reset();
        this.currentContentId = null;
        this.formError = null;
    }

    validateContentFormat(type: string): void {
        const contentControl = this.contentForm.get('content');
        if (!contentControl) return;

        const content = contentControl.value;
        if (!content) return;

        if (type === 'json') {
            try {
                JSON.parse(content);
                contentControl.setErrors(null);
            } catch (e) {
                contentControl.setErrors({ invalidJson: true });
            }
        }
    }

    formatContent(content: string, type: string): string {
        if (type === 'json') {
            try {
                const parsed = JSON.parse(content);
                return JSON.stringify(parsed, null, 2);
            } catch (e) {
                return content;
            }
        }
        return content;
    }

    beautifyJson(): void {
        const content = this.contentForm.get('content')?.value;
        const type = this.contentForm.get('contentType')?.value;

        if (type === 'json' && content) {
            try {
                const parsed = JSON.parse(content);
                this.contentForm.patchValue({
                    content: JSON.stringify(parsed, null, 2)
                });
            } catch (e) {
                this.formError = 'Invalid JSON format';
                setTimeout(() => this.formError = null, 3000);
            }
        }
    }

    getHtmlPreview(): string {
        const content = this.contentForm.get('content')?.value;
        const type = this.contentForm.get('contentType')?.value;

        if (type === 'html') {
            return content;
        } else if (type === 'json') {
            try {
                return `<pre>${JSON.stringify(JSON.parse(content), null, 2)}</pre>`;
            } catch {
                return '<p style="color: red;">Invalid JSON</p>';
            }
        }
        return `<p>${content}</p>`;
    }

    submitForm(): void {
        if (this.contentForm.invalid) {
            this.contentForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        this.formError = null;

        const formValue = this.contentForm.value;

        // Minify JSON before saving
        let contentToSave = formValue.content;
        if (formValue.contentType === 'json') {
            try {
                const parsed = JSON.parse(formValue.content);
                contentToSave = JSON.stringify(parsed);
            } catch (e) {
                this.formError = 'Invalid JSON format';
                this.isSaving = false;
                return;
            }
        }

        const payload = {
            section: formValue.section,
            contentType: formValue.contentType,
            content: contentToSave,
            isPublished: formValue.isPublished
        };

        const request$ = this.isEditMode && this.currentContentId
            ? this.contentService.update(this.currentContentId, payload as SiteContentUpdateRequest)
            : this.contentService.create(payload as SiteContentInsertRequest);

        request$
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isSaving = false)
            )
            .subscribe({
                next: () => {
                    this.hasUnsavedChanges = false;
                    this.closeModal();
                    this.loadContents();
                },
                error: (error) => {
                    this.formError = error.message || 'An error occurred while saving';
                    console.error('Save failed:', error);
                }
            });
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.contentForm.get(fieldName);
        return !!(field && field.invalid && field.touched);
    }

    confirmDelete(content: SiteContentResponse): void {
        this.contentToDelete = content;
        this.showDeleteModal = true;
    }

    cancelDelete(): void {
        this.contentToDelete = null;
        this.showDeleteModal = false;
    }

    executeDelete(): void {
        if (!this.contentToDelete) return;

        this.isDeleting = true;

        this.contentService.delete(this.contentToDelete.id)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isDeleting = false)
            )
            .subscribe({
                next: () => {
                    this.cancelDelete();
                    this.loadContents();
                },
                error: (error) => {
                    console.error('Delete failed:', error);
                    this.formError = error.message || 'Failed to delete content';
                }
            });
    }

    togglePublishStatus(content: SiteContentResponse): void {
        const updatePayload: SiteContentUpdateRequest = {
            section: content.section,
            contentType: content.contentType,
            content: content.content,
            isPublished: !content.isPublished
        };

        this.contentService.update(content.id, updatePayload)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.loadContents();
                },
                error: (error) => {
                    console.error('Failed to toggle publish status:', error);
                }
            });
    }

    formatDate(dateString: Date | string | undefined): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    getContentPreview(content: string, type: string): string {
        if (type === 'json') {
            try {
                const parsed = JSON.parse(content);
                return JSON.stringify(parsed).substring(0, 100) + '...';
            } catch (e) {
                return content.substring(0, 100) + '...';
            }
        }
        return content.substring(0, 100) + '...';
    }

    getContentTypeIcon(type: string): string {
        switch (type) {
            case 'html': return '📝';
            case 'json': return '{}';
            case 'text': return '📄';
            default: return '📋';
        }
    }

    openMediaBrowser(): void {
        this.showMediaBrowser = true;
        this.mediaSearchQuery = '';
        this.selectedMediaForInsert = null;
        this.loadMediaList();
    }

    closeMediaBrowser(): void {
        this.showMediaBrowser = false;
        this.mediaSearchQuery = '';
        this.selectedMediaForInsert = null;
        this.selectedMediaUrl = '';
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

    filterMediaList(): void {
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

    selectMediaForInsert(media: MediaResponse): void {
        this.selectedMediaForInsert = this.selectedMediaForInsert?.id === media.id ? null : media;
        this.selectedMediaUrl = this.selectedMediaForInsert ? this.selectedMediaForInsert.fileUrl : '';
    }


    copyMediaUrl(url: string): void {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(url).then(() => {
                console.log('URL copied to clipboard');
            }).catch(err => {
                console.error('Failed to copy URL:', err);
            });
        }
    }
}