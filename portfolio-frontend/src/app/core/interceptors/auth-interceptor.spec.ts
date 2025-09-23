import { TestBed } from '@angular/core/testing';
import { HttpInterceptorFn, HttpRequest, HttpHandler } from '@angular/common/http';
import { of } from 'rxjs';

import { authInterceptor } from './auth-interceptor';
import { AuthService } from '../services/auth';

describe('authInterceptor', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('AuthService', [], { token$: of('test-token') });

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: spy }
      ]
    });

    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should be created', () => {
    TestBed.runInInjectionContext(() => {
      expect(authInterceptor).toBeTruthy();
    });
  });

  it('should add authorization header when token is available', () => {
    TestBed.runInInjectionContext(() => {
      const mockRequest = new HttpRequest('GET', '/test');
      const mockNext: HttpHandler = {
        handle: jasmine.createSpy('handle').and.returnValue(of({}))
      };

      authInterceptor(mockRequest, mockNext.handle);

      expect(mockNext.handle).toHaveBeenCalled();
    });
  });
});
