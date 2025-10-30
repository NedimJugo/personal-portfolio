import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'projects/:id',
    renderMode: RenderMode.Server  // Use server-side rendering instead of prerendering
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender  // Prerender all other routes
  }
];