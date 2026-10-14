import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models';

// HTTP hatasından kullanıcıya gösterilecek mesajı çıkarıyor
export function problemMessage(error: unknown, fallback = 'Bir hata oluştu.'): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as ProblemDetails | null;
    if (body?.detail) return body.detail;
    if (error.status === 0) return 'Sunucuya ulaşılamıyor.';
    if (error.status === 429) return 'Çok fazla istek gönderildi, biraz bekleyin.';
  }
  return fallback;
}

// 400 cevabındaki alan bazlı hatalar: { "title.tr": ["..."] }
export function problemFieldErrors(error: unknown): Record<string, string[]> {
  if (error instanceof HttpErrorResponse && error.status === 400) {
    return (error.error as ProblemDetails | null)?.errors ?? {};
  }
  return {};
}
