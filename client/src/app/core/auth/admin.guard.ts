import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

// Admin sayfalarına girmeden önce oturum var mı bakıyor.
// NOT: Bu guard SADECE kullanıcı deneyimi için. Tarayıcıdaki kod değiştirilebilir;
// asıl güvenlik API tarafındaki RequireAuthorization("Admin") kontrolünde.
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.ensureSession().pipe(
    map((ok) => (ok ? true : router.createUrlTree(['/admin/login'], { queryParams: { returnUrl: state.url } })))
  );
};

// Zaten giriş yapmışsa login sayfasını gösterme, panele gönder
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.ensureSession().pipe(map((ok) => (ok ? router.createUrlTree(['/admin']) : true)));
};
