import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProjectService } from '../../../services/project.service';
import { BlogPostService } from '../../../services/blog-post.service';
import { ContactMessageService } from '../../../services/contact-message.service';
import { PageViewService } from '../../../services/page-view.service';

interface StatCard {
  title: string;
  value: string | number;
  icon: string;
  color: string;
  change?: string;
  isLoading: boolean;
}

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-overview.component.html',
  styleUrls: ['./admin-overview.component.scss']
})
export class AdminOverviewComponent implements OnInit {
  stats: StatCard[] = [
    {
      title: 'Total Projects',
      value: 0,
      icon: 'folder',
      color: '#667eea',
      isLoading: true
    },
    {
      title: 'Blog Posts',
      value: 0,
      icon: 'file-text',
      color: '#f56565',
      isLoading: true
    },
    {
      title: 'Messages',
      value: 0,
      icon: 'mail',
      color: '#48bb78',
      isLoading: true
    },
    {
      title: 'Page Views',
      value: 0,
      icon: 'eye',
      color: '#ed8936',
      isLoading: true
    }
  ];

  recentActivity: any[] = [];
  isLoadingActivity = true;

  constructor(
    private projectService: ProjectService,
    private blogService: BlogPostService,
    private contactService: ContactMessageService,
    private pageViewService: PageViewService
  ) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    // Load Projects
    this.projectService.get({ page: 1, pageSize: 1, includeTotalCount: true }).subscribe({
      next: (result) => {
        this.stats[0].value = result.totalCount || 0;
        this.stats[0].isLoading = false;
      },
      error: () => {
        this.stats[0].isLoading = false;
      }
    });

    // Load Blog Posts
    this.blogService.get({ page: 1, pageSize: 1, includeTotalCount: true }).subscribe({
      next: (result) => {
        this.stats[1].value = result.totalCount || 0;
        this.stats[1].isLoading = false;
      },
      error: () => {
        this.stats[1].isLoading = false;
      }
    });

    // Load Messages
    this.contactService.get({ page: 1, pageSize: 1, includeTotalCount: true }).subscribe({
      next: (result) => {
        this.stats[2].value = result.totalCount || 0;
        this.stats[2].isLoading = false;
      },
      error: () => {
        this.stats[2].isLoading = false;
      }
    });

    // Load Page Views
    this.pageViewService.get({ page: 1, pageSize: 1, includeTotalCount: true }).subscribe({
      next: (result) => {
        this.stats[3].value = result.totalCount || 0;
        this.stats[3].isLoading = false;
      },
      error: () => {
        this.stats[3].isLoading = false;
      }
    });

    // Load Recent Activity (recent messages)
    this.contactService.get({ page: 1, pageSize: 5, sortBy: 'createdAt', desc: true }).subscribe({
      next: (result) => {
        this.recentActivity = result.items.map(msg => ({
          type: 'message',
          title: msg.subject,
          description: `From ${msg.name} (${msg.email})`,
          time: this.getTimeAgo(new Date(msg.createdAt)),
          icon: 'mail',
          color: '#48bb78'
        }));
        this.isLoadingActivity = false;
      },
      error: () => {
        this.isLoadingActivity = false;
      }
    });
  }

  getTimeAgo(date: Date): string {
    const seconds = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    
    const intervals: { [key: string]: number } = {
      year: 31536000,
      month: 2592000,
      week: 604800,
      day: 86400,
      hour: 3600,
      minute: 60,
      second: 1
    };

    for (const [name, secondsInInterval] of Object.entries(intervals)) {
      const interval = Math.floor(seconds / secondsInInterval);
      if (interval >= 1) {
        return `${interval} ${name}${interval !== 1 ? 's' : ''} ago`;
      }
    }

    return 'just now';
  }
}