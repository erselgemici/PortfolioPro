import { ChangeDetectionStrategy, Component, ElementRef, effect, inject, viewChild } from '@angular/core';
import { ConfirmService } from '../core/confirm.service';

@Component({
  selector: 'app-confirm-host',
  template: `
    <!-- NOT: Native <dialog> elemanı. showModal() ile açılınca odak otomatik içeride kalıyor,
         Esc ile kapanıyor ve arka plan tıklanamıyor. Bunları elle yazmaya gerek kalmıyor. -->
    <dialog #dialog class="dialog" (cancel)="answer(false)">
      @if (confirm.current(); as req) {
        <h3>{{ req.title }}</h3>
        <p>{{ req.message }}</p>
        <div class="actions">
          <button type="button" class="btn btn-sm" (click)="answer(false)">Vazgeç</button>
          <button type="button" class="btn btn-sm" [class.btn-danger]="req.danger" [class.btn-primary]="!req.danger"
            (click)="answer(true)">{{ req.confirmText }}</button>
        </div>
      }
    </dialog>
  `,
  styles: `
    .dialog {
      width: min(420px, calc(100vw - 32px)); padding: 24px; border-radius: 16px;
      border: 1px solid var(--border); background: var(--bg-elev); color: var(--text); box-shadow: var(--shadow);
    }
    .dialog::backdrop { background: rgba(0, 0, 0, 0.45); backdrop-filter: blur(2px); }
    h3 { font-size: 1.1rem; margin-bottom: 8px; }
    p { color: var(--text-muted); font-size: 0.93rem; }
    .actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 22px; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmHost {
  protected readonly confirm = inject(ConfirmService);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const el = this.dialog().nativeElement;
      if (this.confirm.current()) {
        if (!el.open) el.showModal();
      } else if (el.open) {
        el.close();
      }
    });
  }

  answer(ok: boolean): void {
    this.confirm.current()?.resolve(ok);
  }
}
