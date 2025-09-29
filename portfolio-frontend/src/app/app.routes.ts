import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/contact-messages', pathMatch: 'full' },
  { 
    path: 'contact-messages', 
    loadComponent: () => import('./components/contact-messages/contact-message-list.component').then(m => m.ContactMessageListComponent)
  },
  { path: '**', redirectTo: '/contact-messages' }
];