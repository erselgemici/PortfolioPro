import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith } from 'rxjs';
import { PortfolioApi } from '../../core/api/portfolio-api.service';
import { ProjectCategory, ProjectListItem } from '../../core/models';
import { I18nService } from '../../core/i18n/i18n.service';
import { TranslatePipe } from '../../core/i18n/i18n.pipes';
import { TranslationKey } from '../../core/i18n/translations';
import { SeoService } from '../../core/seo.service';
import { ProjectCard } from '../../shared/project-card';

type Filter = 'all' | ProjectCategory;

interface PageState {
  loading: boolean;
  error: boolean;
  projects: ProjectListItem[];
}

@Component({
  selector: 'app-projects-page',
  imports: [TranslatePipe, ProjectCard],
  template: `
    <section class="section page">
      <div class="container">
        <div class="section-head">
          <span class="kicker">~/projects</span>
          <h1>{{ 'projects.pageTitle' | t }}</h1>
          <p>{{ 'projects.pageSubtitle' | t }}</p>
        </div>

        <!-- Sadece projesi olan kategoriler filtre olarak çıkıyor -->
        <div class="filters" role="tablist">
          @for (f of filters(); track f.value) {
            <button type="button" role="tab" class="filter" [class.active]="filter() === f.value"
              [attr.aria-selected]="filter() === f.value" (click)="filter.set(f.value)">
              {{ f.key | t }} <span class="count">{{ f.count }}</span>
            </button>
          }
        </div>

        @if (state().loading) {
          <div class="grid">
            @for (i of [1, 2, 3]; track i) {
              <div class="skeleton" style="height: 380px; border-radius: 18px"></div>
            }
          </div>
        } @else if (state().error) {
          <p class="muted">{{ 'common.loadError' | t }}</p>
        } @else {
          <div class="grid">
            @for (project of visible(); track project.id) {
              <app-project-card [project]="project" />
            } @empty {
              <p class="muted">{{ 'projects.empty' | t }}</p>
            }
          </div>
        }
      </div>
    </section>
  `,
  styles: `
    .page { padding-top: 64px; }
    h1 { font-size: clamp(2rem, 4vw, 2.8rem); }
    .filters { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 32px; }
    .filter {
      display: inline-flex; align-items: center; gap: 8px; height: 36px; padding: 0 14px;
      border-radius: 999px; border: 1px solid var(--border); background: var(--bg-elev);
      color: var(--text-muted); font-size: 0.88rem; font-weight: 500; transition: all 0.15s;
    }
    .filter:hover { color: var(--text); border-color: var(--border-strong); }
    .filter.active { background: var(--text); color: var(--bg); border-color: var(--text); }
    .count { font-family: var(--font-mono); font-size: 0.75rem; opacity: 0.7; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
    @media (max-width: 400px) { .grid { grid-template-columns: 1fr; } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsPage {
  private readonly api = inject(PortfolioApi);
  private readonly i18n = inject(I18nService);

  protected readonly filter = signal<Filter>('all');

  // NOT: toSignal ile Observable'ı signal'e çevirdim. subscribe/unsubscribe yazmaya gerek kalmıyor,
  // component yok olunca abonelik otomatik kapanıyor.
  protected readonly state = toSignal(
    this.api.getProjects().pipe(
      map((projects): PageState => ({ loading: false, error: false, projects })),
      catchError(() => of<PageState>({ loading: false, error: true, projects: [] })),
      startWith<PageState>({ loading: true, error: false, projects: [] })
    ),
    { requireSync: true }
  );

  protected readonly filters = computed(() => {
    const projects = this.state().projects;
    const categories: ProjectCategory[] = ['Web', 'Mobile', 'Api', 'Tool'];

    return [
      { value: 'all' as Filter, key: 'projects.filterAll' as TranslationKey, count: projects.length },
      ...categories
        .map((c) => ({
          value: c as Filter,
          key: `category.${c}` as TranslationKey,
          count: projects.filter((p) => p.category === c).length
        }))
        .filter((f) => f.count > 0)
    ];
  });

  protected readonly visible = computed(() => {
    const f = this.filter();
    const projects = this.state().projects;
    return f === 'all' ? projects : projects.filter((p) => p.category === f);
  });

  constructor() {
    inject(SeoService).set(`${this.i18n.t('projects.pageTitle')} · Ersel Gemici`, this.i18n.t('projects.pageSubtitle'));
  }
}
