import { AdminResource, UploadKind } from '../../core/api/admin-api.service';

// NOT: Admin formlarını bir ayar listesinden üretiyorum. Her sayfa için ayrı form HTML'i yazmak yerine alanları
// bir dizi olarak tanımlıyorum, DynamicForm component'i bu diziden formu kendisi oluşturuyor.
// Yeni bir alan eklemek için sadece bu listeye bir satır eklemek yetiyor.

export type FieldType =
  | 'text'
  | 'textarea'
  | 'markdown'
  | 'number'
  | 'date'
  | 'month'
  | 'checkbox'
  | 'select'
  | 'tags'
  | 'url'
  | 'email'
  | 'upload';

export interface FieldDef {
  key: string;
  label: string;
  type: FieldType;
  localized?: boolean; // true ise TR ve EN için iki ayrı input
  required?: boolean;
  nullable?: boolean; // sayı alanı boş bırakılınca 0 yerine null gönderilsin (örn. bitiş yılı)
  maxLength?: number;
  hint?: string;
  full?: boolean; // formda tam genişlik kaplasın mı
  options?: { value: string; label: string }[];
  uploadKind?: UploadKind;
}

export interface ColumnDef<T = any> {
  label: string;
  value: (row: T) => string;
  mono?: boolean;
  badge?: (row: T) => { text: string; kind: 'success' | 'accent' | 'default' } | null;
}

export interface CrudConfig<T = any> {
  title: string;
  singular: string;
  description: string;
  resource: AdminResource;
  columns: ColumnDef<T>[];
  fields: FieldDef[];
  defaults: () => Record<string, unknown>;
}
