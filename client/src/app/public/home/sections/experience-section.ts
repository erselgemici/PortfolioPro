import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Experience } from '../../../core/models';
import { LocalizePipe, TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { MonthYearPipe } from '../../../shared/date.pipe';
import { RevealDirective } from '../../../shared/reveal.directive';

@Component({
  selector: 'app-experience-section',
  imports: [TranslatePipe, LocalizePipe, MonthYearPipe, RevealDirective],
  template: `
    <section id="experience" class="section">
      <div class="container">
        <div class="section-head" appReveal>
          <span class="kicker">{{ 'experience.kicker' | t }}</span>
          <h2>{{ 'experience.title' | t }}</h2>
        </div>

        <ol class="timeline">
          @for (item of experiences(); track item.id) {
            <li class="item" appReveal>
              <div class="dates mono">
                {{ item.startDate | monthYear: 'year' }} —
                @if (item.endDate) {
                  {{ item.endDate | monthYear: 'year' }}
                } @else {
                  <span class="current">{{ 'experience.present' | t }}</span>
                }
              </div>

              <div class="content">
                <h3>{{ item.position | loc }}</h3>
                <p class="company">
                  {{ item.company }}
                  @if (item.location) {
                    <span class="sep">·</span> {{ item.location }}
                  }
                </p>
                <p class="description">{{ item.description | loc }}</p>
                @if (item.technologies.length) {
                  <div class="chips">
                    @for (tech of item.technologies; track tech) {
                      <span class="chip">{{ tech }}</span>
                    }
                  </div>
                }
              </div>
            </li>
          }
        </ol>
      </div>
    </section>
  `,
  styles: `
    .timeline { list-style: none; margin: 0; padding: 0; border-left: 1px solid var(--border); margin-left: 6px; }
    .item { position: relative; display: grid; grid-template-columns: 150px minmax(0, 1fr); gap: 24px; padding: 0 0 44px 32px; }
    .item:last-child { padding-bottom: 0; }
    .item::before {
      content: ''; position: absolute; left: -6px; top: 6px; width: 11px; height: 11px; border-radius: 50%;
      background: var(--bg); border: 2px solid var(--accent);
    }
    .dates { font-size: 0.82rem; color: var(--text-muted); padding-top: 2px; }
    .current { color: var(--accent); }
    h3 { font-size: 1.15rem; }
    .company { margin-top: 4px; color: var(--text-muted); font-weight: 500; }
    .sep { margin: 0 4px; color: var(--text-faint); }
    .description { margin-top: 12px; color: var(--text-muted); white-space: pre-line; }
    .chips { margin-top: 14px; }
    @media (max-width: 720px) { .item { grid-template-columns: 1fr; gap: 6px; padding-left: 24px; } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExperienceSection {
  readonly experiences = input<Experience[]>([]);
}
