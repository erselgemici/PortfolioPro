import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

// Token gereken istekler: admin API'si ve oturum gerektiren auth endpoint'leri
function needsToken(req: HttpRequest<unknown>): boolean {
  return req.url.startsWith('/api/admin') || req.url === '/api/auth/me' || req.url === '/api/auth/change-password';
}

function withToken(req: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  // NOT: Angular'da gelen istek nesnesi değiştirilemiyor. clone ile kopyasını alıp header'ı ona ekliyoruz.
  return token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
}

// Angular 15'ten beri interceptor'ı class yerine fonksiyon olarak yazabiliyoruz. DevLog'dakinin yeni hali.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!needsToken(req)) return next(req);

  const auth = inject(AuthService);
  const router = inject(Router);

  return next(withToken(req, auth.accessToken)).pipe(
    catchError((error: unknown) => {
      // 401 geldiyse access token'ın süresi dolmuş olabilir: bir kez refresh dene, isteği tekrarla
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return auth.refresh().pipe(
          switchMap((ok) => {
            if (!ok) {
              router.navigate(['/admin/login']);
              return throwError(() => error);
            }
            return next(withToken(req, auth.accessToken));
          })
        );
      }
      return throwError(() => error);
    })
  );
};
