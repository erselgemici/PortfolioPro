import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AdminApi } from '../../core/api/admin-api.service';
import { problemMessage } from '../../core/api/problem';
import { ConfirmService } from '../../core/confirm.service';
import { Message } from '../../core/models';
import { ToastService } from '../../core/toast.service';
import { Icon } from '../../shared/icon';
import { AdminState } from '../admin-state.service';
import { PageHeader } from '../shared/page-header';

@Component({
  selector: 'app-messages-page',
  imports: [DatePipe, Icon, PageHeader],
  template: `
    <app-page-header title="Mesajlar" description="İletişim formundan gelen mesajlar.">
      <div class="tabs">
        <button type="button" class="tab" [class.active]="!onlyUnread()" (click)="onlyUnread.set(false)">Tümü</button>
        <button type="button" class="tab" [class.active]="onlyUnread()" (click)="onlyUnread.set(true)">
          Okunmamış ({{ unreadCount() }})
        </button>
      </div>
    </app-page-header>

    <div class="list">
      @for (m of visible(); track m.id) {
        <article class="card msg" [class.unread]="!m.isRead" [class.expanded]="openId() === m.id">
          <button type="button" class="msg-head" (click)="toggle(m)" [attr.aria-expanded]="openId() === m.id">
            <span class="dot"></span>
            <span class="who">
              <strong>{{ m.name }}</strong>
              <span class="muted mono">{{ m.email }}</span>
            </span>
            <span class="subject">{{ m.subject || (m.body.length > 70 ? m.body.slice(0, 70) + '…' : m.body) }}</span>
            <span class="date mono">{{ m.createdAt | date: 'dd.MM.yyyy HH:mm' }}</span>
          </button>

          @if (openId() === m.id) {
            <div class="msg-body">
              @if (m.subject) {
                <p class="subject-full"><strong>{{ m.subject }}</strong></p>
              }
              <p class="text">{{ m.body }}</p>
              <div class="actions">
                <a class="btn btn-sm btn-primary" [href]="replyLink(m)"><app-icon name="mail" /> Yanıtla</a>
                <button type="button" class="btn btn-sm" (click)="setRead(m, !m.isRead)">
                  {{ m.isRead ? 'Okunmadı işaretle' : 'Okundu işaretle' }}
                </button>
                <button type="button" class="btn btn-sm btn-ghost danger" (click)="remove(m)"><app-icon name="trash" /> Sil</button>
              </div>
            </div>
          }
        </article>
      } @empty {
        <div class="card empty">
          <app-icon name="inbox" />
          <p>{{ loading() ? 'Yükleniyor...' : 'Gösterilecek mesaj yok.' }}</p>
        </div>
      }
    </div>
  `,
  styles: `
    .tabs { display: inline-flex; padding: 3px; border-radius: 10px; background: var(--bg-subtle); border: 1px solid var(--border); }
    .tab { border: 0; background: none; padding: 6px 12px; border-radius: 8px; font-size: 0.85rem; font-weight: 500; color: var(--text-muted); }
    .tab.active { background: var(--bg-elev); color: var(--text); box-shadow: var(--shadow-sm); }
    .list { display: flex; flex-direction: column; gap: 8px; }
    .msg { overflow: hidden; }
    .msg-head {
      width: 100%; display: grid; grid-template-columns: 10px 220px minmax(0, 1fr) auto; align-items: center; gap: 14px;
      padding: 14px 18px; border: 0; background: none; text-align: left;
    }
    .msg-head:hover { background: var(--bg-subtle); }
    .dot { width: 8px; height: 8px; border-radius: 50%; }
    .unread .dot { background: var(--accent); }
    .who { display: flex; flex-direction: column; min-width: 0; }
    .who .muted { font-size: 0.75rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .unread .who strong { color: var(--text); }
    .subject { color: var(--text-muted); font-size: 0.9rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .unread .subject { color: var(--text); font-weight: 500; }
    .date { font-size: 0.75rem; color: var(--text-faint); }
    .msg-body { padding: 4px 18px 18px 42px; border-top: 1px solid var(--border); }
    .subject-full { margin-top: 14px; }
    .text { margin-top: 10px; white-space: pre-wrap; line-height: 1.7; }
    .actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 18px; }
    .danger { color: var(--danger); }
    .empty { display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 48px; color: var(--text-muted); }
    .empty app-icon { width: 28px; height: 28px; }
    @media (max-width: 720px) {
      .msg-head { grid-template-columns: 10px minmax(0, 1fr) auto; }
      .subject { grid-column: 2 / -1; }
      .msg-body { padding-left: 18px; }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MessagesPage {
  private readonly api = inject(AdminApi);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly state = inject(AdminState);

  protected readonly messages = signal<Message[]>([]);
  protected readonly loading = signal(true);
  protected readonly onlyUnread = signal(false);
  protected readonly openId = signal<string | null>(null);

  protected readonly unreadCount = computed(() => this.messages().filter((m) => !m.isRead).length);
  protected readonly visible = computed(() =>
    this.onlyUnread() ? this.messages().filter((m) => !m.isRead) : this.messages()
  );

  constructor() {
    this.api.messages().subscribe({
      next: (list) => {
        this.messages.set(list);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.toast.error(problemMessage(err, 'Mesajlar yüklenemedi.'));
      }
    });
  }

  toggle(m: Message): void {
    const opening = this.openId() !== m.id;
    this.openId.set(opening ? m.id : null);
    // Mesaj açılınca otomatik okundu say
    if (opening && !m.isRead) this.setRead(m, true);
  }

  setRead(m: Message, value: boolean): void {
    this.api.setMessageRead(m.id, value).subscribe({
      next: () => {
        this.messages.update((list) => list.map((x) => (x.id === m.id ? { ...x, isRead: value } : x)));
        this.state.unreadMessages.set(this.unreadCount());
      },
      error: (err: unknown) => this.toast.error(problemMessage(err))
    });
  }

  async remove(m: Message): Promise<void> {
    const ok = await this.confirm.ask({
      title: 'Mesaj silinsin mi?',
      message: `${m.name} tarafından gönderilen mesaj kalıcı olarak silinecek.`,
      confirmText: 'Sil',
      danger: true
    });
    if (!ok) return;

    this.api.deleteMessage(m.id).subscribe({
      next: () => {
        this.messages.update((list) => list.filter((x) => x.id !== m.id));
        this.state.unreadMessages.set(this.unreadCount());
        this.toast.success('Mesaj silindi.');
      },
      error: (err: unknown) => this.toast.error(problemMessage(err))
    });
  }

  replyLink(m: Message): string {
    const subject = encodeURIComponent(`Re: ${m.subject || 'Portfolyo mesajın'}`);
    return `mailto:${m.email}?subject=${subject}`;
  }
}
