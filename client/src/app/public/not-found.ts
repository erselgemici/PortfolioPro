import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../core/i18n/i18n.pipes';
import { Icon } from '../shared/icon';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink, TranslatePipe, Icon],
  template: `
    <section class="section">
      <div class="container box">
        <p class="code mono">404</p>
        <h1>{{ 'notFound.title' | t }}</h1>
        <p class="muted">{{ 'notFound.text' | t }}</p>
        <a routerLink="/" class="btn btn-primary"><app-icon name="arrow-left" /> {{ 'notFound.home' | t }}</a>
      </div>
    </section>
  `,
  styles: `
    .box { display: flex; flex-direction: column; align-items: flex-start; gap: 16px; padding-block: 40px; }
    .code { font-size: 4rem; font-weight: 700; color: var(--accent); line-height: 1; }
    h1 { font-size: 2rem; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotFound {}
