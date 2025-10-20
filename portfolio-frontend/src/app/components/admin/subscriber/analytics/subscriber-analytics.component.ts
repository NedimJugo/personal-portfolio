import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, takeUntil, finalize } from 'rxjs';
import { SubscriberService } from '../../../../services/subscriber.service';
import { Router } from '@angular/router';

interface GrowthDataPoint {
  date: string;
  count: number;
}

interface SourceDistribution {
  source: string;
  count: number;
  percentage: number;
}

@Component({
  selector: 'app-subscribers-analytics',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './subscriber-analytics.component.html',
  styleUrls: ['./subscriber-analytics.component.scss']
})
export class SubscribersAnalyticsComponent implements OnInit, OnDestroy {
  filterForm!: FormGroup;
  private destroy$ = new Subject<void>();

  growthData: GrowthDataPoint[] = [];
  sourceDistribution: SourceDistribution[] = [];
  
  totalSubscribers = 0;
  periodGrowth = 0;
  averagePerDay = 0;
  
  isLoading = false;
  error: string | null = null;

  constructor(
    private subscriberService: SubscriberService,
    private fb: FormBuilder,
    private router: Router 
  ) {}

  ngOnInit(): void {
    this.initFilterForm();
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      dateRange: ['week'] // week, month, year
    });

    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadAnalytics();
      });
  }

  loadAnalytics(): void {
    this.isLoading = true;
    this.error = null;

    this.subscriberService.get({
      page: 0,
      pageSize: 1000,
      retrieveAll: true,
      includeTotalCount: true
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.calculateAnalytics(result.items);
        },
        error: (error) => {
          console.error('Failed to load analytics:', error);
          this.error = 'Failed to load analytics data. Please try again.';
        }
      });
  }

  private calculateAnalytics(subscribers: any[]): void {
    const dateRange = this.filterForm.get('dateRange')?.value;
    const now = new Date();
    let daysBack = 7;

    if (dateRange === 'month') daysBack = 30;
    else if (dateRange === 'year') daysBack = 365;

    const startDate = new Date(now.getTime() - daysBack * 24 * 60 * 60 * 1000);

    // Filter subscribers within date range
    const filteredSubscribers = subscribers.filter(s =>
      new Date(s.subscribedAt) >= startDate
    );

    this.totalSubscribers = filteredSubscribers.length;

    // Calculate period growth
    const midPoint = new Date(now.getTime() - (daysBack / 2) * 24 * 60 * 60 * 1000);
    const firstHalfCount = filteredSubscribers.filter(s =>
      new Date(s.subscribedAt) < midPoint
    ).length;
    const secondHalfCount = filteredSubscribers.filter(s =>
      new Date(s.subscribedAt) >= midPoint
    ).length;

    if (firstHalfCount > 0) {
      this.periodGrowth = ((secondHalfCount - firstHalfCount) / firstHalfCount) * 100;
    } else {
      this.periodGrowth = secondHalfCount > 0 ? 100 : 0;
    }

    // Average per day
    this.averagePerDay = this.totalSubscribers / daysBack;

    // Growth data
    this.growthData = this.calculateGrowthData(filteredSubscribers, daysBack);

    // Source distribution
    this.sourceDistribution = this.calculateSourceDistribution(subscribers);
  }

  private calculateGrowthData(subscribers: any[], daysBack: number): GrowthDataPoint[] {
    const now = new Date();
    const data: GrowthDataPoint[] = [];
    const grouping = daysBack > 60 ? 'week' : 'day';

    if (grouping === 'day') {
      for (let i = daysBack - 1; i >= 0; i--) {
        const date = new Date(now.getTime() - i * 24 * 60 * 60 * 1000);
        const dateStr = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
        const count = subscribers.filter(s => {
          const subDate = new Date(s.subscribedAt);
          return subDate.toDateString() === date.toDateString();
        }).length;

        data.push({ date: dateStr, count });
      }
    } else {
      // Weekly grouping
      const weeks = Math.ceil(daysBack / 7);
      for (let i = weeks - 1; i >= 0; i--) {
        const weekStart = new Date(now.getTime() - (i + 1) * 7 * 24 * 60 * 60 * 1000);
        const weekEnd = new Date(now.getTime() - i * 7 * 24 * 60 * 60 * 1000);
        const dateStr = `Week ${weeks - i}`;
        const count = subscribers.filter(s => {
          const subDate = new Date(s.subscribedAt);
          return subDate >= weekStart && subDate < weekEnd;
        }).length;

        data.push({ date: dateStr, count });
      }
    }

    return data;
  }

  private calculateSourceDistribution(subscribers: any[]): SourceDistribution[] {
  const sourceCounts = subscribers.reduce((acc, s) => {
    const source = s.source || 'Direct';
    acc[source] = (acc[source] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const total = subscribers.length;

  const entries = Object.entries(sourceCounts) as [string, number][]; // ✅ eksplicitno označimo tip

  return entries
    .map(([source, count]) => ({
      source,
      count,
      percentage: (count / total) * 100
    }))
    .sort((a, b) => b.count - a.count);
}


  getMaxGrowthValue(): number {
    return Math.max(...this.growthData.map(d => d.count), 1);
  }

  retryLoad(): void {
    this.loadAnalytics();
  }

  getSourceColor(index: number): string {
    const colors = [
      '#4ECDC4', // comic-blue
      '#FFD93D', // comic-yellow
      '#FF6B6B', // comic-red
      '#48BB78', // success-color
      '#A78BFA', // comic-purple
      '#ED8936', // warning-color
      '#4299E1', // info-color
    ];
    return colors[index % colors.length];
  }

      goBackToOverview(): void {
        window.history.back();

    }
}