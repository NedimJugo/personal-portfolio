import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { ProjectService } from '../../../core/services/project';
import { AuthService } from '../../../core/services/auth';
import { Project } from '../../../core/models/project';
import { User } from '../../../core/models/user';


@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    RouterModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Admin Dashboard</h1>
        <p>Welcome back, {{ currentUser?.fullName || 'Admin' }}!</p>
      </div>

      <!-- Stats Cards -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon">
                <mat-icon color="primary">work</mat-icon>
              </div>
              <div class="stat-details">
                <h3>{{ projectsCount }}</h3>
                <p>Total Projects</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon">
                <mat-icon color="accent">visibility</mat-icon>
              </div>
              <div class="stat-details">
                <h3>{{ totalViews }}</h3>
                <p>Total Views</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon">
                <mat-icon color="warn">publish</mat-icon>
              </div>
              <div class="stat-details">
                <h3>{{ publishedCount }}</h3>
                <p>Published</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon">
                <mat-icon>draft</mat-icon>
              </div>
              <div class="stat-details">
                <h3>{{ draftsCount }}</h3>
                <p>Drafts</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Recent Projects -->
      <mat-card class="recent-projects">
        <mat-card-header>
          <mat-card-title>Recent Projects</mat-card-title>
          <mat-card-subtitle>Your latest work</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="table-container" *ngIf="recentProjects.length > 0; else noProjectsTemplate">
            <table mat-table [dataSource]="recentProjects" class="projects-table">
              <ng-container matColumnDef="title">
                <th mat-header-cell *matHeaderCellDef>Title</th>
                <td mat-cell *matCellDef="let project">{{ project.title }}</td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let project">
                  <span class="status-badge" [class.published]="project.isPublished" [class.draft]="!project.isPublished">
                    {{ project.isPublished ? 'Published' : 'Draft' }}
                  </span>
                </td>
              </ng-container>

              <ng-container matColumnDef="views">
                <th mat-header-cell *matHeaderCellDef>Views</th>
                <td mat-cell *matCellDef="let project">{{ project.views }}</td>
              </ng-container>

              <ng-container matColumnDef="updatedAt">
                <th mat-header-cell *matHeaderCellDef>Last Updated</th>
                <td mat-cell *matCellDef="let project">{{ project.updatedAt | date:'short' }}</td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let project">
                  <button mat-icon-button color="primary" title="Edit">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="accent" title="View" [routerLink]="['/projects', project.slug]" target="_blank">
                    <mat-icon>visibility</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          </div>

          <ng-template #noProjectsTemplate>
            <div class="no-projects">
              <mat-icon>work_off</mat-icon>
              <p>No projects yet. Create your first project to get started!</p>
              <button mat-raised-button color="primary">
                <mat-icon>add</mat-icon>
                Create Project
              </button>
            </div>
          </ng-template>
        </mat-card-content>
      </mat-card>

      <!-- Quick Actions -->
      <mat-card class="quick-actions">
        <mat-card-header>
          <mat-card-title>Quick Actions</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="actions-grid">
            <button mat-raised-button color="primary">
              <mat-icon>add</mat-icon>
              New Project
            </button>
            <button mat-raised-button>
              <mat-icon>article</mat-icon>
              New Blog Post
            </button>
            <button mat-raised-button>
              <mat-icon>photo_library</mat-icon>
              Media Library
            </button>
            <button mat-raised-button>
              <mat-icon>settings</mat-icon>
              Settings
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styleUrls: ['./dashboard.scss']
})
export class Dashboard implements OnInit {
  currentUser: User | null = null;
  recentProjects: Project[] = [];
  projectsCount = 0;
  publishedCount = 0;
  draftsCount = 0;
  totalViews = 0;

  displayedColumns: string[] = ['title', 'status', 'views', 'updatedAt', 'actions'];

  constructor(
    private projectService: ProjectService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.projectService.getAdminProjects().subscribe({
      next: (response) => {
        const projects = response.data;
        this.recentProjects = projects.slice(0, 5); // Show latest 5
        this.projectsCount = projects.length;
        this.publishedCount = projects.filter(p => p.isPublished).length;
        this.draftsCount = projects.filter(p => !p.isPublished).length;
        this.totalViews = projects.reduce((sum, p) => sum + p.views, 0);
      },
      error: (error) => {
        console.error('Failed to load dashboard data:', error);
      }
    });
  }
}