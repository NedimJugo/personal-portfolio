import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { UserInfo } from '../../../models/auth/user-info.model';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent implements OnInit {
  currentUser: UserInfo | null = null;
  sidebarCollapsed = false;
  mobileMenuOpen = false;
  contentExpanded = true;

  standaloneItems = [
    { icon: 'grid', label: 'Overview', route: '/admin/dashboard/overview', active: true }
  ];

  contentItems = [
    { icon: 'users', label: 'Users', route: '/admin/dashboard/user', active: false },
    { icon: 'folder', label: 'Projects', route: '/admin/dashboard/projects', active: false },
    { icon: 'file-text', label: 'Blog Posts', route: '/admin/dashboard/blog-post', active: false },
    { icon: 'cpu', label: 'Technologies', route: '/admin/dashboard/tech', active: false },
    { icon: 'tag', label: 'Tags', route: '/admin/dashboard/tag', active: false },
    { icon: 'share-2', label: 'Social Links', route: '/admin/dashboard/social-link', active: false },
    { icon: 'graduation-cap', label: 'Education', route: '/admin/dashboard/education', active: false },
    { icon: 'award', label: 'Certificates', route: '/admin/dashboard/certificate', active: false },
    { icon: 'zap', label: 'Skills', route: '/admin/dashboard/skill', active: false },
    { icon: 'mail', label: 'Messages', route: '/admin/dashboard/messages', active: false },
    { icon: 'image', label: 'Media', route: '/admin/dashboard/media', active: false }
  ];

  bottomItems = [
    { icon: 'bar-chart', label: 'Analytics', route: '/admin/dashboard/analytics', active: false },
    { icon: 'settings', label: 'Settings', route: '/admin/dashboard/settings', active: false }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  toggleContentSection(): void {
    this.contentExpanded = !this.contentExpanded;
  }

  loadCurrentUser(): void {
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser = user;
      },
      error: (error) => {
        console.error('Error loading user:', error);
        this.logout();
      }
    });
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  logout(): void {
    const refreshToken = this.authService.getRefreshToken();
    this.authService.logout(refreshToken || undefined).subscribe({
      next: () => {
        this.router.navigate(['/admin/login']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        this.authService.clearSession();
        this.router.navigate(['/admin/login']);
      }
    });
  }
}