import { DOCUMENT, Injectable, computed, effect, inject, signal } from '@angular/core';

export type ThemePreference = 'light' | 'dark' | 'system';
const STORAGE_KEY = 'pp-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly media = window.matchMedia('(prefers-color-scheme: dark)');

  readonly preference = signal<ThemePreference>(this.readPreference());
  private readonly systemDark = signal(this.media.matches);

  // "system" seçiliyse işletim sisteminin temasına bakıyoruz
  readonly resolved = computed<'light' | 'dark'>(() => {
    const pref = this.preference();
    if (pref === 'system') return this.systemDark() ? 'dark' : 'light';
    return pref;
  });

  constructor() {
    // Kullanıcı işletim sisteminde temayı değiştirirse anında uyum sağla
    this.media.addEventListener('change', (e) => this.systemDark.set(e.matches));

    effect(() => {
      const theme = this.resolved();
      this.document.documentElement.setAttribute('data-theme', theme);
      this.document
        .querySelector('meta[name="theme-color"]')
        ?.setAttribute('content', theme === 'dark' ? '#0b0d10' : '#f7f7f5');
    });
  }

  toggle(): void {
    const next = this.resolved() === 'dark' ? 'light' : 'dark';
    this.preference.set(next);
    try {
      localStorage.setItem(STORAGE_KEY, next);
    } catch {
      /* yok say */
    }
  }

  private readPreference(): ThemePreference {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved === 'light' || saved === 'dark' || saved === 'system') return saved;
    } catch {
      /* yok say */
    }
    // NOT: İlk girişte koyu tema. Kullanıcı açık temayı seçerse localStorage'da saklanıyor.
    return 'dark';
  }
}
