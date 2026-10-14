import { Certificate, Education, Experience, ProjectAdmin, Skill } from '../../core/models';
import { CrudConfig } from './crud-types';

const projectCategories = [
  { value: 'Web', label: 'Web' },
  { value: 'Mobile', label: 'Mobil' },
  { value: 'Api', label: 'API' },
  { value: 'Tool', label: 'Araç' }
];

const skillCategories = [
  { value: 'Backend', label: 'Backend' },
  { value: 'Frontend', label: 'Frontend' },
  { value: 'Database', label: 'Veritabanı' },
  { value: 'Mobile', label: 'Mobil' },
  { value: 'DevOps', label: 'DevOps' },
  { value: 'Tools', label: 'Araçlar' }
];

const emptyLocalized = () => ({ tr: '', en: '' });
const yearRange = (start: string, end: string | null) => `${start.slice(0, 7)} → ${end ? end.slice(0, 7) : 'devam'}`;

export const projectsConfig: CrudConfig<ProjectAdmin> = {
  title: 'Projeler',
  singular: 'Proje',
  description: 'Portfolyoda gösterilen projeler. Yayında olmayanlar ziyaretçilere görünmez.',
  resource: 'projects',
  columns: [
    { label: 'Başlık', value: (p) => p.title.tr },
    { label: 'Kategori', value: (p) => p.category },
    { label: 'Tarih', value: (p) => p.projectDate.slice(0, 7), mono: true },
    {
      label: 'Durum',
      value: () => '',
      badge: (p) => (p.isPublished ? { text: 'Yayında', kind: 'success' } : { text: 'Taslak', kind: 'default' })
    },
    { label: 'Sıra', value: (p) => String(p.displayOrder), mono: true }
  ],
  fields: [
    { key: 'title', label: 'Başlık', type: 'text', localized: true, required: true, maxLength: 150, full: true },
    { key: 'slug', label: 'Slug', type: 'text', maxLength: 80, hint: 'Boş bırakılırsa başlıktan üretilir. Örn: devlog' },
    { key: 'category', label: 'Kategori', type: 'select', required: true, options: projectCategories },
    { key: 'projectDate', label: 'Proje tarihi', type: 'month', required: true, hint: 'Sitede projeler bu tarihe göre sıralanır, en yeni en üstte.' },
    { key: 'summary', label: 'Kısa özet', type: 'textarea', localized: true, required: true, maxLength: 400, full: true },
    { key: 'technologies', label: 'Teknolojiler', type: 'tags', full: true, hint: 'Virgülle ayır: .NET, Angular, SQL Server' },
    { key: 'coverImageUrl', label: 'Kapak görseli', type: 'upload', uploadKind: 'Image', full: true, hint: 'JPG, PNG ya da WEBP, en fazla 2 MB. 16:9 oran önerilir.' },
    { key: 'githubUrl', label: 'GitHub linki', type: 'url' },
    { key: 'liveUrl', label: 'Canlı site linki', type: 'url' },
    { key: 'storeUrl', label: 'Mağaza linki', type: 'url' },
    { key: 'displayOrder', label: 'Sıra', type: 'number' },
    { key: 'content', label: 'Detay içeriği (markdown)', type: 'markdown', localized: true, full: true, hint: '## başlık, - liste, **kalın**, `kod`, ``` kod bloğu ```' },
    { key: 'isFeatured', label: 'Ana sayfada öne çıkar', type: 'checkbox' },
    { key: 'isPublished', label: 'Yayında', type: 'checkbox' }
  ],
  defaults: () => ({
    title: emptyLocalized(),
    slug: '',
    summary: emptyLocalized(),
    content: emptyLocalized(),
    category: 'Web',
    projectDate: new Date().toISOString().slice(0, 10),
    coverImageUrl: null,
    githubUrl: null,
    liveUrl: null,
    storeUrl: null,
    technologies: [],
    isFeatured: false,
    isPublished: true,
    displayOrder: 0
  })
};

export const experiencesConfig: CrudConfig<Experience> = {
  title: 'Deneyimler',
  singular: 'Deneyim',
  description: 'İş deneyimleri. Bitiş tarihi boş bırakılırsa "devam ediyor" olarak gösterilir.',
  resource: 'experiences',
  columns: [
    { label: 'Pozisyon', value: (e) => e.position.tr },
    { label: 'Şirket', value: (e) => e.company },
    { label: 'Tarih', value: (e) => yearRange(e.startDate, e.endDate), mono: true },
    { label: 'Sıra', value: (e) => String(e.displayOrder), mono: true }
  ],
  fields: [
    { key: 'position', label: 'Pozisyon', type: 'text', localized: true, required: true, maxLength: 150, full: true },
    { key: 'company', label: 'Şirket', type: 'text', required: true, maxLength: 150 },
    { key: 'location', label: 'Konum', type: 'text', maxLength: 100 },
    { key: 'startDate', label: 'Başlangıç', type: 'month', required: true },
    { key: 'endDate', label: 'Bitiş', type: 'month', hint: 'Hala çalışıyorsan boş bırak' },
    { key: 'description', label: 'Açıklama', type: 'textarea', localized: true, required: true, maxLength: 3000, full: true },
    { key: 'technologies', label: 'Teknolojiler', type: 'tags', full: true, hint: 'Virgülle ayır' },
    { key: 'displayOrder', label: 'Sıra', type: 'number' }
  ],
  defaults: () => ({
    company: '',
    position: emptyLocalized(),
    description: emptyLocalized(),
    location: '',
    startDate: '',
    endDate: null,
    technologies: [],
    displayOrder: 0
  })
};

export const educationsConfig: CrudConfig<Education> = {
  title: 'Eğitim',
  singular: 'Eğitim',
  description: 'Okul ve bölüm bilgileri.',
  resource: 'educations',
  columns: [
    { label: 'Okul', value: (e) => e.school },
    { label: 'Bölüm', value: (e) => e.department.tr },
    { label: 'Yıllar', value: (e) => `${e.startYear} → ${e.endYear ?? 'devam'}`, mono: true }
  ],
  fields: [
    { key: 'school', label: 'Okul', type: 'text', required: true, maxLength: 150, full: true },
    { key: 'department', label: 'Bölüm', type: 'text', localized: true, required: true, maxLength: 150, full: true },
    { key: 'degree', label: 'Derece', type: 'text', localized: true, required: true, maxLength: 100, full: true, hint: 'Lisans / Bachelor\'s' },
    { key: 'startYear', label: 'Başlangıç yılı', type: 'number', required: true },
    { key: 'endYear', label: 'Bitiş yılı', type: 'number', nullable: true, hint: 'Devam ediyorsa boş bırak' },
    { key: 'description', label: 'Açıklama', type: 'textarea', localized: true, maxLength: 1000, full: true },
    { key: 'displayOrder', label: 'Sıra', type: 'number' }
  ],
  defaults: () => ({
    school: '',
    department: emptyLocalized(),
    degree: emptyLocalized(),
    description: emptyLocalized(),
    startYear: new Date().getFullYear() - 4,
    endYear: null,
    displayOrder: 0
  })
};

export const skillsConfig: CrudConfig<Skill> = {
  title: 'Yetenekler',
  singular: 'Yetenek',
  description: 'Kategorilere göre gruplanarak gösterilir. Öne çıkanlar ana sayfadaki kod penceresinde de yer alır.',
  resource: 'skills',
  columns: [
    { label: 'Ad', value: (s) => s.name },
    { label: 'Kategori', value: (s) => s.category },
    { label: 'Öne çıkan', value: () => '', badge: (s) => (s.isHighlighted ? { text: 'Evet', kind: 'accent' } : null) },
    { label: 'Sıra', value: (s) => String(s.displayOrder), mono: true }
  ],
  fields: [
    { key: 'name', label: 'Ad', type: 'text', required: true, maxLength: 50 },
    { key: 'category', label: 'Kategori', type: 'select', required: true, options: skillCategories },
    { key: 'displayOrder', label: 'Sıra', type: 'number' },
    { key: 'isHighlighted', label: 'Öne çıkar', type: 'checkbox' }
  ],
  defaults: () => ({ name: '', category: 'Backend', isHighlighted: false, displayOrder: 0 })
};

export const certificatesConfig: CrudConfig<Certificate> = {
  title: 'Sertifikalar',
  singular: 'Sertifika',
  description: 'Kurs ve sertifikalar.',
  resource: 'certificates',
  columns: [
    { label: 'Başlık', value: (c) => c.title },
    { label: 'Veren', value: (c) => c.issuer },
    { label: 'Tarih', value: (c) => c.issuedAt, mono: true }
  ],
  fields: [
    { key: 'title', label: 'Başlık', type: 'text', required: true, maxLength: 200, full: true },
    { key: 'issuer', label: 'Veren kurum', type: 'text', required: true, maxLength: 100 },
    { key: 'issuedAt', label: 'Tarih', type: 'date', required: true },
    { key: 'credentialUrl', label: 'Doğrulama linki', type: 'url', full: true },
    { key: 'displayOrder', label: 'Sıra', type: 'number' }
  ],
  defaults: () => ({ title: '', issuer: '', issuedAt: '', credentialUrl: null, displayOrder: 0 })
};
