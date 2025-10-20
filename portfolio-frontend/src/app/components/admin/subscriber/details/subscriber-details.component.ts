import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, finalize } from 'rxjs';
import { SubscriberResponse } from '../../../../models/subscriber/subscriber-response.model';
import { SubscriberService } from '../../../../services/subscriber.service';

@Component({
  selector: 'app-subscriber-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscriber-details.component.html',
  styleUrls: ['./subscriber-details.component.scss']
})
export class SubscriberDetailsComponent implements OnInit, OnDestroy, OnChanges {
  @Input() subscriberId!: string;
  @Output() close = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  subscriber: SubscriberResponse | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(private subscriberService: SubscriberService) {}

  ngOnInit(): void {
    if (this.subscriberId) {
      this.loadSubscriberDetails();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['subscriberId'] && !changes['subscriberId'].firstChange) {
      this.loadSubscriberDetails();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSubscriberDetails(): void {
    if (!this.subscriberId) return;

    this.isLoading = true;
    this.error = null;

    this.subscriberService.getById(this.subscriberId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (subscriber) => {
          this.subscriber = subscriber;
        },
        error: (error) => {
          console.error('Failed to load subscriber details:', error);
          this.error = 'Failed to load subscriber details. Please try again.';
        }
      });
  }

  closePanel(): void {
    this.close.emit();
  }

  retryLoad(): void {
    this.loadSubscriberDetails();
  }

  formatDate(dateString: string | Date | undefined): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatShortDate(dateString: string | Date | undefined): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getSubscriptionDuration(): string {
    if (!this.subscriber) return 'N/A';

    const startDate = new Date(this.subscriber.subscribedAt);
    const endDate = this.subscriber.unsubscribedAt 
      ? new Date(this.subscriber.unsubscribedAt) 
      : new Date();

    const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 30) {
      return `${diffDays} day${diffDays !== 1 ? 's' : ''}`;
    } else if (diffDays < 365) {
      const months = Math.floor(diffDays / 30);
      return `${months} month${months !== 1 ? 's' : ''}`;
    } else {
      const years = Math.floor(diffDays / 365);
      const remainingMonths = Math.floor((diffDays % 365) / 30);
      return `${years} year${years !== 1 ? 's' : ''}${remainingMonths > 0 ? `, ${remainingMonths} month${remainingMonths !== 1 ? 's' : ''}` : ''}`;
    }
  }
}