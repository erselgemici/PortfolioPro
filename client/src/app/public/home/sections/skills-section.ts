import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Skill, SkillCategory } from '../../../core/models';
import { TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { TranslationKey } from '../../../core/i18n/translations';
import { Icon } from '../../../shared/icon';
import { RevealDirective } from '../../../shared/reveal.directive';

const CATEGORY_ORDER: SkillCategory[] = ['Backend', 'Frontend', 'Database', 'Mobile', 'DevOps', 'Tools'];
const CATEGORY_ICONS: Record<SkillCategory, string> = {
  Backend: 'server',
  Frontend: 'layout',
  Database: 'database',
  Mobile: 'smartphone',
  DevOps: 'cloud',
  Tools: 'wrench'
};

@Component({
  selector: 'app-skills-section',
  imports: [TranslatePipe, Icon, RevealDirective],
  template: `
    <section id="skills" class="section">
      <div class="container">
        <div class="section-head" appReveal>
          <span class="kicker">{{ 'skills.kicker' | t }}</span>
          <h2>{{ 'skills.title' | t }}</h2>
          <p>{{ 'skills.subtitle' | t }}</p>
        </div>

        <div class="grid">
          @for (group of groups(); track group.category) {
            <div class="card group" appReveal>
              <div class="group-head">
                <span class="icon"><app-icon [name]="group.icon" /></span>
                <h3>{{ group.labelKey | t }}</h3>
              </div>
              <div class="chips">
                @for (skill of group.skills; track skill.id) {
                  <span class="chip" [class.chip-accent]="skill.isHighlighted">{{ skill.name }}</span>
                }
              </div>
            </div>
          }
        </div>
      </div>
    </section>
  `,
  styles: `
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 16px; }
    @media (max-width: 400px) { .grid { grid-template-columns: 1fr; } }
    .group { padding: 22px; transition: border-color 0.2s; }
    .group:hover { border-color: var(--border-strong); }
    .group-head { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
    .icon { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; background: var(--accent-soft); color: var(--accent); }
    .icon app-icon { width: 18px; height: 18px; }
    h3 { font-size: 1rem; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SkillsSection {
  readonly skills = input<Skill[]>([]);

  // Düz listeyi kategorilere göre grupluyoruz, boş kategoriler gösterilmiyor
  protected readonly groups = computed(() =>
    CATEGORY_ORDER.map((category) => ({
      category,
      icon: CATEGORY_ICONS[category],
      labelKey: `skills.${category}` as TranslationKey,
      skills: this.skills().filter((s) => s.category === category)
    })).filter((g) => g.skills.length > 0)
  );
}
