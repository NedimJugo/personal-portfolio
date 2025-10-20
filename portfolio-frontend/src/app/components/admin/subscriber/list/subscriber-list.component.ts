import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { SubscriberResponse } from '../../../../models/subscriber/subscriber-response.model';
import { SubscriberService } from '../../../../services/subscriber.service';
import { SubscriberSearchObject } from '../../../../models/subscriber/subscriber-search.model';
import { SubscriberDetailsComponent } from '../details/subscriber-details.component';

@Component({
    selector: 'app-subscribers-list',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule, SubscriberDetailsComponent],
    templateUrl: './subscriber-list.component.html',
    styleUrls: ['./subscriber-list.component.scss']
})
export class SubscribersListComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;

    private destroy$ = new Subject<void>();

    subscribers: SubscriberResponse[] = [];
    totalCount: number | undefined;
    currentPage = 0;
    selectedSubscriberId: string | null = null;
    showDetailsPanel = false;

    searchParams: SubscriberSearchObject = {
        page: 0,
        pageSize: 20,
        sortBy: 'subscribedAt',
        desc: true,
        includeTotalCount: true,
        retrieveAll: false
    };

    isLoading = false;
    actionInProgress: { [key: string]: boolean } = {};

    constructor(
        private subscriberService: SubscriberService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.loadSubscribers();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initSearchForm(): void {
        this.searchForm = this.fb.group({
            email: [''],
            isActive: [''],
            sortBy: ['subscribedAt'],
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

    loadSubscribers(): void {
        this.isLoading = true;

        this.subscriberService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.subscribers = result.items;
                    this.totalCount = result.totalCount;
                },
                error: (error) => {
                    console.error('Failed to load subscribers:', error);
                }
            });
    }

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

        let isActiveValue: boolean | undefined;
        if (formValue.isActive === true || formValue.isActive === 'true') {
            isActiveValue = true;
        } else if (formValue.isActive === false || formValue.isActive === 'false') {
            isActiveValue = false;
        } else {
            isActiveValue = undefined;
        }

        this.searchParams = {
            ...this.searchParams,
            email: formValue.email?.trim() || undefined,
            isActive: isActiveValue,
            sortBy: formValue.sortBy || 'subscribedAt',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadSubscribers();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(formValue.email || formValue.isActive !== '');
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            email: '',
            isActive: '',
            sortBy: 'subscribedAt',
            desc: true
        });
    }

    resendConfirmation(subscriber: SubscriberResponse): void {
        if (this.actionInProgress[subscriber.id]) return;

        this.actionInProgress[subscriber.id] = true;

        // Simulate API call - replace with actual service method when available
        setTimeout(() => {
            this.actionInProgress[subscriber.id] = false;
            alert(`Confirmation email resent to ${subscriber.email}`);
        }, 1500);
    }

    deactivateSubscriber(subscriber: SubscriberResponse): void {
        if (this.actionInProgress[subscriber.id]) return;

        if (!confirm(`Deactivate subscriber ${subscriber.email}?`)) {
            return;
        }

        this.actionInProgress[subscriber.id] = true;

        this.subscriberService.update(subscriber.id, {
            ...subscriber,
            isActive: false,
            unsubscribedAt: new Date()
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => {
                    this.actionInProgress[subscriber.id] = false;
                })
            )
            .subscribe({
                next: () => {
                    this.loadSubscribers();
                },
                error: (error) => {
                    console.error('Failed to deactivate subscriber:', error);
                    alert('Failed to deactivate subscriber. Please try again.');
                }
            });
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadSubscribers();
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

    formatDate(dateString: string | Date | undefined): string {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    goBackToOverview(): void {
        window.history.back();

    }

    openDetails(subscriber: SubscriberResponse): void {
        this.selectedSubscriberId = subscriber.id;
        this.showDetailsPanel = true;
    }

    closeDetails(): void {
        this.showDetailsPanel = false;
        this.selectedSubscriberId = null;
    }
}