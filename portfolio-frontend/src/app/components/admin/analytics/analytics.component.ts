// analytics-dashboard.component.ts
import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { Subject, forkJoin, finalize, takeUntil } from 'rxjs';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { ContactMessageResponse } from '../../../models/contact-message/contact-message-response.model';
import { BlogPostResponse } from '../../../models/blog-post/blog-post-response.model';
import { PageViewResponse } from '../../../models/page-view/page-view-response.model';
import { PageViewService } from '../../../services/page-view.service';
import { BlogPostService } from '../../../services/blog-post.service';
import { BlogPostLikeService } from '../../../services/blog-post-like.service';
import { ProjectService } from '../../../services/project.service';
import { ContactMessageService } from '../../../services/contact-message.service';
import { SubscriberService } from '../../../services/subscriber.service';
import { SubscriberResponse } from '../../../models/subscriber/subscriber-response.model';
import { ProjectResponse } from '../../../models/project/project-response.model';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

Chart.register(...registerables);

interface KPICard {
    title: string;
    value: number | string;
    change?: string;
    icon: string;
    color: string;
    isLoading: boolean;
}

interface DateRangePreset {
    label: string;
    days: number;
}

@Component({
    selector: 'app-analytics-dashboard',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './analytics.component.html',
    styleUrls: ['./analytics.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('trafficChart') trafficChartRef!: ElementRef<HTMLCanvasElement>;
    @ViewChild('topPagesChart') topPagesChartRef!: ElementRef<HTMLCanvasElement>;
    @ViewChild('blogEngagementChart') blogEngagementChartRef!: ElementRef<HTMLCanvasElement>;
    @ViewChild('subscribersChart') subscribersChartRef!: ElementRef<HTMLCanvasElement>;
    @ViewChild('projectsChart') projectsChartRef!: ElementRef<HTMLCanvasElement>;

    private destroy$ = new Subject<void>();
    private trafficChart?: Chart;
    private topPagesChart?: Chart;
    private blogEngagementChart?: Chart;
    private subscribersChart?: Chart;
    private projectsChart?: Chart;

    filterForm!: FormGroup;
    granularity: 'daily' | 'weekly' | 'monthly' = 'daily';

    datePresets: DateRangePreset[] = [
        { label: '7 Days', days: 7 },
        { label: '30 Days', days: 30 },
        { label: '90 Days', days: 90 }
    ];

    kpiCards: KPICard[] = [
        { title: 'Total Page Views', value: 0, icon: 'eye', color: '#4ECDC4', isLoading: true },
        { title: 'Unique Visitors', value: 0, icon: 'users', color: '#FF6B6B', isLoading: true },
        { title: 'New Subscribers', value: 0, change: '+12%', icon: 'mail', color: '#48BB78', isLoading: true },
        { title: 'Contact Messages', value: 0, icon: 'message', color: '#ED8936', isLoading: true },
        { title: 'Published Posts', value: 0, icon: 'file', color: '#A78BFA', isLoading: true },
        { title: 'Top Projects', value: 0, icon: 'star', color: '#FFD93D', isLoading: true }
    ];

    recentMessages: ContactMessageResponse[] = [];
    recentPosts: BlogPostResponse[] = [];

    isLoadingCharts = true;
    isLoadingMessages = true;
    isLoadingPosts = true;
    errorMessage: string | null = null;

    selectedMessage: ContactMessageResponse | null = null;
    showMessageModal = false;

    private allPageViews: PageViewResponse[] = [];

    constructor(
        private fb: FormBuilder,
        private pageViewService: PageViewService,
        private blogPostService: BlogPostService,
        private blogPostLikeService: BlogPostLikeService,
        private projectService: ProjectService,
        private contactMessageService: ContactMessageService,
        private subscriberService: SubscriberService
    ) { }

    ngOnInit(): void {
        this.initFilterForm();
        this.applyPreset(30);
    }

    ngOnDestroy(): void {
        this.destroyCharts();
        this.destroy$.next();
        this.destroy$.complete();
    }

    ngAfterViewInit(): void {
        // Initialize charts after view is ready
        setTimeout(() => {
            this.initCharts();
        }, 0);
    }

    private initFilterForm(): void {
        const today = new Date();
        const thirtyDaysAgo = new Date(today);
        thirtyDaysAgo.setDate(today.getDate() - 30);

        this.filterForm = this.fb.group({
            startDate: [this.formatDateForInput(thirtyDaysAgo)],
            endDate: [this.formatDateForInput(today)]
        });
    }

    applyPreset(days: number): void {
        const today = new Date();
        const startDate = new Date(today);
        startDate.setDate(today.getDate() - days);

        this.filterForm.patchValue({
            startDate: this.formatDateForInput(startDate),
            endDate: this.formatDateForInput(today)
        });

        this.applyFilters();
    }

    applyFilters(): void {
        this.errorMessage = null;
        this.loadAllData();
    }

    private loadAllData(): void {
        this.loadKPIData();
        this.loadChartData();
        this.loadRecentMessages();
        this.loadRecentPosts();
    }

    private loadKPIData(): void {
        this.kpiCards.forEach(card => card.isLoading = true);

        const { startDate, endDate } = this.getDateRange();

        forkJoin({
            pageViews: this.pageViewService.get({ page: 0, pageSize: 10000, retrieveAll: true }),
            subscribers: this.subscriberService.get({ page: 0, pageSize: 10000, retrieveAll: true }),
            messages: this.contactMessageService.get({ page: 0, pageSize: 10000, retrieveAll: true }),
            posts: this.blogPostService.get({ page: 0, pageSize: 1000, retrieveAll: true }),
            projects: this.projectService.get({ page: 0, pageSize: 1000, retrieveAll: true })
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => {
                    this.kpiCards.forEach(card => card.isLoading = false);
                })
            )
            .subscribe({
                next: (data) => {
                    const filteredPageViews = data.pageViews.items.filter(pv =>
                        this.isDateInRange(pv.viewedAt, startDate, endDate)
                    );
                    const filteredSubscribers = data.subscribers.items.filter(sub =>
                        sub.subscribedAt && this.isDateInRange(sub.subscribedAt.toString(), startDate, endDate)
                    );
                    const filteredMessages = data.messages.items.filter(msg =>
                        this.isDateInRange(msg.createdAt.toString(), startDate, endDate)
                    );

                    this.kpiCards[0].value = filteredPageViews.length;
                    this.kpiCards[1].value = new Set(filteredPageViews.map(pv => pv.visitorKey).filter(Boolean)).size || filteredPageViews.length;
                    this.kpiCards[2].value = filteredSubscribers.length;
                    this.kpiCards[3].value = filteredMessages.length;
                    this.kpiCards[4].value = data.posts.items.filter(p => p.publishedAt && this.isDateInRange(p.publishedAt, startDate, endDate)).length;
                    this.kpiCards[5].value = data.projects.items.filter(p => p.viewCount > 10).length;
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load KPI data: ' + error.message;
                    console.error('KPI load error:', error);
                }
            });
    }

    private loadChartData(): void {
        this.isLoadingCharts = true;

        const { startDate, endDate } = this.getDateRange();

        forkJoin({
            pageViews: this.pageViewService.get({ page: 0, pageSize: 10000, retrieveAll: true }),
            posts: this.blogPostService.get({ page: 0, pageSize: 1000, retrieveAll: true }),
            projects: this.projectService.get({ page: 0, pageSize: 1000, retrieveAll: true }),
            subscribers: this.subscriberService.get({ page: 0, pageSize: 10000, retrieveAll: true })
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => {
                    this.isLoadingCharts = false;
                })
            )
            .subscribe({
                next: (data) => {
                    this.allPageViews = data.pageViews.items.filter(pv =>
                        this.isDateInRange(pv.viewedAt, startDate, endDate)
                    );

                    // Use setTimeout to ensure ViewChild refs are available
                    setTimeout(() => {
                        this.updateTrafficChart(this.allPageViews);
                        this.updateTopPagesChart(this.allPageViews);
                        this.updateBlogEngagementChart(data.posts.items);
                        this.updateProjectsChart(data.projects.items);
                        this.updateSubscribersChart(data.subscribers.items);
                    }, 100);
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load chart data: ' + error.message;
                    console.error('Chart load error:', error);
                }
            });
    }

    private loadRecentMessages(): void {
        this.isLoadingMessages = true;

        this.contactMessageService.get({
            page: 0,
            pageSize: 10,
            sortBy: 'createdAt',
            desc: true
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => {
                    this.isLoadingMessages = false;
                })
            )
            .subscribe({
                next: (result) => {
                    this.recentMessages = result.items;
                },
                error: (error) => {
                    console.error('Failed to load messages:', error);
                }
            });
    }

    private loadRecentPosts(): void {
        this.isLoadingPosts = true;

        this.blogPostService.get({
            page: 0,
            pageSize: 5,
            sortBy: 'publishedAt',
            desc: true
        })
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => {
                    this.isLoadingPosts = false;
                })
            )
            .subscribe({
                next: (result) => {
                    this.recentPosts = result.items;
                },
                error: (error) => {
                    console.error('Failed to load posts:', error);
                }
            });
    }

    private initCharts(): void {
        this.createTrafficChart();
        this.createTopPagesChart();
        this.createBlogEngagementChart();
        this.createSubscribersChart();
        this.createProjectsChart();
    }

    private createTrafficChart(): void {
        if (!this.trafficChartRef?.nativeElement) return;

        const ctx = this.trafficChartRef.nativeElement.getContext('2d');
        if (!ctx) return;

        const config: ChartConfiguration = {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Page Views',
                    data: [],
                    borderColor: '#4ECDC4',
                    backgroundColor: 'rgba(78, 205, 196, 0.1)',
                    borderWidth: 3,
                    tension: 0.4,
                    fill: true,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#4ECDC4'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: { size: 13, weight: 'bold' },
                            color: '#2D3748'
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            color: '#E2E8F0'
                        }
                    },
                    x: {
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            display: false
                        }
                    }
                }
            }
        };

        this.trafficChart = new Chart(ctx, config);
    }

    private createTopPagesChart(): void {
        if (!this.topPagesChartRef?.nativeElement) return;

        const ctx = this.topPagesChartRef.nativeElement.getContext('2d');
        if (!ctx) return;

        const config: ChartConfiguration = {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Views',
                    data: [],
                    backgroundColor: '#FF6B6B',
                    borderColor: '#1A202C',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                indexAxis: 'y',
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            color: '#E2E8F0'
                        }
                    },
                    y: {
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            display: false
                        }
                    }
                }
            }
        };

        this.topPagesChart = new Chart(ctx, config);
    }

    private createBlogEngagementChart(): void {
        if (!this.blogEngagementChartRef?.nativeElement) return;

        const ctx = this.blogEngagementChartRef.nativeElement.getContext('2d');
        if (!ctx) return;

        const config: ChartConfiguration = {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Likes',
                    data: [],
                    backgroundColor: '#48BB78',
                    borderColor: '#1A202C',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: { size: 13, weight: 'bold' },
                            color: '#2D3748'
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            color: '#E2E8F0'
                        }
                    },
                    x: {
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            display: false
                        }
                    }
                }
            }
        };

        this.blogEngagementChart = new Chart(ctx, config);
    }

    private createSubscribersChart(): void {
        if (!this.subscribersChartRef?.nativeElement) return;

        const ctx = this.subscribersChartRef.nativeElement.getContext('2d');
        if (!ctx) return;

        const config: ChartConfiguration = {
            type: 'line',
            data: {
                labels: [],
                datasets: [
                    {
                        label: 'New Subscribers',
                        data: [],
                        borderColor: '#ED8936',
                        backgroundColor: 'rgba(237, 137, 54, 0.1)',
                        borderWidth: 3,
                        tension: 0.4,
                        fill: true,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        yAxisID: 'y'
                    },
                    {
                        label: 'Total Subscribers',
                        data: [],
                        borderColor: '#A78BFA',
                        backgroundColor: 'rgba(167, 139, 250, 0.1)',
                        borderWidth: 3,
                        tension: 0.4,
                        fill: true,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: { size: 13, weight: 'bold' },
                            color: '#2D3748'
                        }
                    }
                },
                scales: {
                    y: {
                        type: 'linear',
                        position: 'left',
                        beginAtZero: true,
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            color: '#E2E8F0'
                        }
                    },
                    y1: {
                        type: 'linear',
                        position: 'right',
                        beginAtZero: true,
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            display: false
                        }
                    },
                    x: {
                        ticks: {
                            font: { size: 12 },
                            color: '#718096'
                        },
                        grid: {
                            display: false
                        }
                    }
                }
            }
        };

        this.subscribersChart = new Chart(ctx, config);
    }

    private createProjectsChart(): void {
        if (!this.projectsChartRef?.nativeElement) return;

        const ctx = this.projectsChartRef.nativeElement.getContext('2d');
        if (!ctx) return;

        const config: ChartConfiguration = {
            type: 'doughnut',
            data: {
                labels: [],
                datasets: [{
                    data: [],
                    backgroundColor: [
                        '#4ECDC4',
                        '#FF6B6B',
                        '#48BB78',
                        '#ED8936',
                        '#A78BFA',
                        '#FFD93D'
                    ],
                    borderColor: '#1A202C',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'right',
                        labels: {
                            font: { size: 12, weight: 'bold' },
                            color: '#2D3748',
                            padding: 12
                        }
                    }
                }
            }
        };

        this.projectsChart = new Chart(ctx, config);
    }

    private updateTrafficChart(pageViews: PageViewResponse[]): void {
        if (!this.trafficChartRef?.nativeElement) return;

        // If chart exists, destroy it before recreating
        if (this.trafficChart) {
            this.trafficChart.destroy();
        }

        this.createTrafficChart();

        if (!this.trafficChart) {
            console.warn('Traffic chart could not be initialized');
            return;
        }

        const aggregated = this.aggregateByGranularity(pageViews);
        this.trafficChart.data.labels = aggregated.labels;
        this.trafficChart.data.datasets[0].data = aggregated.counts;
        this.trafficChart.update();
    }

    private updateTopPagesChart(pageViews: PageViewResponse[]): void {
        if (!this.topPagesChartRef?.nativeElement) return;

        // If chart exists, destroy it before recreating
        if (this.topPagesChart) {
            this.topPagesChart.destroy();
        }

        this.createTopPagesChart();

        if (!this.topPagesChart) {
            console.warn('Top pages chart could not be initialized');
            return;
        }

        const pathCounts = new Map<string, number>();
        pageViews.forEach(pv => {
            const path = pv.path || 'Unknown';
            pathCounts.set(path, (pathCounts.get(path) || 0) + 1);
        });

        const sorted = Array.from(pathCounts.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, 8);

        this.topPagesChart.data.labels = sorted.map(([path]) => path.length > 30 ? path.substring(0, 27) + '...' : path);
        this.topPagesChart.data.datasets[0].data = sorted.map(([, count]) => count);
        this.topPagesChart.update();
    }

    private updateBlogEngagementChart(posts: BlogPostResponse[]): void {
        if (!this.blogEngagementChartRef?.nativeElement) return;

        // If chart exists, destroy it before recreating
        if (this.blogEngagementChart) {
            this.blogEngagementChart.destroy();
        }

        this.createBlogEngagementChart();

        if (!this.blogEngagementChart) {
            console.warn('Blog engagement chart could not be initialized');
            return;
        }

        const sorted = posts
            .filter(p => p.likeCount > 0)
            .sort((a, b) => b.likeCount - a.likeCount)
            .slice(0, 6);

        this.blogEngagementChart.data.labels = sorted.map(p => p.title.length > 25 ? p.title.substring(0, 22) + '...' : p.title);
        this.blogEngagementChart.data.datasets[0].data = sorted.map(p => p.likeCount);
        this.blogEngagementChart.update();
    }

    private updateProjectsChart(projects: ProjectResponse[]): void {
        if (!this.projectsChartRef?.nativeElement) return;

        // If chart exists, destroy it before recreating
        if (this.projectsChart) {
            this.projectsChart.destroy();
        }

        this.createProjectsChart();

        if (!this.projectsChart) {
            console.warn('Projects chart could not be initialized');
            return;
        }

        const sorted = projects
            .sort((a, b) => b.viewCount - a.viewCount)
            .slice(0, 6);

        this.projectsChart.data.labels = sorted.map(p => p.title);
        this.projectsChart.data.datasets[0].data = sorted.map(p => p.viewCount);
        this.projectsChart.update();
    }

    private updateSubscribersChart(subscribers: SubscriberResponse[]): void {
        if (!this.subscribersChartRef?.nativeElement) return;

        // If chart exists, destroy it before recreating
        if (this.subscribersChart) {
            this.subscribersChart.destroy();
        }

        this.createSubscribersChart();

        if (!this.subscribersChart) {
            console.warn('Subscribers chart could not be initialized');
            return;
        }

        const { startDate, endDate } = this.getDateRange();
        const filtered = subscribers.filter(sub =>
            sub.subscribedAt && this.isDateInRange(sub.subscribedAt.toString(), startDate, endDate)
        );

        const dailyNew = new Map<string, number>();
        filtered.forEach(sub => {
            if (!sub.subscribedAt) return;
            const date = new Date(sub.subscribedAt);
            const key = date.toISOString().split('T')[0];
            dailyNew.set(key, (dailyNew.get(key) || 0) + 1);
        });

        const dateRange = this.generateDateRange(startDate, endDate);
        const newCounts: number[] = [];
        const cumulativeCounts: number[] = [];
        let cumulative = 0;

        dateRange.forEach(date => {
            const count = dailyNew.get(date) || 0;
            newCounts.push(count);
            cumulative += count;
            cumulativeCounts.push(cumulative);
        });

        this.subscribersChart.data.labels = dateRange;
        this.subscribersChart.data.datasets[0].data = newCounts;
        this.subscribersChart.data.datasets[1].data = cumulativeCounts;
        this.subscribersChart.update();
    }

    private aggregateByGranularity(pageViews: PageViewResponse[]): { labels: string[]; counts: number[] } {
        const grouped = new Map<string, number>();

        pageViews.forEach(pv => {
            const date = new Date(pv.viewedAt);
            let key: string;

            switch (this.granularity) {
                case 'weekly':
                    const weekStart = new Date(date);
                    weekStart.setDate(date.getDate() - date.getDay());
                    key = weekStart.toISOString().split('T')[0];
                    break;
                case 'monthly':
                    key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
                    break;
                default:
                    key = date.toISOString().split('T')[0];
            }

            grouped.set(key, (grouped.get(key) || 0) + 1);
        });

        const sorted = Array.from(grouped.entries()).sort((a, b) => a[0].localeCompare(b[0]));

        return {
            labels: sorted.map(([date]) => this.formatChartLabel(date)),
            counts: sorted.map(([, count]) => count)
        };
    }

    private formatChartLabel(dateStr: string): string {
        const date = new Date(dateStr);
        switch (this.granularity) {
            case 'monthly':
                return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
            case 'weekly':
                return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
            default:
                return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
        }
    }

    private generateDateRange(start: Date, end: Date): string[] {
        const dates: string[] = [];
        const current = new Date(start);

        while (current <= end) {
            dates.push(current.toISOString().split('T')[0]);
            current.setDate(current.getDate() + 1);
        }

        return dates;
    }

    private getDateRange(): { startDate: Date; endDate: Date } {
        const formValue = this.filterForm.value;
        return {
            startDate: new Date(formValue.startDate),
            endDate: new Date(formValue.endDate)
        };
    }

    private isDateInRange(dateStr: string, start: Date, end: Date): boolean {
        const date = new Date(dateStr);
        return date >= start && date <= end;
    }

    private formatDateForInput(date: Date): string {
        return date.toISOString().split('T')[0];
    }

    openMessageModal(message: ContactMessageResponse): void {
        this.selectedMessage = message;
        this.showMessageModal = true;
    }

    closeMessageModal(): void {
        this.showMessageModal = false;
        this.selectedMessage = null;
    }

    exportCSV(): void {
        if (this.allPageViews.length === 0) {
            alert('No data to export');
            return;
        }

        const headers = ['Path', 'Referrer', 'Country', 'City', 'Viewed At'];
        const rows = this.allPageViews.map(pv => [
            pv.path || '',
            pv.referrer || '',
            pv.country || '',
            pv.city || '',
            pv.viewedAt || ''
        ]);

        const csvContent = [
            headers.join(','),
            ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `analytics-export-${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }


    exportPDF(): void {
  const dashboardElement = document.querySelector('.analytics-container') as HTMLElement;
  
  if (!dashboardElement) {
    alert('No data to export');
    return;
  }

  // Show loading state
  const originalTitle = document.title;
  document.title = 'Generating PDF...';

  html2canvas(dashboardElement, {
    scale: 2, // Higher quality
    useCORS: true,
    logging: false,
    backgroundColor: '#ffffff'
  }).then(canvas => {
    const imgData = canvas.toDataURL('image/png');
    const pdf = new jsPDF('p', 'mm', 'a4');
    const imgWidth = 210; // A4 width in mm
    const pageHeight = 295; // A4 height in mm
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    
    let heightLeft = imgHeight;
    let position = 0;

    pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
    heightLeft -= pageHeight;

    // Add additional pages if content is too long
    while (heightLeft >= 0) {
      position = heightLeft - imgHeight;
      pdf.addPage();
      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;
    }

    // Restore original title and download
    document.title = originalTitle;
    pdf.save(`analytics-dashboard-${new Date().toISOString().split('T')[0]}.pdf`);
  }).catch(error => {
    console.error('Error generating PDF:', error);
    document.title = originalTitle;
    alert('Error generating PDF: ' + error.message);
  });
}
    dismissError(): void {
        this.errorMessage = null;
    }

    formatDate(dateStr: string | Date): string {
        const date = new Date(dateStr);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }

    private destroyCharts(): void {
        this.trafficChart?.destroy();
        this.topPagesChart?.destroy();
        this.blogEngagementChart?.destroy();
        this.subscribersChart?.destroy();
        this.projectsChart?.destroy();
    }
}