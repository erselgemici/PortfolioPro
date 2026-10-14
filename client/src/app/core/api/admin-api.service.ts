import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Dashboard, Message, Profile } from '../models';

export type AdminResource = 'projects' | 'experiences' | 'educations' | 'skills' | 'certificates';
export type UploadKind = 'Image' | 'Document';

// Admin panelinin API istekleri. Token'ı interceptor ekliyor, burada hiç token kodu yok.
@Injectable({ providedIn: 'root' })
export class AdminApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/admin';

  dashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.base}/dashboard`);
  }

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>(`${this.base}/profile`);
  }

  updateProfile(body: Profile): Observable<void> {
    return this.http.put<void>(`${this.base}/profile`, body);
  }

  // ---- Generic CRUD: projeler, deneyimler, eğitim, yetenekler, sertifikalar aynı yapıda ----
  list<T>(resource: AdminResource): Observable<T[]> {
    return this.http.get<T[]>(`${this.base}/${resource}`);
  }

  create(resource: AdminResource, body: unknown): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/${resource}`, body);
  }

  update(resource: AdminResource, id: string, body: unknown): Observable<string> {
    return this.http.put<string>(`${this.base}/${resource}/${id}`, body);
  }

  remove(resource: AdminResource, id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${resource}/${id}`);
  }

  // ---- Mesajlar ----
  messages(onlyUnread = false): Observable<Message[]> {
    const params = onlyUnread ? new HttpParams().set('unread', true) : undefined;
    return this.http.get<Message[]>(`${this.base}/messages`, { params });
  }

  setMessageRead(id: string, value: boolean): Observable<void> {
    return this.http.patch<void>(`${this.base}/messages/${id}/read`, null, {
      params: new HttpParams().set('value', value)
    });
  }

  deleteMessage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/messages/${id}`);
  }

  // ---- Dosya yükleme ----
  upload(file: File, kind: UploadKind): Observable<{ url: string }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(`${this.base}/uploads`, form, {
      params: new HttpParams().set('kind', kind)
    });
  }
}
