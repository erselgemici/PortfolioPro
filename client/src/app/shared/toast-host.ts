import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../core/toast.service';
import { Icon } from './icon';

@Component({
  selector: 'app-toast-host',
  imports: [Icon],
  template: `
    <div class="toasts" aria-live="polite">
      @for (toast of toasts.toasts(); track toast.id) {
        <div class="toast" [class]="'toast toast-' + toast.type" role="status">
          <app-icon [name]="toast.type === 'success' ? 'check' : toast.type === 'error' ? 'alert' : 'info'" />
          <span>{{ toast.text }}</span>
          <button type="button" class="close" (click)="toasts.dismiss(toast.id)" aria-label="Kapat">
            <app-icon name="x" />
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts {
      position: fixed; right: 16px; bottom: 16px; z-index: 1000;
      display: flex; flex-direction: column; gap: 8px; max-width: min(380px, calc(100vw - 32px));
    }
    .toast {
      display: flex; align-items: flex-start; gap: 10px;
      padding: 12px 12px 12px 14px; border-radius: 12px;
      background: var(--bg-elev); border: 1px solid var(--border); box-shadow: var(--shadow);
      font-size: 0.9rem; animation: slide-in 0.25s var(--ease);
    }
    .toast > app-icon { width: 18px; height: 18px; margin-top: 2px; }
    .toast-success > app-icon { color: var(--success); }
    .toast-error > app-icon { color: var(--danger); }
    .toast-info > app-icon { color: var(--accent); }
    span { flex: 1; }
    .close { border: 0; background: none; color: var(--text-faint); padding: 2px; }
    .close app-icon { width: 16px; height: 16px; }
    @keyframes slide-in { from { opacity: 0; transform: translateY(8px); } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ToastHost {
  protected readonly toasts = inject(ToastService);
}
