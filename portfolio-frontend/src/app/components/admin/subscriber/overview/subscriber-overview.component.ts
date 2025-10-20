import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, finalize } from 'rxjs';
import { SubscriberService } from '../../../../services/subscriber.service';
import { RouterModule } from '@angular/router';

interface SubscriberSummary {
  totalSubscribers: number;
  newThisWeek: number;
  newThisMonth: number;
  growthPercentage: number;
  activeSubscribers: number;
  inactiveSubscribers: number;
  topSources: { source: string; count: number }[];
  weeklyTrend: { day: string; count: number }[];
}

@Component({
  selector: 'app-subscribers-overview',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './subscriber-overview.component.html',
  styleUrls: ['./subscriber-overview.component.scss']
})
export class SubscribersOverviewComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  summary: SubscriberSummary = {
    totalSubscribers: 0,
    newThisWeek: 0,
    newThisMonth: 0,
    growthPercentage: 0,
    activeSubscribers: 0,
    inactiveSubscribers: 0,
    topSources: [],
    weeklyTrend: []
  };
  
  isLoading = false;
  error: string | null = null;

  constructor(private subscriberService: SubscriberService) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSummary(): void {
    this.isLoading = true;
    this.error = null;

    // Fetch all subscribers to calculate summary
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
          this.calculateSummary(result.items);
        },
        error: (error) => {
          console.error('Failed to load summary:', error);
          this.error = 'Failed to load subscriber summary. Please try again.';
        }
      });
  }

  private calculateSummary(subscribers: any[]): void {
    const now = new Date();
    const oneWeekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    const oneMonthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
    const twoMonthsAgo = new Date(now.getTime() - 60 * 24 * 60 * 60 * 1000);

    // Total subscribers
    this.summary.totalSubscribers = subscribers.length;

    // Active/Inactive
    this.summary.activeSubscribers = subscribers.filter(s => s.isActive).length;
    this.summary.inactiveSubscribers = subscribers.filter(s => !s.isActive).length;

    // New this week/month
    this.summary.newThisWeek = subscribers.filter(s => 
      new Date(s.subscribedAt) >= oneWeekAgo
    ).length;
    
    this.summary.newThisMonth = subscribers.filter(s => 
      new Date(s.subscribedAt) >= oneMonthAgo
    ).length;

    // Growth percentage (compare last month vs previous month)
    const lastMonthCount = subscribers.filter(s => 
      new Date(s.subscribedAt) >= oneMonthAgo
    ).length;
    
    const previousMonthCount = subscribers.filter(s => {
      const subDate = new Date(s.subscribedAt);
      return subDate >= twoMonthsAgo && subDate < oneMonthAgo;
    }).length;

    if (previousMonthCount > 0) {
      this.summary.growthPercentage = 
        ((lastMonthCount - previousMonthCount) / previousMonthCount) * 100;
    } else {
      this.summary.growthPercentage = lastMonthCount > 0 ? 100 : 0;
    }

    // Top sources
const sourceCounts = subscribers.reduce((acc, s) => {
  const source = s.source || 'Direct';
  acc[source] = (acc[source] || 0) + 1;
  return acc;
}, {} as Record<string, number>);

const entries = Object.entries(sourceCounts) as [string, number][];
this.summary.topSources = entries
  .map(([source, count]) => ({ source, count }))
  .sort((a, b) => b.count - a.count)
  .slice(0, 5);


    // Weekly trend (last 7 days)
    const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    this.summary.weeklyTrend = [];
    
    for (let i = 6; i >= 0; i--) {
      const date = new Date(now.getTime() - i * 24 * 60 * 60 * 1000);
      const dayName = days[date.getDay()];
      const count = subscribers.filter(s => {
        const subDate = new Date(s.subscribedAt);
        return subDate.toDateString() === date.toDateString();
      }).length;
      
      this.summary.weeklyTrend.push({ day: dayName, count });
    }
  }

  retryLoad(): void {
    this.loadSummary();
  }

  getMaxTrendValue(): number {
    return Math.max(...this.summary.weeklyTrend.map(t => t.count), 1);
  }
}