import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { SocialLinkResponse } from '../../../../models/social-link/social-link-response.model';
import { SocialLinkService } from '../../../../services/social-link.service';
import { SocialLinkSearchObject } from '../../../../models/social-link/social-link-search.model';
import { SocialLinkInsertRequest } from '../../../../models/social-link/social-link-insert-request.model';
import { SocialLinkUpdateRequest } from '../../../../models/social-link/social-link-update-request.model';

@Component({
  selector: 'app-social-link-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './social-link-CRUD.component.html',
  styleUrls: ['./social-link-CRUD.component.scss']
})
export class SocialLinkCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  socialLinkForm!: FormGroup;

  private destroy$ = new Subject<void>();

  socialLinks: SocialLinkResponse[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: SocialLinkSearchObject = {
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
  showAllPlatforms = false;

  currentSocialLinkId: string | null = null;
  socialLinkToDelete: SocialLinkResponse | null = null;

  // Popular social platforms for quick selection
  popularPlatforms = [
    { name: 'GitHub', icon: 'fab fa-github', color: '#333333' },
    { name: 'LinkedIn', icon: 'fab fa-linkedin', color: '#0A66C2' },
    { name: 'X', icon: 'fab fa-x-twitter', color: '#000000' },
    { name: 'Instagram', icon: 'fab fa-instagram', color: '#E4405F' },
    { name: 'Facebook', icon: 'fab fa-facebook', color: '#1877F2' },
    { name: 'YouTube', icon: 'fab fa-youtube', color: '#FF0000' },
    { name: 'TikTok', icon: 'fab fa-tiktok', color: '#000000' },
    { name: 'Discord', icon: 'fab fa-discord', color: '#5865F2' },
    { name: 'Medium', icon: 'fab fa-medium', color: '#000000' },
    { name: 'Stack Overflow', icon: 'fab fa-stack-overflow', color: '#F58025' },
    { name: 'Dribbble', icon: 'fab fa-dribbble', color: '#EA4C89' },
    { name: 'Behance', icon: 'fab fa-behance', color: '#1769FF' },
     { name: 'Threads', icon: 'fab fa-threads', color: '#000000' },   
    { name: 'Bluesky', icon: 'fa-brands fa-bluesky', color: '#1185FE' },
    { name: 'Telegram', icon: 'fab fa-telegram', color: '#0088CC' },  
    { name: 'Pinterest', icon: 'fab fa-pinterest', color: '#E60023' }, 
    { name: 'Reddit', icon: 'fab fa-reddit', color: '#FF4500' },     
    { name: 'Twitch', icon: 'fab fa-twitch', color: '#9146FF' },       
    { name: 'Snapchat', icon: 'fab fa-snapchat', color: '#FFFC00' }     
  ];

  constructor(
    private socialLinkService: SocialLinkService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initSocialLinkForm();
    this.loadSocialLinks();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      platform: [''],
      isVisible: [''],
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

  private initSocialLinkForm(): void {
    this.socialLinkForm = this.fb.group({
      platform: ['', Validators.required],
      displayName: ['', Validators.required],
      url: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      iconClass: [''],
      color: ['#4ECDC4'],
      isVisible: [true],
      displayOrder: [0, [Validators.required, Validators.min(0)]]
    });

    // Auto-populate fields when platform is selected
    this.socialLinkForm.get('platform')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(platform => {
        const popularPlatform = this.popularPlatforms.find(p => p.name === platform);
        if (popularPlatform && !this.isEditMode) {
          this.socialLinkForm.patchValue({
            displayName: platform,
            iconClass: popularPlatform.icon,
            color: popularPlatform.color
          }, { emitEvent: false });
        }
      });
  }

  loadSocialLinks(): void {
    this.isLoading = true;

    this.socialLinkService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.socialLinks = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => {
          console.error('Failed to load social links:', error);
        }
      });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    // Normalize isVisible properly
    let isVisibleValue: boolean | undefined;
    if (formValue.isVisible === true || formValue.isVisible === 'true') {
      isVisibleValue = true;
    } else if (formValue.isVisible === false || formValue.isVisible === 'false') {
      isVisibleValue = false;
    } else {
      isVisibleValue = undefined; // All Status
    }

    this.searchParams = {
      ...this.searchParams,
      platform: formValue.platform?.trim() || undefined,
      isVisible: isVisibleValue,
      sortBy: formValue.sortBy || 'displayOrder',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadSocialLinks();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.platform ||
      formValue.isVisible !== ''
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      platform: '',
      isVisible: '',
      sortBy: 'displayOrder',
      desc: false
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadSocialLinks();
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
    this.currentSocialLinkId = null;
    this.formError = null;
    this.socialLinkForm.reset({
      isVisible: true,
      displayOrder: this.socialLinks.length,
      color: '#4ECDC4'
    });
    this.showModal = true;
  }

  openEditModal(socialLink: SocialLinkResponse): void {
    this.isEditMode = true;
    this.currentSocialLinkId = socialLink.id;
    this.formError = null;

    this.socialLinkForm.patchValue({
      platform: socialLink.platform,
      displayName: socialLink.displayName,
      url: socialLink.url,
      iconClass: socialLink.iconClass,
      color: socialLink.color || '#4ECDC4',
      isVisible: socialLink.isVisible,
      displayOrder: socialLink.displayOrder
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.socialLinkForm.reset();
    this.currentSocialLinkId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.socialLinkForm.invalid) {
      this.socialLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.socialLinkForm.value;

    const payload = {
      ...formValue,
      iconClass: formValue.iconClass || undefined,
      color: formValue.color || undefined
    };

    const request$ = this.isEditMode && this.currentSocialLinkId
      ? this.socialLinkService.update(this.currentSocialLinkId, payload as SocialLinkUpdateRequest)
      : this.socialLinkService.create(payload as SocialLinkInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadSocialLinks();
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.socialLinkForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(socialLink: SocialLinkResponse): void {
    this.socialLinkToDelete = socialLink;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.socialLinkToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.socialLinkToDelete) return;

    this.isDeleting = true;

    this.socialLinkService.delete(this.socialLinkToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadSocialLinks();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  selectPlatform(platform: { name: string; icon: string; color: string }): void {
    this.socialLinkForm.patchValue({
      platform: platform.name,
      displayName: platform.name,
      iconClass: platform.icon,
      color: platform.color
    });
  }

  formatDate(dateString: Date): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  togglePlatformList(): void {
  this.showAllPlatforms = !this.showAllPlatforms;
}

}