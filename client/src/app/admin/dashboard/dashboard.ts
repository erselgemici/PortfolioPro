import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { AdminApi } from '../../core/api/admin-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Icon } from '../../shared/icon';
import { PageHeader } from '../shared/page-header';

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink, DatePipe, Icon, PageHeader],
  template: `
    <app-page-header [title]="'Hoş geldin, ' + (auth.user()?.fullName ?? '')" description="Sitedeki içeriğin kısa özeti.">
      <a routerLink="/admin/projects" class="btn btn-primary btn-sm"><app-icon name="plus" /> Proje ekle</a>
    </app-page-header>

    @if (data(); as d) {
      <div class="stats">
        <a routerLink="/admin/projects" class="card stat">
          <span class="stat-icon"><app-icon name="folder" /></span>
          <span class="label">Projeler</span>
          <strong>{{ d.projects }}</strong>
          <span class="sub">{{ d.publishedProjects }} yayında</span>
        </a>
        <a routerLink="/admin/messages" class="card stat">
          <span class="stat-icon"><app-icon name="inbox" /></span>
          <span class="label">Mesajlar</span>
          <strong>{{ d.messages }}</strong>
          <span class="sub" [class.accent]="d.unreadMessages > 0">{{ d.unreadMessages }} okunmamış</span>
        </a>
        <a routerLink="/admin/skills" class="card stat">
          <span class="stat-icon"><app-icon name="layers" /></span>
          <span class="label">Yetenekler</span>
          <strong>{{ d.skills }}</strong>
          <span class="sub">teknoloji</span>
        </a>
        <a routerLink="/admin/experiences" class="card stat">
          <span class="stat-icon"><app-icon name="briefcase" /></span>
          <span class="label">Deneyim</span>
          <strong>{{ d.experiences }}</strong>
          <span class="sub">{{ d.certificates }} sertifika</span>
        </a>
      </div>

      <div class="card panel">
        <div class="panel-head">
          <h2>Son mesajlar</h2>
          <a routerLink="/admin/messages" class="link">Tümü <app-icon name="arrow-right" /></a>
        </div>
        @for (m of d.latestMessages; track m.id) {
          <a routerLink="/admin/messages" class="msg">
            <span class="dot" [class.unread]="!m.isRead"></span>
            <div class="msg-body">
              <strong>{{ m.name }}</strong>
              <span class="muted">{{ m.subject || m.body }}</span>
            </div>
            <span class="date mono">{{ m.createdAt | date: 'dd.MM.yyyy' }}</span>
          </a>
        } @empty {
          <p class="muted empty">Henüz mesaj yok.</p>
        }
      </div>
    } @else if (failed()) {
      <p class="muted">Panel verileri yüklenemedi.</p>
    } @else {
      <div class="stats">
        @for (i of [1, 2, 3, 4]; track i) {
          <div class="skeleton" style="height: 132px; border-radius: 18px"></div>
        }
      </div>
    }
  `,
  styles: `
    .stats { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat { display: flex; flex-direction: column; gap: 2px; padding: 20px; transition: border-color 0.15s; }
    .stat:hover { border-color: var(--border-strong); }
    .stat-icon { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; background: var(--accent-soft); color: var(--accent); margin-bottom: 12px; }
    .stat-icon app-icon { width: 18px; height: 18px; }
    .label { font-size: 0.85rem; color: var(--text-muted); }
    strong { font-size: 1.9rem; letter-spacing: -0.02em; }
    .sub { font-size: 0.8rem; color: var(--text-faint); }
    .sub.accent { color: var(--accent); font-weight: 600; }
    .panel { padding: 8px 0; }
    .panel-head { display: flex; justify-content: space-between; align-items: center; padding: 12px 20px; }
    .panel-head h2 { font-size: 1rem; }
    .link { display: inline-flex; align-items: center; gap: 4px; color: var(--accent); font-size: 0.88rem; font-weight: 550; }
    .link app-icon { width: 15px; height: 15px; }
    .msg { display: flex; align-items: center; gap: 12px; padding: 12px 20px; border-top: 1px solid var(--border); }
    .msg:hover { background: var(--bg-subtle); }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: transparent; flex-shrink: 0; }
    .dot.unread { background: var(--accent); }
    .msg-body { display: flex; flex-direction: column; min-width: 0; flex: 1; }
    .msg-body .muted { font-size: 0.85rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .date { font-size: 0.78rem; color: var(--text-faint); }
    .empty { padding: 12px 20px 20px; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPage {
  protected readonly auth = inject(AuthService);
  protected readonly failed = signal(false);

  protected readonly data = toSignal(
    inject(AdminApi)
      .dashboard()
      .pipe(
        catchError(() => {
          this.failed.set(true);
          return of(null);
        })
      ),
    { initialValue: null }
  );
}
