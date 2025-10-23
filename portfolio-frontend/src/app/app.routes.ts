import { Routes } from '@angular/router';
import { ProjectsComponent } from './components/project/projects.component';
import { ProjectDetailsComponent } from './components/project/details/project-details.component';
import { ExperiencesComponent } from './components/experience/experiences.component';
import { BlogComponent } from './components/blog/blog.component';
import { ContactComponent } from './components/contact/contact.component';
import { AboutComponent } from './components/about/about.component';
import { authGuard } from './guards/auth.guard';
import { UnsubscribeComponent } from './components/unsubscribe/unsubscribe.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
   {
    path: 'admin/login',
    loadComponent: () => import('./components/admin/admin-login/admin-login.component').then(m => m.AdminLoginComponent)

  },
   {
    path: 'unsubscribe',
    component: UnsubscribeComponent
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
      {
        path: 'tech',
        loadComponent: () => import('./components/admin/CRUDs/tech/tech-CRUD.component').then(m => m.TechCrudComponent)
      },
      {
        path: 'tag',
        loadComponent: () => import('./components/admin/CRUDs/tag/tag-CRUD.component').then(m => m.TagCrudComponent)
      },
      {
        path: 'social-link',
        loadComponent: () => import('./components/admin/CRUDs/social-link/social-link-CRUD.component').then(m => m.SocialLinkCrudComponent)
      },
      {
        path: 'certificate',
        loadComponent: () => import('./components/admin/CRUDs/certificate/certificate-CRUD.component').then(m => m.CertificateCrudComponent)
      },
      {
        path: 'media',
        loadComponent: () => import('./components/admin/CRUDs/media/media-CRUD.component').then(m => m.MediaCrudComponent)
      },
      {
        path: 'skill',
        loadComponent: () => import('./components/admin/CRUDs/skill/skill-CRUD.component').then(m => m.SkillCrudComponent)
      },
      {
        path: 'user',
        loadComponent: () => import('./components/admin/CRUDs/application-user/application-user-CRUD.component').then(m => m.ApplicationUserCrudComponent)
      },
      {
        path: 'testimonial',
        loadComponent: () => import('./components/admin/CRUDs/testimonial/testimonial-CRUD.component').then(m => m.TestimonialCrudComponent)
      },
      // {
      //   path: 'projects',
      //   loadComponent: () => import('./components/admin/projects/admin-projects.component').then(m => m.AdminProjectsComponent)
      // },
      {
        path: 'blog-post',
        loadComponent: () => import('./components/admin/CRUDs/blog/blog-post-CRUD.component').then(m => m.BlogPostCrudComponent)
      },
      {
        path: 'subscriber',
        loadComponent: () => import('./components/admin/subscriber/overview/subscriber-overview.component').then(m => m.SubscribersOverviewComponent)
      },
      {
        path: 'subscriber-list',
        loadComponent: () => import('./components/admin/subscriber/list/subscriber-list.component').then(m => m.SubscribersListComponent)
      },
      {
        path: 'subscriber-analytics',
        loadComponent: () => import('./components/admin/subscriber/analytics/subscriber-analytics.component').then(m => m.SubscribersAnalyticsComponent)
      },
      {
        path: 'subscriber-details',
        loadComponent: () => import('./components/admin/subscriber/details/subscriber-details.component').then(m => m.SubscriberDetailsComponent)
      },
      {
        path: 'subscriber-export-import',
        loadComponent: () => import('./components/admin/subscriber/export-import/subscriber-export-import.component').then(m => m.SubscribersExportImportComponent)
      },
      {
        path: 'message',
        loadComponent: () => import('./components/admin/contact-message/contact-message.component').then(m => m.ContactMessagesComponent)
      },
      {
  path: 'email-template',
  loadComponent: () => import('./components/admin/email-template/email-template-manager.component').then(m => m.EmailTemplateManagerComponent)
}
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