import { Injectable, inject, signal } from '@angular/core';
import { AdminApi } from '../core/api/admin-api.service';

// Admin paneli genelinde paylaşılan küçük durum: okunmamış mesaj sayısı (sidebar'daki rozet)
@Injectable({ providedIn: 'root' })
export class AdminState {
  private readonly api = inject(AdminApi);
  readonly unreadMessages = signal(0);

  refreshUnread(): void {
    this.api.messages(true).subscribe({
      next: (list) => this.unreadMessages.set(list.length),
      error: () => {
        /* rozet gösterilemezse sorun değil */
      }
    });
  }
}
