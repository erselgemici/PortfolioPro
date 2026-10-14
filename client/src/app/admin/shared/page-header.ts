import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  template: `
    <div class="head">
      <div>
        <h1>{{ title() }}</h1>
        @if (description()) {
          <p class="muted">{{ description() }}</p>
        }
      </div>
      <div class="actions"><ng-content /></div>
    </div>
  `,
  styles: `
    .head { display: flex; align-items: flex-end; justify-content: space-between; gap: 16px; flex-wrap: wrap; margin-bottom: 24px; }
    h1 { font-size: 1.6rem; }
    p { margin-top: 6px; font-size: 0.92rem; max-width: 620px; }
    .actions { display: flex; gap: 8px; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly description = input('');
}
