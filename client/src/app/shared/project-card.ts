import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectListItem } from '../core/models';
import { LocalizePipe, TranslatePipe } from '../core/i18n/i18n.pipes';
import { TranslationKey } from '../core/i18n/translations';
import { Icon } from './icon';

@Component({
  selector: 'app-project-card',
  imports: [RouterLink, Icon, LocalizePipe, TranslatePipe],
  template: `
    @let p = project();
    <article class="project">
      <a class="cover" [routerLink]="['/projects', p.slug]" [attr.aria-label]="p.title | loc">
        @if (p.coverImageUrl) {
          <img [src]="p.coverImageUrl" [alt]="p.title | loc" loading="lazy" decoding="async" />
        } @else {
          <div class="placeholder" [attr.data-category]="p.category">
            <app-icon [name]="categoryIcon()" />
            <span class="mono">{{ p.slug }}</span>
          </div>
        }
      </a>

      <div class="body">
        <div class="meta">
          <span class="badge">{{ categoryKey() | t }}</span>
          <span class="year mono">{{ p.projectDate.slice(0, 4) }}</span>
        </div>
        <h3><a [routerLink]="['/projects', p.slug]">{{ p.title | loc }}</a></h3>
        <p class="summary">{{ p.summary | loc }}</p>

        <div class="chips">
          @for (tech of visibleTech(); track tech) {
            <span class="chip">{{ tech }}</span>
          }
          @if (hiddenTechCount() > 0) {
            <span class="chip">+{{ hiddenTechCount() }}</span>
          }
        </div>

        <div class="links">
          <a class="details" [routerLink]="['/projects', p.slug]">
            {{ 'projects.details' | t }} <app-icon name="arrow-right" />
          </a>
          <span class="spacer"></span>
          @if (p.githubUrl) {
            <a class="icon-btn" [href]="p.githubUrl" target="_blank" rel="noopener" [attr.aria-label]="'projects.code' | t">
              <app-icon name="github" />
            </a>
          }
          @if (p.liveUrl) {
            <a class="icon-btn" [href]="p.liveUrl" target="_blank" rel="noopener" [attr.aria-label]="'projects.live' | t">
              <app-icon name="external" />
            </a>
          }
          @if (p.storeUrl) {
            <a class="icon-btn" [href]="p.storeUrl" target="_blank" rel="noopener" [attr.aria-label]="'projects.store' | t">
              <app-icon name="play" />
            </a>
          }
        </div>
      </div>
    </article>
  `,
  styles: `
    :host { display: block; }
    .project {
      height: 100%; display: flex; flex-direction: column; overflow: hidden;
      background: var(--bg-elev); border: 1px solid var(--border); border-radius: var(--radius-lg);
      transition: border-color 0.2s, transform 0.25s var(--ease), box-shadow 0.25s;
    }
    .project:hover { border-color: var(--border-strong); transform: translateY(-3px); box-shadow: var(--shadow); }
    .cover { display: block; aspect-ratio: 16 / 9; overflow: hidden; border-bottom: 1px solid var(--border); background: var(--bg-subtle); }
    .cover img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.4s var(--ease); }
    .project:hover .cover img { transform: scale(1.03); }
    .placeholder {
      width: 100%; height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px;
      color: var(--accent);
      background:
        radial-gradient(circle at 20% 20%, var(--accent-soft), transparent 55%),
        radial-gradient(var(--grid-dot) 1px, transparent 1px) 0 0 / 16px 16px,
        var(--bg-subtle);
    }
    .placeholder app-icon { width: 34px; height: 34px; }
    .placeholder span { font-size: 0.78rem; color: var(--text-muted); }
    .body { flex: 1; display: flex; flex-direction: column; gap: 10px; padding: 20px; }
    .meta { display: flex; align-items: center; justify-content: space-between; }
    .year { font-size: 0.78rem; color: var(--text-faint); }
    h3 { font-size: 1.12rem; }
    h3 a:hover { color: var(--accent); }
    .summary { color: var(--text-muted); font-size: 0.93rem; flex: 1; }
    .links { display: flex; align-items: center; gap: 2px; margin-top: 6px; }
    .details { display: inline-flex; align-items: center; gap: 6px; font-weight: 550; font-size: 0.9rem; color: var(--accent); }
    .details app-icon { width: 16px; height: 16px; transition: transform 0.2s; }
    .details:hover app-icon { transform: translateX(3px); }
    .spacer { flex: 1; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectCard {
  readonly project = input.required<ProjectListItem>();

  protected readonly visibleTech = computed(() => this.project().technologies.slice(0, 4));
  protected readonly hiddenTechCount = computed(() => Math.max(0, this.project().technologies.length - 4));
  protected readonly categoryKey = computed(() => `category.${this.project().category}` as TranslationKey);
  protected readonly categoryIcon = computed(() => {
    switch (this.project().category) {
      case 'Mobile': return 'smartphone';
      case 'Api': return 'server';
      case 'Tool': return 'wrench';
      default: return 'code';
    }
  });
}
