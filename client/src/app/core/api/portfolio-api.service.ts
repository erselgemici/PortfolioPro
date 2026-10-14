import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { ContactRequest, Portfolio, ProjectDetail, ProjectListItem } from '../models';

// Ziyaretçi tarafının (public) API istekleri
@Injectable({ providedIn: 'root' })
export class PortfolioApi {
  private readonly http = inject(HttpClient);

  // NOT: shareReplay(1) ile cevap saklanıyor. Ana sayfa -> proje detayı -> ana sayfa geçişinde
  // aynı veri için tekrar istek atılmıyor. Hata olursa cache'i sıfırlıyoruz ki "tekrar dene" çalışsın.
  private portfolio$?: Observable<Portfolio>;

  getPortfolio(): Observable<Portfolio> {
    this.portfolio$ ??= this.http.get<Portfolio>('/api/portfolio').pipe(shareReplay(1));
    return this.portfolio$;
  }

  resetPortfolioCache(): void {
    this.portfolio$ = undefined;
  }

  getProjects(): Observable<ProjectListItem[]> {
    return this.http.get<ProjectListItem[]>('/api/projects');
  }

  getProject(slug: string): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`/api/projects/${encodeURIComponent(slug)}`);
  }

  sendMessage(body: ContactRequest): Observable<void> {
    return this.http.post<void>('/api/contact', body);
  }
}
