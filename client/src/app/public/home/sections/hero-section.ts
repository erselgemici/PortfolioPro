import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Profile, Skill } from '../../../core/models';
import { I18nService } from '../../../core/i18n/i18n.service';
import { LocalizePipe, TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { Icon } from '../../../shared/icon';

@Component({
  selector: 'app-hero-section',
  imports: [RouterLink, Icon, TranslatePipe, LocalizePipe],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeroSection {
  private readonly i18n = inject(I18nService);

  readonly profile = input<Profile | null>(null);
  readonly skills = input<Skill[]>([]);

  // Kod penceresinde gösterilecek teknolojiler: öne çıkanlar, en fazla 5 tane
  protected readonly stack = computed(() =>
    this.skills()
      .filter((s) => s.isHighlighted)
      .slice(0, 5)
      .map((s) => s.name)
  );

  // Seçili dile göre CV linki, o dilde yoksa diğeri
  protected readonly cvUrl = computed(() => {
    const p = this.profile();
    if (!p) return null;
    return this.i18n.lang() === 'tr' ? p.cvUrlTr || p.cvUrlEn : p.cvUrlEn || p.cvUrlTr;
  });
}
