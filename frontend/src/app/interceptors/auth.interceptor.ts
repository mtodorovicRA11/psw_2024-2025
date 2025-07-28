import { HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export function authInterceptor(request: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  // Ne dodaj token za login, register i test endpoint-ove
  const isAuthEndpoint = request.url.includes('/api/Users/login') || 
                        request.url.includes('/api/Users/register') ||
                        request.url.includes('/api/Tour/create-test-tours') ||
                        request.url.includes('/api/Users/test-email');
  
  if (token && !isAuthEndpoint) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      // Ne uhvataj 401 greške na auth endpoint-ovima
      if (error.status === 401 && !isAuthEndpoint) {
        console.log('401 Unauthorized error caught in interceptor');
        authService.logout();
      }
      return throwError(() => error);
    })
  );
} 