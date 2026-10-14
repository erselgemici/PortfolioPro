import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { AdminApi } from '../../core/api/admin-api.service';
import { problemFieldErrors, problemMessage } from '../../core/api/problem';
import { ConfirmService } from '../../core/confirm.service';
import { ToastService } from '../../core/toast.service';
import { Icon } from '../../shared/icon';
import { DynamicForm } from '../shared/dynamic-form';
import { PageHeader } from '../shared/page-header';
import { CrudConfig } from './crud-types';

type Row = Record<string, unknown> & { id: string };

// Projeler, deneyimler, eğitim, yetenekler ve sertifikalar sayfalarının ortak component'i.
// Hangi kaynakla çalışacağını route'taki config'den öğreniyor (bkz. admin.routes.ts).
@Component({
  selector: 'app-crud-page',
  imports: [Icon, DynamicForm, PageHeader],
  templateUrl: './crud-page.html',
  styleUrl: './crud-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CrudPage {
  private readonly api = inject(AdminApi);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  protected readonly config = inject(ActivatedRoute).snapshot.data['config'] as CrudConfig;

  protected readonly rows = signal<Row[]>([]);
  protected readonly loading = signal(true);

  // Drawer (sağdan açılan form paneli) durumu
  protected readonly editing = signal<Row | null>(null);
  protected readonly drawerOpen = signal(false);
  protected readonly formValue = signal<Record<string, unknown> | null>(null);
  protected readonly saving = signal(false);
  protected readonly serverErrors = signal<Record<string, string[]>>({});

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list<Row>(this.config.resource).subscribe({
      next: (rows) => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.toast.error(problemMessage(err, 'Liste yüklenemedi.'));
      }
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.formValue.set(this.config.defaults());
    this.serverErrors.set({});
    this.drawerOpen.set(true);
  }

  openEdit(row: Row): void {
    this.editing.set(row);
    this.formValue.set({ ...row });
    this.serverErrors.set({});
    this.drawerOpen.set(true);
  }

  close(): void {
    this.drawerOpen.set(false);
  }

  save(value: Record<string, unknown>): void {
    const current = this.editing();
    this.saving.set(true);
    this.serverErrors.set({});

    const request$: Observable<unknown> = current
      ? this.api.update(this.config.resource, current.id, value)
      : this.api.create(this.config.resource, value);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.toast.success(current ? `${this.config.singular} güncellendi.` : `${this.config.singular} eklendi.`);
        this.load();
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.serverErrors.set(problemFieldErrors(err));
        this.toast.error(problemMessage(err, 'Kaydedilemedi.'));
      }
    });
  }

  async remove(row: Row): Promise<void> {
    const ok = await this.confirm.ask({
      title: `${this.config.singular} silinsin mi?`,
      message: 'Bu işlem geri alınamaz.',
      confirmText: 'Sil',
      danger: true
    });
    if (!ok) return;

    this.api.remove(this.config.resource, row.id).subscribe({
      next: () => {
        // Listeyi tekrar çekmeden satırı çıkarıyoruz (optimistic değil, cevap geldikten sonra)
        this.rows.update((list) => list.filter((r) => r.id !== row.id));
        this.toast.success(`${this.config.singular} silindi.`);
      },
      error: (err: unknown) => this.toast.error(problemMessage(err, 'Silinemedi.'))
    });
  }
}
