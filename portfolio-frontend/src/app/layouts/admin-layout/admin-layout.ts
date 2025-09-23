import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth';
import { User } from '../../core/models/auth-response';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule
  ],
  template: `
    <mat-sidenav-container class="admin-container">
      <!-- Sidebar -->
      <mat-sidenav #sidenav mode="side" opened class="admin-sidenav">
        <div class="sidenav-header">
          <h3>Portfolio Admin</h3>
        </div>
        <mat-nav-list>
          <a mat-list-item routerLink="/admin/dashboard" routerLinkActive="active">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>
          <a mat-list-item routerLink="/admin/projects" routerLinkActive="active">
            <mat-icon matListItemIcon>work</mat-icon>
            <span matListItemTitle>Projects</span>
          </a>
          <a mat-list-item routerLink="/admin/blog" routerLinkActive="active">
            <mat-icon matListItemIcon>article</mat-icon>
            <span matListItemTitle>Blog</span>
          </a>
          <a mat-list-item routerLink="/admin/media" routerLinkActive="active">
            <mat-icon matListItemIcon>photo_library</mat-icon>
            <span matListItemTitle>Media</span>
          </a>
          <a mat-list-item routerLink="/admin/messages" routerLinkActive="active">
            <mat-icon matListItemIcon>message</mat-icon>
            <span matListItemTitle>Messages</span>
          </a>
          <mat-divider></mat-divider>
          <a mat-list-item routerLink="/admin/settings" routerLinkActive="active">
            <mat-icon matListItemIcon>settings</mat-icon>
            <span matListItemTitle>Settings</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <!-- Main Content -->
      <mat-sidenav-content class="admin-content">
        <!-- Top Toolbar -->
        <mat-toolbar class="admin-toolbar">
          <button mat-icon-button (click)="sidenav.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          
          <span class="toolbar-spacer"></span>

          <!-- View Site Button -->
          <button mat-button routerLink="/" target="_blank">
            <mat-icon>launch</mat-icon>
            View Site
          </button>

          <!-- User Menu -->
          <button mat-icon-button [matMenuTriggerFor]="userMenu">
            <mat-icon>account_circle</mat-icon>
          </button>
          <mat-menu #userMenu="matMenu">
            <div class="user-info">
              <p><strong>{{ currentUser?.fullName }}</strong></p>
              <p class="email">{{ currentUser?.email }}</p>
            </div>
            <mat-divider></mat-divider>
            <button mat-menu-item routerLink="/admin/profile">
              <mat-icon>person</mat-icon>
              Profile
            </button>
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              Logout
            </button>
          </mat-menu>
        </mat-toolbar>

        <!-- Page Content -->
        <div class="page-content">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styleUrls: ['./admin-layout.scss']
})
export class AdminLayout {
  currentUser: User | null = null;

  constructor(private authService: AuthService) {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout();
  }
}