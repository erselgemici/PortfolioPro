import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { AccessTokenResponse, AdminUser } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // NOT: Access token SADECE bellekte (signal) tutuluyor, localStorage'a yazılmıyor.
  // localStorage'daki veriyi sayfadaki her script okuyabilir; kötü niyetli bir script sızarsa (XSS) token çalınır. Sayfa yenilenince token
  // kayboluyor ama sorun değil: httpOnly cookie'deki refresh token ile sessizce yenisi alınıyor.
  private readonly token = signal<string | null>(null);
  private readonly expiresAt = signal<number>(0);
  readonly user = signal<AdminUser | null>(null);

  readonly isLoggedIn = computed(() => !!this.token());

  // Aynı anda birden fazla istek 401 alırsa hepsi ayrı ayrı refresh atmasın, tek istek paylaşılsın
  private refreshInFlight$: Observable<boolean> | null = null;

  get accessToken(): string | null {
    return this.token();
  }

  hasValidToken(): boolean {
    // süresinin dolmasına 10 sn'den az kaldıysa geçersiz say
    return !!this.token() && Date.now() < this.expiresAt() - 10_000;
  }

  login(email: string, password: string): Observable<void> {
    return this.http
      .post<AccessTokenResponse>('/api/auth/login', { email, password }, { withCredentials: true })
      .pipe(
        tap((res) => this.setSession(res)),
        map(() => undefined)
      );
  }

  // Cookie'deki refresh token ile yeni access token al. Başarılıysa true.
  refresh(): Observable<boolean> {
    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.http
        .post<AccessTokenResponse>('/api/auth/refresh', null, { withCredentials: true })
        .pipe(
          tap((res) => this.setSession(res)),
          map(() => true),
          catchError(() => {
            this.clearSession();
            return of(false);
          }),
          finalize(() => (this.refreshInFlight$ = null)),
          shareReplay(1)
        );
    }
    return this.refreshInFlight$;
  }

  // Guard'ın kullandığı metot: token geçerliyse hemen true, değilse refresh dene
  ensureSession(): Observable<boolean> {
    return this.hasValidToken() ? of(true) : this.refresh();
  }

  logout(redirect = true): void {
    this.http
      .post('/api/auth/logout', null, { withCredentials: true })
      .pipe(catchError(() => of(null)))
      .subscribe();

    this.clearSession();
    if (redirect) this.router.navigate(['/admin/login']);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>('/api/auth/change-password', { currentPassword, newPassword });
  }

  clearSession(): void {
    this.token.set(null);
    this.expiresAt.set(0);
    this.user.set(null);
  }

  private setSession(res: AccessTokenResponse): void {
    this.token.set(res.accessToken);
    this.expiresAt.set(new Date(res.expiresAt).getTime());
    this.user.set(res.user);
  }
}
