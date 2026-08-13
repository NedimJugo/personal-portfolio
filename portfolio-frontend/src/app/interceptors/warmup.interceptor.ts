import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { WarmupService } from '../services/warmup.service';

export const warmupInterceptor: HttpInterceptorFn = (req, next) => {
  const warmupService = inject(WarmupService);
  
  // Start warmup on first request
  warmupService.startWarmup();

  return next(req).pipe(
    finalize(() => {
      // Complete warmup after first request finishes
      warmupService.completeWarmup();
    })
  );
};
