import { bootstrapApplication } from '@angular/platform-browser';
import { importProvidersFrom } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { App } from './app/app';
import { routes } from './app/app.routes';
import { authInterceptor } from './app/core/interceptors/auth-interceptor';
import { errorInterceptor } from './app/core/interceptors/error-interceptor';

bootstrapApplication(App, {
  providers: [
    importProvidersFrom(
      BrowserAnimationsModule,
      RouterModule.forRoot(routes)
    ),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))
  ]
}).catch(err => console.error(err));