import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PortfolioApi } from '../../core/api/portfolio-api.service';
import { Portfolio } from '../../core/models';
import { I18nService } from '../../core/i18n/i18n.service';
import { TranslatePipe } from '../../core/i18n/i18n.pipes';
import { SeoService } from '../../core/seo.service';
import { HeroSection } from './sections/hero-section';
import { AboutSection } from './sections/about-section';
import { SkillsSection } from './sections/skills-section';
import { ExperienceSection } from './sections/experience-section';
import { ProjectsSection } from './sections/projects-section';
import { EducationSection } from './sections/education-section';
import { ContactSection } from './sections/contact-section';

type LoadState = 'loading' | 'ready' | 'error';

@Component({
  selector: 'app-home',
  imports: [
    TranslatePipe,
    HeroSection,
    AboutSection,
    SkillsSection,
    ExperienceSection,
    ProjectsSection,
    EducationSection,
    ContactSection
  ],
  template: `
    @switch (state()) {
      @case ('loading') {
        <div class="container loading" aria-busy="true">
          <div class="skeleton" style="width: 180px; height: 26px"></div>
          <div class="skeleton" style="width: min(520px, 90%); height: 64px"></div>
          <div class="skeleton" style="width: min(420px, 80%); height: 22px"></div>
          <div class="skeleton" style="width: 260px; height: 44px; margin-top: 12px"></div>
          <span class="sr-only">{{ 'common.loading' | t }}</span>
        </div>
      }
      @case ('error') {
        <div class="container error-box">
          <p>{{ 'common.loadError' | t }}</p>
          <button type="button" class="btn" (click)="load()">{{ 'common.retry' | t }}</button>
        </div>
      }
      @case ('ready') {
        @if (data(); as d) {
          <app-hero-section [profile]="d.profile" [skills]="d.skills" />
          <app-about-section [profile]="d.profile" [projectCount]="d.featuredProjects.length" [skillCount]="d.skills.length" />
          @if (d.skills.length) {
            <app-skills-section [skills]="d.skills" />
          }
          @if (d.experiences.length) {
            <app-experience-section [experiences]="d.experiences" />
          }
          @if (d.featuredProjects.length) {
            <app-projects-section [projects]="d.featuredProjects" />
          }
          @if (d.educations.length || d.certificates.length) {
            <app-education-section [educations]="d.educations" [certificates]="d.certificates" />
          }
          <app-contact-section [profile]="d.profile" />
        }
      }
    }
  `,
  styles: `
    .loading { display: flex; flex-direction: column; gap: 16px; padding-block: 120px; }
    .error-box { display: flex; flex-direction: column; align-items: flex-start; gap: 16px; padding-block: 120px; color: var(--text-muted); }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Home {
  private readonly api = inject(PortfolioApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly i18n = inject(I18nService);
  private readonly seo = inject(SeoService);

  protected readonly state = signal<LoadState>('loading');
  protected readonly data = signal<Portfolio | null>(null);

  private readonly profile = computed(() => this.data()?.profile ?? null);

  constructor() {
    this.load();

    // Dil ya da profil değişince sekme başlığını güncelle
    effect(() => {
      const p = this.profile();
      if (!p) return;
      this.seo.set(`${p.fullName} · ${this.i18n.pick(p.title)}`, this.i18n.pick(p.headline));
    });
  }

  load(): void {
    this.state.set('loading');
    this.api
      .getPortfolio()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.data.set(data);
          this.state.set('ready');
        },
        error: () => {
          this.api.resetPortfolioCache();
          this.state.set('error');
        }
      });
  }
}
