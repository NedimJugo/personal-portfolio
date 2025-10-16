import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip, forkJoin } from 'rxjs';
import { BlogPostResponse } from '../../../../models/blog-post/blog-post-response.model';
import { BlogPostService } from '../../../../services/blog-post.service';
import { MediaService } from '../../../../services/media.service';
import { ProjectService } from '../../../../services/project.service';
import { TagService } from '../../../../services/tag.service';
import { TechService } from '../../../../services/tech.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { ProjectResponse } from '../../../../models/project/project-response.model';
import { TagResponse } from '../../../../models/tag/tag-response.model';
import { TechResponse } from '../../../../models/tech/tech-response.model';
import { BlogPostSearchObject } from '../../../../models/blog-post/blog-post-search.model';
import { BlogPostInsertRequest } from '../../../../models/blog-post/blog-post-insert-request.model';
import { BlogPostUpdateRequest } from '../../../../models/blog-post/blog-post-update-request.model';
import { BlogPostStatus } from '../../../../models/enums/blog-post-status.enum';

interface BlogPostWithImage extends BlogPostResponse {
  featuredImageUrl?: string;
  parsedTags?: string[];
  parsedCategories?: string[];
}

interface TagTechItem {
  id: string;
  name: string;
  type: 'tag' | 'tech';
}

@Component({
  selector: 'app-blog-post-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './blog-post-CRUD.component.html',
  styleUrls: ['./blog-post-CRUD.component.scss']
})
export class BlogPostCrudComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  blogPostForm!: FormGroup;

  private destroy$ = new Subject<void>();

  blogPosts: BlogPostWithImage[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: BlogPostSearchObject = {
    page: 0,
    pageSize: 20,
    sortBy: 'createdAt',
    desc: true,
    includeTotalCount: true,
    retrieveAll: false
  };

  // Projects, Tags, Techs
  projects: ProjectResponse[] = [];
  allTagTechItems: TagTechItem[] = [];
  selectedTagTechIds: string[] = [];
  
  // Mock categories (hardcoded for now)
  mockCategories = [
    'Backend',
    'Frontend',
    'Web Development',
    'Mobile Development',
    'DevOps',
    'Cloud',
    'Database',
    'AI/ML',
    'Security',
    'Performance',
    'Testing',
    'Tutorial',
    'Best Practices'
  ];
  selectedCategories: string[] = [];

  // Status options
  BlogPostStatus = BlogPostStatus;
  statusOptions = [
  { key: 'Draft', value: 0 },
  { key: 'Published', value: 1 },
  { key: 'Archived', value: 2 }
];

  isLoading = false;
  isSaving = false;
  isDeleting = false;
  showModal = false;
  showDeleteModal = false;
  isEditMode = false;
  formError: string | null = null;

  currentBlogPostId: string | null = null;
  blogPostToDelete: BlogPostResponse | null = null;

  // Media selector
  showMediaSelectorModal = false;
  mediaList: MediaResponse[] = [];
  isLoadingMedia = false;
  selectedMediaId: string | null = null;
  mediaSearchQuery = '';
  filteredMediaList: MediaResponse[] = [];

  // Tag search
  tagTechSearchQuery = '';
  filteredTagTechItems: TagTechItem[] = [];

  constructor(
    private blogPostService: BlogPostService,
    private mediaService: MediaService,
    private projectService: ProjectService,
    private tagService: TagService,
    private techService: TechService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initBlogPostForm();
    this.loadProjects();
     this.loadTagsAndTechs(() => {
    this.loadBlogPosts();
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
      sortBy: ['createdAt'],
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

  private initBlogPostForm(): void {
    this.blogPostForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      slug: ['', [Validators.required, Validators.maxLength(250)]],
      excerpt: ['', [Validators.required, Validators.maxLength(500)]],
      content: ['', Validators.required],
      featuredImage: [''],
      status: [0, Validators.required],
      createdById: [1, Validators.required],
      projectId: ['']
    });

    // Auto-generate slug from title
    this.blogPostForm.get('title')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(title => {
        if (title && !this.isEditMode) {
          const slug = this.generateSlug(title);
          this.blogPostForm.patchValue({ slug }, { emitEvent: false });
        }
      });
  }

  private loadProjects(): void {
    this.projectService.get({ 
      pageSize: 100, 
      isPublished: true,
      sortBy: 'title'
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.projects = result.items;
        },
        error: (error) => {
          console.error('Failed to load projects:', error);
        }
      });
  }

  private loadTagsAndTechs(callback?: () => void): void {
  forkJoin({
    tags: this.tagService.get({ pageSize: 100, sortBy: 'name' }),
    techs: this.techService.get({ pageSize: 100, sortBy: 'name' })
  })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (result) => {
        const tagItems: TagTechItem[] = result.tags.items.map(tag => ({
          id: tag.id,
          name: tag.name,
          type: 'tag' as const
        }));
        
        const techItems: TagTechItem[] = result.techs.items.map(tech => ({
          id: tech.id,
          name: tech.name,
          type: 'tech' as const
        }));

        this.allTagTechItems = [...tagItems, ...techItems].sort((a, b) => 
          a.name.localeCompare(b.name)
        );
        this.filteredTagTechItems = [...this.allTagTechItems];
        
        // Call callback after data is loaded
        if (callback) {
          callback();
        }
      },
      error: (error) => {
        console.error('Failed to load tags and techs:', error);
      }
    });
}

  loadBlogPosts(): void {
  this.isLoading = true;

  this.blogPostService.get(this.searchParams)
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isLoading = false)
    )
    .subscribe({
      next: (result) => {
        this.blogPosts = result.items.map(post => ({
          ...post,
          parsedTags: this.mapIdsToNames(this.parseJsonArray(post.tags)), 
          parsedCategories: this.parseJsonArray(post.categories)
        }));
        this.totalCount = result.totalCount;
        this.loadFeaturedImages();
      },
      error: (error) => {
        console.error('Failed to load blog posts:', error);
      }
    });
}
private mapIdsToNames(ids: string[]): string[] {
  return ids
    .map(id => {
      const item = this.allTagTechItems.find(item => item.id === id);
      return item ? item.name : null;
    })
    .filter((name): name is string => name !== null);
}

  private loadFeaturedImages(): void {
    this.blogPosts.forEach(post => {
      if (post.featuredImage) {
        this.mediaService.getById(post.featuredImage)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (media: MediaResponse) => {
              post.featuredImageUrl = media.fileUrl;
            },
            error: (error) => {
              console.error(`Failed to load featured image for ${post.title}:`, error);
            }
          });
      }
    });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    this.searchParams = {
      ...this.searchParams,
      title: formValue.title?.trim() || undefined,
      status: formValue.status || undefined,
      sortBy: formValue.sortBy || 'createdAt',
      desc: formValue.desc !== false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadBlogPosts();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(formValue.title || formValue.status);
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      title: '',
      status: '',
      sortBy: 'createdAt',
      desc: true
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadBlogPosts();
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
    this.currentBlogPostId = null;
    this.formError = null;
    this.selectedTagTechIds = [];
    this.selectedCategories = [];
    
    this.blogPostForm.reset({
      status: 0,
      createdById: 1
    });
    
    this.showModal = true;
  }

  openEditModal(post: BlogPostResponse): void {
  this.isEditMode = true;
  this.currentBlogPostId = post.id;
  this.formError = null;

  // Parse tags and categories
  this.selectedTagTechIds = this.parseJsonArray(post.tags);
  this.selectedCategories = this.parseJsonArray(post.categories);

  this.blogPostForm.patchValue({
    title: post.title,
    slug: post.slug,
    excerpt: post.excerpt,
    content: post.content,
    featuredImage: post.featuredImage,
    status: Number(post.status), // <-- Ensure it's a number
    createdById: post.createdById,
    projectId: (post as any).projectId || ''
  });

  this.showModal = true;
}

  closeModal(): void {
    this.showModal = false;
    this.blogPostForm.reset();
    this.currentBlogPostId = null;
    this.formError = null;
    this.selectedTagTechIds = [];
    this.selectedCategories = [];
  }

 submitForm(): void {
  if (this.blogPostForm.invalid) {
    this.blogPostForm.markAllAsTouched();
    return;
  }

  this.isSaving = true;
  this.formError = null;

  const formValue = this.blogPostForm.value;

  const payload = {
    ...formValue,
    featuredImage: formValue.featuredImage || undefined,
    projectId: formValue.projectId || undefined,
    tags: JSON.stringify(this.selectedTagTechIds),
    categories: JSON.stringify(this.selectedCategories),
    status: Number(formValue.status) // <-- Force to number
  };

  const request$ = this.isEditMode && this.currentBlogPostId
    ? this.blogPostService.update(this.currentBlogPostId, payload as BlogPostUpdateRequest)
    : this.blogPostService.create(payload as BlogPostInsertRequest);

  request$
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isSaving = false)
    )
    .subscribe({
      next: () => {
        this.closeModal();
        this.loadBlogPosts();
      },
      error: (error) => {
        this.formError = error.message || 'An error occurred while saving';
        console.error('Save failed:', error);
      }
    });
}

  isFieldInvalid(fieldName: string): boolean {
    const field = this.blogPostForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(post: BlogPostResponse): void {
    this.blogPostToDelete = post;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.blogPostToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.blogPostToDelete) return;

    this.isDeleting = true;

    this.blogPostService.delete(this.blogPostToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadBlogPosts();
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  // Tag/Tech Selection Methods
  toggleTagTech(id: string): void {
    const index = this.selectedTagTechIds.indexOf(id);
    if (index > -1) {
      this.selectedTagTechIds.splice(index, 1);
    } else {
      this.selectedTagTechIds.push(id);
    }
  }

  isTagTechSelected(id: string): boolean {
    return this.selectedTagTechIds.includes(id);
  }

  filterTagTech(): void {
    if (!this.tagTechSearchQuery.trim()) {
      this.filteredTagTechItems = [...this.allTagTechItems];
      return;
    }

    const query = this.tagTechSearchQuery.toLowerCase().trim();
    this.filteredTagTechItems = this.allTagTechItems.filter(item =>
      item.name.toLowerCase().includes(query)
    );
  }

  getSelectedTagTechNames(): string[] {
    return this.selectedTagTechIds
      .map(id => this.allTagTechItems.find(item => item.id === id))
      .filter((item): item is TagTechItem => !!item)
      .map(item => item.name);
  }

  // Category Selection Methods
  toggleCategory(category: string): void {
    const index = this.selectedCategories.indexOf(category);
    if (index > -1) {
      this.selectedCategories.splice(index, 1);
    } else {
      this.selectedCategories.push(category);
    }
  }

  isCategorySelected(category: string): boolean {
    return this.selectedCategories.includes(category);
  }

  // Media Selector Methods
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
    this.selectedMediaId = this.blogPostForm.get('featuredImage')?.value || null;
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
      this.blogPostForm.patchValue({ featuredImage: this.selectedMediaId });
    }
    this.closeMediaSelector();
  }

  clearMediaSelection(): void {
    this.blogPostForm.patchValue({ featuredImage: null });
    this.selectedMediaId = null;
  }

  getSelectedMediaPreview(): MediaResponse | null {
    const mediaId = this.blogPostForm.get('featuredImage')?.value;
    if (!mediaId) return null;

    return this.mediaList.find(m => m.id === mediaId) ||
      this.filteredMediaList.find(m => m.id === mediaId) ||
      null;
  }

  // Utility Methods
  private generateSlug(title: string): string {
    return title
      .toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim();
  }

  private parseJsonArray(jsonString: string): string[] {
    try {
      const parsed = JSON.parse(jsonString);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getStatusBadgeClass(status: BlogPostStatus): string {
    switch (status) {
      case BlogPostStatus.Published:
        return 'badge-success';
      case BlogPostStatus.Draft:
        return 'badge-warning';
      case BlogPostStatus.Archived:
        return 'badge-outline';
      default:
        return 'badge-outline';
    }
  }

  getStatusLabel(status: BlogPostStatus): string {
    const option = this.statusOptions.find(opt => opt.value === status);
    return option ? option.key : 'Unknown';
  }

  getProjectName(projectId: string): string {
    const project = this.projects.find(p => p.id === projectId);
    return project ? project.title : 'Unknown Project';
  }

  truncateContent(content: string, maxLength: number = 150): string {
    if (content.length <= maxLength) return content;
    return content.substring(0, maxLength) + '...';
  }
}