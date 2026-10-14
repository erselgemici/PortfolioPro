import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AdminApi } from '../../core/api/admin-api.service';
import { problemFieldErrors, problemMessage } from '../../core/api/problem';
import { Profile } from '../../core/models';
import { ToastService } from '../../core/toast.service';
import { FieldDef } from '../crud/crud-types';
import { DynamicForm } from '../shared/dynamic-form';
import { PageHeader } from '../shared/page-header';

@Component({
  selector: 'app-profile-page',
  imports: [DynamicForm, PageHeader],
  template: `
    <app-page-header title="Profil" description="Hero, hakkımda ve iletişim bölümlerinde gösterilen bilgiler." />

    <div class="card body">
      @if (value(); as v) {
        <app-dynamic-form
          [fields]="fields"
          [value]="v"
          [saving]="saving()"
          [serverErrors]="serverErrors()"
          [showCancel]="false"
          submitLabel="Profili kaydet"
          (save)="save($event)" />
      } @else {
        <div class="skeleton" style="height: 400px"></div>
      }
    </div>
  `,
  styles: `.body { padding: 24px; } @media (max-width: 640px) { .body { padding: 16px; } }`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfilePage {
  private readonly api = inject(AdminApi);
  private readonly toast = inject(ToastService);

  protected readonly value = signal<Record<string, unknown> | null>(null);
  protected readonly saving = signal(false);
  protected readonly serverErrors = signal<Record<string, string[]>>({});

  protected readonly fields: FieldDef[] = [
    { key: 'fullName', label: 'Ad soyad', type: 'text', required: true, maxLength: 100 },
    { key: 'email', label: 'Herkese açık e-posta', type: 'email', required: true, maxLength: 150 },
    { key: 'title', label: 'Ünvan', type: 'text', localized: true, required: true, maxLength: 100, full: true },
    { key: 'headline', label: 'Kısa tanıtım (hero)', type: 'textarea', localized: true, required: true, maxLength: 300, full: true },
    { key: 'about', label: 'Hakkımda', type: 'textarea', localized: true, required: true, maxLength: 4000, full: true, hint: 'Paragrafları boş satırla ayır.' },
    { key: 'location', label: 'Konum', type: 'text', required: true, maxLength: 100 },
    { key: 'yearsOfExperience', label: 'Deneyim (yıl)', type: 'number' },
    { key: 'githubUrl', label: 'GitHub', type: 'url' },
    { key: 'linkedinUrl', label: 'LinkedIn', type: 'url' },
    { key: 'avatarUrl', label: 'Fotoğraf', type: 'upload', uploadKind: 'Image', full: true, hint: 'Dikey (4:5) bir fotoğraf önerilir. En fazla 2 MB.' },
    { key: 'cvUrlTr', label: 'CV (Türkçe, PDF)', type: 'upload', uploadKind: 'Document', full: true },
    { key: 'cvUrlEn', label: 'CV (İngilizce, PDF)', type: 'upload', uploadKind: 'Document', full: true },
    { key: 'isOpenToWork', label: '"Yeni fırsatlara açığım" rozetini göster', type: 'checkbox' }
  ];

  constructor() {
    this.api.getProfile().subscribe({
      next: (p) => this.value.set({ ...p }),
      error: (err: unknown) => this.toast.error(problemMessage(err, 'Profil yüklenemedi.'))
    });
  }

  save(value: Record<string, unknown>): void {
    this.saving.set(true);
    this.serverErrors.set({});

    this.api.updateProfile(value as unknown as Profile).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Profil kaydedildi.');
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.serverErrors.set(problemFieldErrors(err));
        this.toast.error(problemMessage(err, 'Profil kaydedilemedi.'));
      }
    });
  }
}
