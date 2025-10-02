import { Routes } from '@angular/router';
import { ProjectsComponent } from './components/project/projects.component';
import { ProjectDetailsComponent } from './components/project/details/project-details.component';
import { ExperiencesComponent } from './components/experience/experiences.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { 
    path: 'home', 
    loadComponent: () => import('./components/home.component').then(m => m.HomeComponent)
  },
   { path: 'projects', component: ProjectsComponent },
  { path: 'projects/:id', component: ProjectDetailsComponent }, // if you have a details page
  // { path: 'about', component: AboutComponent },
  { path: 'experience', component: ExperiencesComponent },
  // { path: 'blog', component: BlogComponent },
  // { path: 'contact', component: ContactComponent },
  { path: '**', redirectTo: '' } // redirect to home for unknown routes
];