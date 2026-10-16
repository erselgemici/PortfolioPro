import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { PortfolioApi } from '../../core/api/portfolio-api.service';
import { ProjectDetail } from '../../core/models';
import { I18nService } from '../../core/i18n/i18n.service';
import { LocalizePipe, TranslatePipe } from '../../core/i18n/i18n.pipes';
import { TranslationKey } from '../../core/i18n/translations';
import { SeoService } from '../../core/seo.service';
import { Icon } from '../../shared/icon';
import { renderMarkdown } from '../../shared/markdown';
import { MonthYearPipe } from '../../shared/date.pipe';

@Component({
  selector: 'app-project-detail',
  imports: [RouterLink, TranslatePipe, LocalizePipe, Icon, MonthYearPipe],
  template: `
    <article class="section page">
      <div class="container narrow">
        <a routerLink="/projects" class="back"><app-icon name="arrow-left" /> {{ 'projects.back' | t }}</a>

        @if (loading()) {
          <div class="skeleton" style="height: 48px; width: 70%; margin-top: 24px"></div>
          <div class="skeleton" style="height: 320px; margin-top: 24px; border-radius: 18px"></div>
        } @else if (notFound()) {
          <h1 class="title">{{ 'projects.notFound' | t }}</h1>
        } @else if (failed()) {
          <p class="muted" style="margin-top: 24px">{{ 'common.loadError' | t }}</p>
        } @else if (project(); as p) {
          <header class="head">
            <div class="meta">
              <span class="badge badge-accent">{{ categoryKey() | t }}</span>
              <span class="date mono">{{ p.projectDate | monthYear }}</span>
            </div>
            <h1 class="title">{{ p.title | loc }}</h1>
            <p class="summary">{{ p.summary | loc }}</p>

            <div class="actions">
              @if (p.liveUrl) {
                <a class="btn btn-primary" [href]="p.liveUrl" target="_blank" rel="noopener">
                  <app-icon name="external" /> {{ 'projects.live' | t }}
                </a>
              }
              @if (p.storeUrl) {
                <a class="btn btn-primary" [href]="p.storeUrl" target="_blank" rel="noopener">
                  <app-icon name="play" /> {{ 'projects.store' | t }}
                </a>
              }
              @if (p.githubUrl) {
                <a class="btn" [href]="p.githubUrl" target="_blank" rel="noopener">
                  <app-icon name="github" /> {{ 'projects.code' | t }}
                </a>
              }
            </div>
          </header>

          @if (p.coverImageUrl) {
            <img class="cover" [src]="p.coverImageUrl" [alt]="p.title | loc" />
          }

          <div class="layout">
            <!-- NOT: renderMarkdown önce HTML karakterlerini zararsız hale getiriyor (escape), sonra Angular da [innerHTML]'i ayrıca temizliyor -->
            <div class="prose" [innerHTML]="contentHtml()"></div>

            <aside class="side card">
              <h2>{{ 'projects.technologies' | t }}</h2>
              <div class="chips">
                @for (tech of p.technologies; track tech) {
                  <span class="chip">{{ tech }}</span>
                }
              </div>
            </aside>
          </div>
        }
      </div>
    </article>
  `,
  styles: `
    .page { padding-top: 48px; }
    .narrow { max-width: 920px; }
    .back { display: inline-flex; align-items: center; gap: 8px; color: var(--text-muted); font-size: 0.9rem; font-weight: 500; }
    .back:hover { color: var(--accent); }
    .back app-icon { width: 16px; height: 16px; }
    .head { margin-top: 28px; display: flex; flex-direction: column; align-items: flex-start; gap: 14px; }
    .meta { display: flex; align-items: center; gap: 12px; }
    .date { font-size: 0.82rem; color: var(--text-muted); }
    .title { font-size: clamp(2rem, 4.5vw, 3rem); letter-spacing: -0.03em; margin-top: 4px; }
    .summary { font-size: 1.12rem; color: var(--text-muted); max-width: 680px; }
    .actions { display: flex; flex-wrap: wrap; gap: 10px; margin-top: 8px; }
    .cover { width: 100%; margin-top: 36px; border-radius: var(--radius-lg); border: 1px solid var(--border); }
    .layout { display: grid; grid-template-columns: minmax(0, 1fr) 240px; gap: 48px; margin-top: 44px; align-items: start; }
    .side { padding: 20px; position: sticky; top: calc(var(--header-h) + 20px); }
    .side h2 { font-size: 0.85rem; color: var(--text-muted); margin-bottom: 12px; font-weight: 600; }
    @media (max-width: 800px) { .layout { grid-template-columns: 1fr; } .side { position: static; order: -1; } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectDetailPage {
  private readonly api = inject(PortfolioApi);
  private readonly i18n = inject(I18nService);
  private readonly seo = inject(SeoService);

  // withComponentInputBinding sayesinde /projects/:slug parametresi direkt buraya geliyor
  readonly slug = input.required<string>();

  protected readonly project = signal<ProjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly failed = signal(false);

  protected readonly categoryKey = computed(() => `category.${this.project()?.category ?? 'Web'}` as TranslationKey);
  protected readonly contentHtml = computed(() => renderMarkdown(this.i18n.pick(this.project()?.content)));

  constructor() {
    // slug değişince (bir projeden diğerine geçince) yeniden yükle
    effect((onCleanup) => {
      const slug = this.slug();
      this.loading.set(true);
      this.notFound.set(false);
      this.failed.set(false);

      const sub = this.api.getProject(slug).subscribe({
        next: (p) => {
          this.project.set(p);
          this.loading.set(false);
        },
        error: (err: unknown) => {
          this.loading.set(false);
          if (err instanceof HttpErrorResponse && err.status === 404) this.notFound.set(true);
          else this.failed.set(true);
        }
      });

      onCleanup(() => sub.unsubscribe());
    });

    effect(() => {
      const p = this.project();
      if (p) this.seo.set(`${this.i18n.pick(p.title)} · Ersel Gemici`, this.i18n.pick(p.summary));
    });
  }
}
