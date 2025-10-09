import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  imports: [CommonModule],
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

  getIconSvg(iconName: string): string {
    const icons: { [key: string]: string } = {
      'folder': '<path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"></path>',
      'file-text': '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line>',
      'mail': '<path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline>',
      'eye': '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>'
    };
    return icons[iconName] || '';
  }
}