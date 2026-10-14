import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { Profile } from '../../../core/models';
import { I18nService } from '../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { RevealDirective } from '../../../shared/reveal.directive';

@Component({
  selector: 'app-about-section',
  imports: [TranslatePipe, RevealDirective],
  template: `
    <section id="about" class="section">
      <div class="container about" appReveal>
        <div class="photo">
          @if (profile()?.avatarUrl; as avatar) {
            <img [src]="avatar" [alt]="profile()?.fullName" width="320" height="380" loading="lazy" />
          } @else {
            <div class="monogram mono">{{ initials() }}</div>
          }
        </div>

        <div class="text">
          <div class="section-head">
            <span class="kicker">{{ 'about.kicker' | t }}</span>
            <h2>{{ 'about.title' | t }}</h2>
          </div>

          <!-- Hakkımda metni boş satırlarla paragraflara bölünüyor -->
          @for (paragraph of paragraphs(); track $index) {
            <p class="paragraph">{{ paragraph }}</p>
          }

          <dl class="stats">
            <div>
              <dt>{{ 'about.years' | t }}</dt>
              <dd>{{ profile()?.yearsOfExperience }}+</dd>
            </div>
            <div>
              <dt>{{ 'about.projects' | t }}</dt>
              <dd>{{ projectCount() }}</dd>
            </div>
            <div>
              <dt>{{ 'about.tech' | t }}</dt>
              <dd>{{ skillCount() }}</dd>
            </div>
          </dl>
        </div>
      </div>
    </section>
  `,
  styles: `
    .about { display: grid; grid-template-columns: 320px minmax(0, 1fr); gap: 64px; align-items: start; }
    @media (max-width: 860px) { .about { grid-template-columns: 1fr; gap: 36px; } .photo { max-width: 260px; } }
    .photo img, .monogram {
      width: 100%; aspect-ratio: 4 / 5; border-radius: var(--radius-lg); object-fit: cover;
      border: 1px solid var(--border);
    }
    .monogram {
      display: grid; place-items: center; font-size: 4rem; font-weight: 700; color: var(--accent);
      background: radial-gradient(circle at 30% 20%, var(--accent-soft), transparent 60%), var(--bg-elev);
    }
    .section-head { margin-bottom: 24px; }
    .paragraph { color: var(--text-muted); font-size: 1.04rem; line-height: 1.8; }
    .paragraph + .paragraph { margin-top: 16px; }
    .stats {
      display: grid; grid-template-columns: repeat(3, 1fr); margin: 36px 0 0;
      border: 1px solid var(--border); border-radius: var(--radius); background: var(--bg-elev);
    }
    .stats > div { padding: 18px 20px; }
    .stats > div + div { border-left: 1px solid var(--border); }
    dt { font-size: 0.8rem; color: var(--text-muted); }
    dd { margin: 4px 0 0; font-size: 1.8rem; font-weight: 650; letter-spacing: -0.02em; }
    @media (max-width: 480px) { .stats > div { padding: 14px 12px; } dd { font-size: 1.4rem; } dt { font-size: 0.72rem; } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AboutSection {
  private readonly i18n = inject(I18nService);

  readonly profile = input<Profile | null>(null);
  readonly projectCount = input(0);
  readonly skillCount = input(0);

  protected readonly paragraphs = computed(() =>
    this.i18n
      .pick(this.profile()?.about)
      .split(/\n\s*\n/)
      .map((p) => p.trim())
      .filter(Boolean)
  );

  protected readonly initials = computed(() =>
    (this.profile()?.fullName ?? '')
      .split(' ')
      .filter(Boolean)
      .map((w) => w[0])
      .slice(0, 2)
      .join('')
      .toUpperCase()
  );
}
