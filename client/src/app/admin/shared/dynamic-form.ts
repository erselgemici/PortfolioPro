import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AdminApi } from '../../core/api/admin-api.service';
import { problemMessage } from '../../core/api/problem';
import { ToastService } from '../../core/toast.service';
import { Icon } from '../../shared/icon';
import { FieldDef } from '../crud/crud-types';

const URL_PATTERN = /^(https?:\/\/\S+|\/uploads\/\S+)$/i;

@Component({
  selector: 'app-dynamic-form',
  imports: [ReactiveFormsModule, Icon],
  templateUrl: './dynamic-form.html',
  styleUrl: './dynamic-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicForm {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(AdminApi);
  private readonly toast = inject(ToastService);

  readonly fields = input.required<FieldDef[]>();
  readonly value = input<Record<string, unknown> | null>(null);
  readonly saving = input(false);
  readonly serverErrors = input<Record<string, string[]>>({});
  readonly submitLabel = input('Kaydet');
  readonly showCancel = input(true);

  readonly save = output<Record<string, unknown>>();
  readonly cancel = output<void>();

  // NOT: computed sayesinde fields ya da value değişince (listeden başka kayıt seçilince)
  // form otomatik olarak baştan kuruluyor
  protected readonly form = computed(() => this.buildForm(this.fields(), this.value() ?? {}));
  protected readonly uploading = signal<string | null>(null);

  // ---------- form oluşturma ----------
  private buildForm(fields: FieldDef[], value: Record<string, unknown>): FormGroup {
    const group: Record<string, AbstractControl> = {};

    for (const field of fields) {
      const raw = value[field.key];
      const validators = this.validatorsFor(field);

      if (field.localized) {
        const loc = (raw as { tr?: string; en?: string } | undefined) ?? {};
        group[field.key] = this.fb.group({
          tr: [loc.tr ?? '', validators],
          en: [loc.en ?? '', validators]
        });
      } else {
        group[field.key] = this.fb.control(this.toFormValue(field, raw), validators);
      }
    }

    return this.fb.group(group);
  }

  private validatorsFor(field: FieldDef): ValidatorFn[] {
    const v: ValidatorFn[] = [];
    if (field.required && field.type !== 'checkbox') v.push(Validators.required);
    if (field.maxLength) v.push(Validators.maxLength(field.maxLength));
    if (field.type === 'email') v.push(Validators.email);
    if (field.type === 'url' || field.type === 'upload') v.push(Validators.pattern(URL_PATTERN));
    return v;
  }

  // API'deki değeri input'un anlayacağı şekle çevir
  private toFormValue(field: FieldDef, raw: unknown): unknown {
    switch (field.type) {
      case 'tags':
        return Array.isArray(raw) ? raw.join(', ') : '';
      case 'month':
        return typeof raw === 'string' ? raw.slice(0, 7) : ''; // "2023-01-01" -> "2023-01"
      case 'checkbox':
        return !!raw;
      case 'number':
        return raw ?? null;
      default:
        return raw ?? '';
    }
  }

  // Input'taki değeri API'nin beklediği şekle çevir
  private toApiValue(field: FieldDef, raw: unknown): unknown {
    switch (field.type) {
      case 'tags':
        return String(raw ?? '')
          .split(',')
          .map((t) => t.trim())
          .filter(Boolean);
      case 'month':
        return raw ? `${raw}-01` : null;
      case 'date':
        return raw || null;
      case 'number':
        // NOT: Backend'de int alanlar null kabul etmiyor (JSON hatası verir), boşsa 0 gönderiyoruz
        if (raw === '' || raw === null || raw === undefined) return field.nullable ? null : 0;
        return Number(raw);
      case 'checkbox':
        return !!raw;
      case 'url':
      case 'upload':
        return String(raw ?? '').trim() || null;
      default:
        return typeof raw === 'string' ? raw.trim() : raw;
    }
  }

  // ---------- template yardımcıları ----------
  protected control(key: string, lang?: 'tr' | 'en'): AbstractControl | null {
    return lang ? this.form().get([key, lang]) : this.form().get(key);
  }

  protected invalid(key: string, lang?: 'tr' | 'en'): boolean {
    const c = this.control(key, lang);
    return !!c && c.invalid && (c.touched || c.dirty);
  }

  protected errorText(field: FieldDef, lang?: 'tr' | 'en'): string {
    const c = this.control(field.key, lang);
    if (c?.hasError('required')) return 'Bu alan zorunlu.';
    if (c?.hasError('maxlength')) return `En fazla ${field.maxLength} karakter.`;
    if (c?.hasError('pattern')) return 'Geçerli bir http/https adresi girin.';
    if (c?.hasError('email')) return 'Geçerli bir e-posta girin.';
    return '';
  }

  // Backend'den gelen 400 hataları: "title.tr", "endDate" gibi anahtarlar
  protected serverErrorFor(key: string): string | null {
    const errors = this.serverErrors();
    const match = Object.keys(errors).find((k) => k.toLowerCase().split('.')[0] === key.toLowerCase());
    return match ? errors[match][0] : null;
  }

  protected uploadValue(key: string): string {
    return (this.form().get(key)?.value as string) ?? '';
  }

  protected isImage(url: string): boolean {
    return /\.(jpe?g|png|webp)$/i.test(url);
  }

  // ---------- dosya yükleme ----------
  onFileSelected(field: FieldDef, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const file = inputEl.files?.[0];
    inputEl.value = ''; // aynı dosyayı tekrar seçebilmek için sıfırla
    if (!file) return;

    this.uploading.set(field.key);
    this.api.upload(file, field.uploadKind ?? 'Image').subscribe({
      next: ({ url }) => {
        this.form().get(field.key)?.setValue(url);
        this.form().get(field.key)?.markAsDirty();
        this.uploading.set(null);
        this.toast.success('Dosya yüklendi.');
      },
      error: (err: unknown) => {
        this.uploading.set(null);
        this.toast.error(problemMessage(err, 'Dosya yüklenemedi.'));
      }
    });
  }

  clearUpload(key: string): void {
    this.form().get(key)?.setValue('');
    this.form().get(key)?.markAsDirty();
  }

  // ---------- kaydet ----------
  submit(): void {
    if (this.form().invalid) {
      this.form().markAllAsTouched();
      this.toast.error('Formda eksik ya da hatalı alanlar var.');
      return;
    }

    const raw = this.form().getRawValue() as Record<string, unknown>;
    const result: Record<string, unknown> = {};

    for (const field of this.fields()) {
      if (field.localized) {
        const loc = raw[field.key] as { tr: string; en: string };
        result[field.key] = { tr: (loc.tr ?? '').trim(), en: (loc.en ?? '').trim() };
      } else {
        result[field.key] = this.toApiValue(field, raw[field.key]);
      }
    }

    this.save.emit(result);
  }
}
