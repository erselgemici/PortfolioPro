import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Certificate, Education } from '../../../core/models';
import { LocalizePipe, TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { Icon } from '../../../shared/icon';
import { MonthYearPipe } from '../../../shared/date.pipe';
import { RevealDirective } from '../../../shared/reveal.directive';

@Component({
  selector: 'app-education-section',
  imports: [TranslatePipe, LocalizePipe, Icon, MonthYearPipe, RevealDirective],
  template: `
    <section id="education" class="section">
      <div class="container">
        <div class="section-head" appReveal>
          <span class="kicker">{{ 'education.kicker' | t }}</span>
          <h2>{{ 'education.title' | t }}</h2>
        </div>

        <div class="columns">
          @if (educations().length) {
            <div appReveal>
              <h3 class="col-title"><app-icon name="graduation" /> {{ 'education.education' | t }}</h3>
              <ul class="list">
                @for (e of educations(); track e.id) {
                  <li class="card entry">
                    <div class="entry-top">
                      <strong>{{ e.school }}</strong>
                      <span class="mono years">{{ e.startYear }} — {{ e.endYear ?? '…' }}</span>
                    </div>
                    <p class="muted">{{ e.department | loc }} · {{ e.degree | loc }}</p>
                    @if (e.description | loc; as desc) {
                      <p class="desc">{{ desc }}</p>
                    }
                  </li>
                }
              </ul>
            </div>
          }

          @if (certificates().length) {
            <div appReveal>
              <h3 class="col-title"><app-icon name="award" /> {{ 'education.certificates' | t }}</h3>
              <ul class="list">
                @for (c of certificates(); track c.id) {
                  <li class="card entry">
                    <div class="entry-top">
                      <strong>{{ c.title }}</strong>
                      <span class="mono years">{{ c.issuedAt | monthYear: 'year' }}</span>
                    </div>
                    <p class="muted">
                      {{ c.issuer }}
                      @if (c.credentialUrl) {
                        · <a [href]="c.credentialUrl" target="_blank" rel="noopener" class="view">{{ 'education.view' | t }}</a>
                      }
                    </p>
                  </li>
                }
              </ul>
            </div>
          }
        </div>
      </div>
    </section>
  `,
  styles: `
    .columns { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 32px; }
    @media (max-width: 400px) { .columns { grid-template-columns: 1fr; } }
    .col-title { display: flex; align-items: center; gap: 10px; font-size: 1rem; margin-bottom: 16px; color: var(--text-muted); }
    .col-title app-icon { color: var(--accent); }
    .list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 12px; }
    .entry { padding: 18px 20px; }
    .entry-top { display: flex; justify-content: space-between; gap: 12px; align-items: baseline; }
    .years { font-size: 0.78rem; color: var(--text-faint); white-space: nowrap; }
    .muted { margin-top: 4px; font-size: 0.92rem; }
    .desc { margin-top: 8px; font-size: 0.9rem; color: var(--text-muted); }
    .view { color: var(--accent); font-weight: 550; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EducationSection {
  readonly educations = input<Education[]>([]);
  readonly certificates = input<Certificate[]>([]);
}
