import { Injectable, signal } from '@angular/core';

export interface ConfirmRequest {
  title: string;
  message: string;
  confirmText: string;
  danger: boolean;
  resolve: (ok: boolean) => void;
}

// window.confirm() yerine sitenin tasarımına uyan onay penceresi.
// Kullanımı: if (await confirm.ask({...})) { sil }
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  readonly current = signal<ConfirmRequest | null>(null);

  ask(options: { title: string; message: string; confirmText?: string; danger?: boolean }): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.current.set({
        title: options.title,
        message: options.message,
        confirmText: options.confirmText ?? 'Onayla',
        danger: options.danger ?? false,
        resolve: (ok) => {
          this.current.set(null);
          resolve(ok);
        }
      });
    });
  }
}
