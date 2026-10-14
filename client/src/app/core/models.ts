// API'den gelen verilerin TypeScript karşılıkları.
// NOT: Backend'deki DTO record'ları ile birebir aynı alan isimleri (camelCase).

export interface Localized {
  tr: string;
  en: string;
}

export type SkillCategory = 'Backend' | 'Frontend' | 'Database' | 'Mobile' | 'DevOps' | 'Tools';
export type ProjectCategory = 'Web' | 'Mobile' | 'Api' | 'Tool';

export interface Profile {
  fullName: string;
  title: Localized;
  headline: Localized;
  about: Localized;
  location: string;
  email: string;
  avatarUrl: string | null;
  cvUrlTr: string | null;
  cvUrlEn: string | null;
  githubUrl: string | null;
  linkedinUrl: string | null;
  yearsOfExperience: number;
  isOpenToWork: boolean;
}

export interface Skill {
  id: string;
  name: string;
  category: SkillCategory;
  isHighlighted: boolean;
  displayOrder: number;
}

export interface Experience {
  id: string;
  company: string;
  position: Localized;
  description: Localized;
  location: string | null;
  startDate: string; // "2023-01-01" (DateOnly)
  endDate: string | null;
  technologies: string[];
  displayOrder: number;
}

export interface Education {
  id: string;
  school: string;
  department: Localized;
  degree: Localized;
  description: Localized;
  startYear: number;
  endYear: number | null;
  displayOrder: number;
}

export interface Certificate {
  id: string;
  title: string;
  issuer: string;
  issuedAt: string;
  credentialUrl: string | null;
  displayOrder: number;
}

export interface ProjectListItem {
  id: string;
  slug: string;
  title: Localized;
  summary: Localized;
  category: ProjectCategory;
  coverImageUrl: string | null;
  githubUrl: string | null;
  liveUrl: string | null;
  storeUrl: string | null;
  technologies: string[];
  projectDate: string; // "2026-02-01", projenin yapıldığı ay
  isFeatured: boolean;
}

export interface ProjectDetail extends Omit<ProjectListItem, 'isFeatured'> {
  content: Localized;
  createdAt: string;
}

export interface ProjectAdmin extends ProjectListItem {
  content: Localized;
  isPublished: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface Portfolio {
  profile: Profile | null;
  skills: Skill[];
  experiences: Experience[];
  educations: Education[];
  certificates: Certificate[];
  featuredProjects: ProjectListItem[];
}

export interface ContactRequest {
  name: string;
  email: string;
  subject: string | null;
  message: string;
  website: string | null; // honeypot
}

export interface Message {
  id: string;
  name: string;
  email: string;
  subject: string | null;
  body: string;
  isRead: boolean;
  createdAt: string;
}

export interface Dashboard {
  projects: number;
  publishedProjects: number;
  skills: number;
  experiences: number;
  certificates: number;
  messages: number;
  unreadMessages: number;
  latestMessages: Message[];
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
}

export interface AccessTokenResponse {
  accessToken: string;
  expiresAt: string;
  user: AdminUser;
}

// Backend'in ProblemDetails cevabı
export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  code?: string;
  errors?: Record<string, string[]>;
}
