import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { ExperienceResponse } from '../../../../models/experience/experience-response.model';
import { ParsedExperience } from '../../../../models/experience/parsed-experience.model';
import { ExperienceService } from '../../../../services/experience.service';
import { MediaService } from '../../../../services/media.service';
import { MediaResponse } from '../../../../models/media/media-response.model';
import { ExperienceSearchObject } from '../../../../models/experience/experience-search.model';
import { ExperienceInsertRequest } from '../../../../models/experience/experience-insert-request.model';
import { ExperienceUpdateRequest } from '../../../../models/experience/experience-update-request.model';
import { EmploymentType } from '../../../../models/enums/employment-type.enum';
import { TechResponse } from '../../../../models/tech/tech-response.model';
import { TechService } from '../../../../services/tech.service';

interface ExperienceWithLogo extends ParsedExperience {
    logoUrl?: string;
}

@Component({
    selector: 'app-experience-crud',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './experience-CRUD.component.html',
    styleUrls: ['./experience-CRUD.component.scss']
})
export class ExperienceCrudComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;
    experienceForm!: FormGroup;

    private destroy$ = new Subject<void>();

    experiences: ExperienceWithLogo[] = [];
    totalCount: number | undefined;
    currentPage = 0;

    searchParams: ExperienceSearchObject = {
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

    currentExperienceId: string | null = null;
    experienceToDelete: ExperienceResponse | null = null;

    // Employment Type options
    employmentTypes = Object.values(EmploymentType);

    // Media selector
    showMediaSelectorModal = false;
    mediaList: MediaResponse[] = [];
    isLoadingMedia = false;
    selectedMediaId: string | null = null;
    mediaSearchQuery = '';
    filteredMediaList: MediaResponse[] = [];

    // Dynamic form arrays for achievements and technologies
    achievementInput = '';
    technologyInput = '';
    achievements: string[] = [];
    technologies: string[] = [];

    techList: TechResponse[] = [];
    isLoadingTechs = false;
    showTechSelectorModal = false;
    selectedTechIds: string[] = [];
    techSearchQuery = '';
    filteredTechList: TechResponse[] = [];

    constructor(
        private experienceService: ExperienceService,
        private mediaService: MediaService,
        private techService: TechService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.initExperienceForm();
        this.loadExperiences();
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
            companyName: [''],
            position: [''],
            employmentType: [''],
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

    private initExperienceForm(): void {
        this.experienceForm = this.fb.group({
            companyName: ['', Validators.required],
            position: ['', Validators.required],
            location: ['', Validators.required],
            employmentType: ['', Validators.required],
            startDate: ['', Validators.required],
            endDate: [''],
            isCurrent: [false],
            description: ['', Validators.required],
            displayOrder: [0, [Validators.required, Validators.min(0)]],
            companyLogo: ['']
        });

        this.experienceForm.get('isCurrent')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(isCurrent => {
                if (isCurrent) {
                    this.experienceForm.patchValue({ endDate: null }, { emitEvent: false });
                }
            });
    }

    loadTechList(): void {
        this.isLoadingTechs = true;
        this.filteredTechList = [];

        this.techService.get({
            page: 0,
            pageSize: 200,
            sortBy: 'name',
            desc: false
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoadingTechs = false)
            )
            .subscribe({
                next: (result) => {
                    this.techList = result.items;
                    this.filteredTechList = result.items;
                },
                error: (error) => {
                    console.error('Failed to load techs:', error);
                    this.techList = [];
                    this.filteredTechList = [];
                }
            });
    }

    openTechSelector(): void {
        this.showTechSelectorModal = true;
        this.selectedTechIds = [...this.technologies]; // Assuming technologies array stores tech IDs
        this.techSearchQuery = '';
        this.loadTechList();
    }

    closeTechSelector(): void {
        this.showTechSelectorModal = false;
        this.techSearchQuery = '';
    }

    filterTechs(): void {
        if (!this.techSearchQuery.trim()) {
            this.filteredTechList = [...this.techList];
            return;
        }

        const query = this.techSearchQuery.toLowerCase().trim();
        this.filteredTechList = this.techList.filter(tech =>
            tech.name?.toLowerCase().includes(query) ||
            tech.slug?.toLowerCase().includes(query) ||
            tech.category?.toLowerCase().includes(query)
        );
    }

    toggleTechSelection(techId: string): void {
        const index = this.selectedTechIds.indexOf(techId);
        if (index > -1) {
            this.selectedTechIds.splice(index, 1);
        } else {
            this.selectedTechIds.push(techId);
        }
    }

    isTechSelected(techId: string): boolean {
        return this.selectedTechIds.includes(techId);
    }

    confirmTechSelection(): void {
        this.technologies = [...this.selectedTechIds];
        this.closeTechSelector();
    }

    loadExperiences(): void {
        this.isLoading = true;

        this.experienceService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.experiences = result.items.map(exp => this.parseExperience(exp));
                    this.totalCount = result.totalCount;
                    this.loadLogos();
                },
                error: (error) => {
                    console.error('Failed to load experiences:', error);
                }
            });
    }

    getTechNameById(techId: string): string {
        const tech = this.techList.find(t => t.id === techId);
        return tech?.name || 'Unknown Tech';
    }

    getTechIconUrl(mediaId: string): string {

        return '';
    }

    private parseExperience(exp: ExperienceResponse): ExperienceWithLogo {
        return {
            ...exp,
            achievementsList: this.parseJsonArray(exp.achievements),
            technologiesList: this.parseJsonArray(exp.technologies)
        };
    }

    private parseJsonArray(jsonStr: string): string[] {
        try {
            const parsed = JSON.parse(jsonStr);
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    }

    private loadLogos(): void {
        this.experiences.forEach(experience => {
            if (experience.companyLogo) {
                this.mediaService.getById(experience.companyLogo)
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                        next: (media: MediaResponse) => {
                            experience.logoUrl = media.fileUrl;
                        },
                        error: (error) => {
                            console.error(`Failed to load logo for ${experience.companyName}:`, error);
                        }
                    });
            }
        });
    }

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

        let isCurrentValue: boolean | undefined;
        if (formValue.isCurrent === true || formValue.isCurrent === 'true') {
            isCurrentValue = true;
        } else if (formValue.isCurrent === false || formValue.isCurrent === 'false') {
            isCurrentValue = false;
        } else {
            isCurrentValue = undefined;
        }

        this.searchParams = {
            ...this.searchParams,
            companyName: formValue.companyName?.trim() || undefined,
            position: formValue.position?.trim() || undefined,
            employmentType: formValue.employmentType || undefined,
            isCurrent: isCurrentValue,
            sortBy: formValue.sortBy || 'displayOrder',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadExperiences();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(
            formValue.companyName ||
            formValue.position ||
            formValue.employmentType ||
            formValue.isCurrent !== ''
        );
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            companyName: '',
            position: '',
            employmentType: '',
            isCurrent: '',
            sortBy: 'displayOrder',
            desc: false
        });
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadExperiences();
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
        this.currentExperienceId = null;
        this.formError = null;
        this.achievements = [];
        this.technologies = [];
        this.achievementInput = '';
        this.technologyInput = '';
        this.experienceForm.reset({
            isCurrent: false,
            displayOrder: this.experiences.length
        });
        this.showModal = true;
    }

    openEditModal(experience: ExperienceWithLogo): void {
        this.isEditMode = true;
        this.currentExperienceId = experience.id;
        this.formError = null;
        this.achievements = [...experience.achievementsList];
        this.technologies = [...experience.technologiesList];
        this.achievementInput = '';
        this.technologyInput = '';

        this.experienceForm.patchValue({
            companyName: experience.companyName,
            position: experience.position,
            location: experience.location,
            employmentType: experience.employmentType,
            startDate: this.formatDateForInput(experience.startDate),
            endDate: experience.endDate ? this.formatDateForInput(experience.endDate) : null,
            isCurrent: experience.isCurrent,
            description: experience.description,
            displayOrder: experience.displayOrder,
            companyLogo: experience.companyLogo
        });

        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.experienceForm.reset();
        this.currentExperienceId = null;
        this.formError = null;
        this.achievements = [];
        this.technologies = [];
        this.achievementInput = '';
        this.technologyInput = '';
    }

    submitForm(): void {
    if (this.experienceForm.invalid) {
        this.experienceForm.markAllAsTouched();
        return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.experienceForm.value;
    
    // Map string enum to numeric value
    const employmentTypeMap: { [key: string]: number } = {
        'FullTime': 0,
        'PartTime': 1,
        'Contract': 2,
        'Internship': 3,
        'Freelance': 4,
        'Temporary': 5
    };

    const payload = {
        ...formValue,
        endDate: formValue.endDate || undefined,
        companyLogo: formValue.companyLogo || undefined,
        employmentType: employmentTypeMap[formValue.employmentType] ?? formValue.employmentType,
        achievements: JSON.stringify(this.achievements),
        technologies: JSON.stringify(this.technologies)
    };

    const request$ = this.isEditMode && this.currentExperienceId
        ? this.experienceService.update(this.currentExperienceId, payload as ExperienceUpdateRequest)
        : this.experienceService.create(payload as ExperienceInsertRequest);

    request$
        .pipe(
            takeUntil(this.destroy$),
            finalize(() => this.isSaving = false)
        )
        .subscribe({
            next: () => {
                this.closeModal();
                this.loadExperiences();
            },
            error: (error) => {
                this.formError = error.message || 'An error occurred while saving';
                console.error('Save failed:', error);
            }
        });
}

    isFieldInvalid(fieldName: string): boolean {
        const field = this.experienceForm.get(fieldName);
        return !!(field && field.invalid && field.touched);
    }

    confirmDelete(experience: ExperienceResponse): void {
        this.experienceToDelete = experience;
        this.showDeleteModal = true;
    }

    cancelDelete(): void {
        this.experienceToDelete = null;
        this.showDeleteModal = false;
    }

    executeDelete(): void {
        if (!this.experienceToDelete) return;

        this.isDeleting = true;

        this.experienceService.delete(this.experienceToDelete.id)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isDeleting = false)
            )
            .subscribe({
                next: () => {
                    this.cancelDelete();
                    this.loadExperiences();
                },
                error: (error) => {
                    console.error('Delete failed:', error);
                }
            });
    }

    // Achievement management
    addAchievement(): void {
        const achievement = this.achievementInput.trim();
        if (achievement && !this.achievements.includes(achievement)) {
            this.achievements.push(achievement);
            this.achievementInput = '';
        }
    }

    removeAchievement(index: number): void {
        this.achievements.splice(index, 1);
    }

    // Technology management
    addTechnology(): void {
        const technology = this.technologyInput.trim();
        if (technology && !this.technologies.includes(technology)) {
            this.technologies.push(technology);
            this.technologyInput = '';
        }
    }

    removeTechnology(index: number): void {
        this.technologies.splice(index, 1);
    }

    // Media selector
    openMediaSelector(): void {
        this.showMediaSelectorModal = true;
        this.selectedMediaId = this.experienceForm.get('companyLogo')?.value || null;
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
            this.experienceForm.patchValue({ companyLogo: this.selectedMediaId });
        }
        this.closeMediaSelector();
    }

    clearMediaSelection(): void {
        this.experienceForm.patchValue({ companyLogo: null });
        this.selectedMediaId = null;
    }

    getSelectedMediaPreview(): MediaResponse | null {
        const mediaId = this.experienceForm.get('companyLogo')?.value;
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

    calculateDuration(startDate: string, endDate?: string, isCurrent?: boolean): string {
        const start = new Date(startDate);
        const end = isCurrent ? new Date() : (endDate ? new Date(endDate) : new Date());

        const months = (end.getFullYear() - start.getFullYear()) * 12 + (end.getMonth() - start.getMonth());
        const years = Math.floor(months / 12);
        const remainingMonths = months % 12;

        if (years === 0) {
            return `${remainingMonths} month${remainingMonths !== 1 ? 's' : ''}`;
        } else if (remainingMonths === 0) {
            return `${years} year${years !== 1 ? 's' : ''}`;
        } else {
            return `${years} year${years !== 1 ? 's' : ''}, ${remainingMonths} month${remainingMonths !== 1 ? 's' : ''}`;
        }
    }

    formatEmploymentType(type: EmploymentType | string | number): string {
  // Handle numeric enum values from API
  if (typeof type === 'number') {
    // Map numeric values to enum keys
    const enumKeys = Object.keys(EmploymentType).filter(key => isNaN(Number(key)));
    const enumKey = enumKeys[type];
    if (enumKey) {
      type = EmploymentType[enumKey as keyof typeof EmploymentType];
    } else {
      return 'Unknown Type';
    }
  }
  
  // Now type should be a string, format it for display
  if (typeof type === 'string') {
    return type
      .replace(/([A-Z])/g, ' $1')
      .replace(/\b\w/g, l => l.toUpperCase())
      .trim();
  }
  
  return 'Unknown Type';
}
}