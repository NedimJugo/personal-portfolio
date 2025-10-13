import { Routes } from '@angular/router';
import { ProjectsComponent } from './components/project/projects.component';
import { ProjectDetailsComponent } from './components/project/details/project-details.component';
import { ExperiencesComponent } from './components/experience/experiences.component';
import { BlogComponent } from './components/blog/blog.component';
import { ContactComponent } from './components/contact/contact.component';
import { AboutComponent } from './components/about/about.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
   {
    path: 'admin/login',
    loadComponent: () => import('./components/admin/admin-login/admin-login.component').then(m => m.AdminLoginComponent)

  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./components/admin/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'overview',
        pathMatch: 'full'
      },
      {
        path: 'overview',
        loadComponent: () => import('./components/admin/admin-overview/admin-overview.component').then(m => m.AdminOverviewComponent)
      },
      {
        path: 'education',
        loadComponent: () => import('./components/admin/CRUDs/education/education-CRUD.component').then(m => m.EducationCrudComponent)
      },
      // {
      //   path: 'users',
      //   loadComponent: () => import('./components/admin/users/admin-users.component').then(m => m.AdminUsersComponent)
      // },
      // {
      //   path: 'projects',
      //   loadComponent: () => import('./components/admin/projects/admin-projects.component').then(m => m.AdminProjectsComponent)
      // },
      // {
      //   path: 'blog',
      //   loadComponent: () => import('./components/admin/blog/admin-blog.component').then(m => m.AdminBlogComponent)
      // },
      // {
      //   path: 'messages',
      //   loadComponent: () => import('./components/admin/messages/admin-messages.component').then(m => m.AdminMessagesComponent)
      // },
      // {
      //   path: 'analytics',
      //   loadComponent: () => import('./components/admin/analytics/admin-analytics.component').then(m => m.AdminAnalyticsComponent)
      // },
      // {
      //   path: 'settings',
      //   loadComponent: () => import('./components/admin/settings/admin-settings.component').then(m => m.AdminSettingsComponent)
      // }
    ]
  },
  { 
    path: 'home', 
    loadComponent: () => import('./components/home.component').then(m => m.HomeComponent)
  },
   { path: 'projects', component: ProjectsComponent },
  { path: 'projects/:id', component: ProjectDetailsComponent }, // if you have a details page
  { path: 'about', component: AboutComponent },
  { path: 'experience', component: ExperiencesComponent },
  { path: 'blog', component: BlogComponent },
  { path: 'contact', component: ContactComponent },
  { path: '**', redirectTo: '' }, // redirect to home for unknown routes

 
];