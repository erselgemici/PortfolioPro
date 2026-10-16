using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortfolioPro.Domain.Common;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Infrastructure.Persistence.Seed;

// Veritabanı ilk kez oluşturulduğunda siteyi boş göstermemek için başlangıç içeriği (CV'deki bilgilerle).
// NOT: Profil tablosu doluysa bu metot hiçbir şey yapmıyor. Sonraki değişiklikler admin panelinden yapılıyor.
internal static class SeedData
{
    private const string Github = "https://github.com/erselgemici";

    public static async Task SeedContentAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Profiles.AnyAsync())
            return;

        logger.LogInformation("Başlangıç içeriği ekleniyor...");

        db.Profiles.Add(Profile());
        db.Skills.AddRange(Skills());
        db.Experiences.AddRange(Experiences());
        db.Educations.AddRange(Educations());
        db.Certificates.AddRange(Certificates());
        db.Projects.AddRange(Projects());

        await db.SaveChangesAsync();
    }

    // ---------------- Profil ----------------
    private static SiteProfile Profile() => new()
    {
        FullName = "Ersel Gemici",
        Title = T("Full Stack Developer", "Full Stack Developer"),
        Headline = T(
            "C# / .NET Core ile backend, Angular ve React ile arayüz geliştiriyorum. Sade, sürdürülebilir ve katmanlı mimariyle çözümler üretiyorum.",
            "I build backends with C# / .NET Core and interfaces with Angular and React, focusing on clean, maintainable, layered solutions."),
        About = T(
            "Matematik mezunuyum. Yaklaşık 3 yıllık kurumsal deneyimimi Softtech ve PiA'da edindim. Backend tarafında C# / .NET, " +
            "arayüz tarafında Angular ve React ile çalışıyorum; MSSQL, PostgreSQL ve MongoDB kullandım.\n\n" +
            "PiA'da uluslararası operasyon ekibinden gelen canlı ortam hatalarını analiz edip çözdüm. 2025'ten bu yana M&Y Yazılım Akademi'de " +
            "aldığım .NET Core eğitimiyle Clean Architecture, CQRS ve yapay zekâ entegrasyonlu projeler geliştiriyorum.\n\n" +
            "Gemici Studio adıyla kendi uygulamalarımı da yayınlıyorum; 101 Çetele Google Play'de.",
            "I studied mathematics and gained about 3 years of enterprise experience at Softtech and PiA. I work with C# / .NET on the backend " +
            "and Angular and React on the frontend, and I have used MSSQL, PostgreSQL and MongoDB.\n\n" +
            "At PiA I analyzed and fixed production issues reported by the international operations team. Since 2025 I have been building " +
            "projects with Clean Architecture, CQRS and AI integrations through the .NET Core program at M&Y Yazılım Akademi.\n\n" +
            "I also publish my own apps under the Gemici Studio name; 101 Çetele is live on Google Play."),
        Location = "Ümraniye, İstanbul",
        Email = "erselgemici.eg@gmail.com",
        GithubUrl = Github,
        LinkedinUrl = "https://www.linkedin.com/in/ersel-gemici",
        YearsOfExperience = 3,
        IsOpenToWork = true
    };

    // ---------------- Yetenekler ----------------
    private static IEnumerable<Skill> Skills() =>
    [
        S("C#", SkillCategory.Backend, 1, true),
        S(".NET", SkillCategory.Backend, 2, true),
        S("ASP.NET Core", SkillCategory.Backend, 3, true),
        S("Entity Framework Core", SkillCategory.Backend, 4, true),
        S("ASP.NET MVC", SkillCategory.Backend, 5),
        S("REST API", SkillCategory.Backend, 6),
        S("CQRS / MediatR", SkillCategory.Backend, 7),
        S("Onion Architecture", SkillCategory.Backend, 8),
        S("Cobol", SkillCategory.Backend, 9),

        S("Angular", SkillCategory.Frontend, 1, true),
        S("React", SkillCategory.Frontend, 2, true),
        S("TypeScript", SkillCategory.Frontend, 3),
        S("JavaScript", SkillCategory.Frontend, 4),
        S("RxJS", SkillCategory.Frontend, 5),
        S("HTML & CSS", SkillCategory.Frontend, 6),
        S("Bootstrap", SkillCategory.Frontend, 7),

        S("SQL Server", SkillCategory.Database, 1, true),
        S("PostgreSQL", SkillCategory.Database, 2, true),
        S("MongoDB", SkillCategory.Database, 3, true),

        S("React Native", SkillCategory.Mobile, 1),
        S("Expo", SkillCategory.Mobile, 2),

        S("Docker", SkillCategory.DevOps, 1),
        S("Git & GitHub", SkillCategory.DevOps, 2),
        S("GitHub Actions", SkillCategory.DevOps, 3),
        S("Datadog", SkillCategory.DevOps, 4),

        S("Visual Studio", SkillCategory.Tools, 1),
        S("Postman", SkillCategory.Tools, 2),
        S("Jira", SkillCategory.Tools, 3),
        S("Camunda", SkillCategory.Tools, 4),
        S("Keycloak", SkillCategory.Tools, 5),
        S("Firebase", SkillCategory.Tools, 6)
    ];

    // ---------------- Deneyim ----------------
    private static IEnumerable<Experience> Experiences() =>
    [
        new Experience
        {
            Company = "Gemici Studio",
            Position = T("Kurucu & Bağımsız Geliştirici", "Founder & Independent Developer"),
            Description = T(
                "Kendi mobil ve web uygulamalarımı fikir aşamasından mağaza yayınına kadar uçtan uca geliştiriyorum. " +
                "Okey 101 skor takip uygulamam 101 Çetele, Google Play'de yayında.",
                "I build my own mobile and web apps end to end, from the idea stage to store release. " +
                "My Okey 101 score-tracking app, 101 Çetele, is live on Google Play."),
            Location = "İstanbul",
            StartDate = new DateOnly(2025, 5, 1),
            Technologies = ["React Native", "Expo", "Firebase", ".NET", "Angular"],
            DisplayOrder = 1
        },
        new Experience
        {
            Company = "Kurulum Cognitive Services",
            Position = T("Software Engineer", "Software Engineer"),
            Description = T(
                "İki aylık bu süreçte .NET tabanlı uygulamaların bakımını yaptım.",
                "During this two-month role I maintained .NET based applications."),
            Location = "İstanbul",
            StartDate = new DateOnly(2026, 2, 1),
            EndDate = new DateOnly(2026, 4, 1),
            Technologies = [".NET"],
            DisplayOrder = 2
        },
        new Experience
        {
            Company = "PiA / DNext",
            Position = T("Software Engineer", "Software Engineer"),
            Description = T(
                "• Uluslararası operasyon ekibinden (Arnavutluk) gelen hata bildirimlerini ve iş süreci kesintilerini analiz ederek çözüme kavuşturdum; sistem sürekliliğini sağladım.\n" +
                "• SQL ve MongoDB üzerinde yapısal iyileştirmeler gerçekleştirerek performans sorunlarını ve veri kaynaklı hataları gideren optimizasyonlar yaptım.\n" +
                "• Kullanıcı taleplerini ve teknik sorunları analiz ederek uçtan uca çözüm ürettim; .NET ve React teknolojileriyle sistemlerin bakım süreçlerini yürüttüm.\n" +
                "• Mevcut uygulamaların iyileştirilmesini ve sürdürülebilirliğini sağlayarak uygulama kararlılığını artırdım.",
                "• Analyzed and resolved bug reports and business process interruptions coming from the international operations team (Albania), ensuring system continuity.\n" +
                "• Made structural improvements on SQL and MongoDB, delivering optimizations that fixed performance issues and data-related errors.\n" +
                "• Analyzed user requests and technical issues and delivered end-to-end solutions; maintained systems built with .NET and React.\n" +
                "• Increased application stability by improving existing applications and keeping them maintainable."),
            Location = "İstanbul",
            StartDate = new DateOnly(2023, 9, 1),
            EndDate = new DateOnly(2025, 4, 1),
            Technologies = [".NET", "React", "SQL", "MongoDB", "Datadog", "Camunda", "Keycloak"],
            DisplayOrder = 3
        },
        new Experience
        {
            Company = "Softtech / Deposit Team",
            Position = T("Software Developer", "Software Developer"),
            Description = T(
                "• Kurumsal mimaride Cobol ve Java teknolojileriyle backend servislerinin geliştirilmesinde ve bakımında görev aldım.\n" +
                "• ASP.NET MVC ile arayüz (frontend) tasarımı ve entegrasyonunu uçtan uca üstlendim.",
                "• Worked on developing and maintaining backend services with Cobol and Java in an enterprise architecture.\n" +
                "• Owned frontend design and integration end to end with ASP.NET MVC."),
            Location = "İstanbul",
            StartDate = new DateOnly(2022, 8, 1),
            EndDate = new DateOnly(2023, 9, 1),
            Technologies = ["Cobol", "Java", "ASP.NET MVC"],
            DisplayOrder = 4
        }
    ];

    // ---------------- Eğitim ----------------
    private static IEnumerable<Education> Educations() =>
    [
        new Education
        {
            School = "M&Y Yazılım Akademi",
            Department = T("Full Stack .NET Core Developer Bootcamp", "Full Stack .NET Core Developer Bootcamp"),
            Degree = T("Bootcamp", "Bootcamp"),
            Description = LocalizedText.Empty,
            StartYear = 2025,
            EndYear = 2026,
            DisplayOrder = 1
        },
        new Education
        {
            School = "Anadolu Üniversitesi",
            Department = T("Web Tasarımı ve Programlama", "Web Design and Programming"),
            Degree = T("Ön Lisans", "Associate Degree"),
            Description = LocalizedText.Empty,
            StartYear = 2020,
            EndYear = 2022,
            DisplayOrder = 2
        },
        new Education
        {
            School = "Kocaeli Üniversitesi",
            Department = T("Matematik", "Mathematics"),
            Degree = T("Lisans", "Bachelor's Degree"),
            Description = T("C ve MATLAB programlama seçmeli dersleri.", "Elective courses in C and MATLAB programming."),
            StartYear = 2014,
            EndYear = 2020,
            DisplayOrder = 3
        }
    ];

    // ---------------- Sertifikalar ----------------
    private static IEnumerable<Certificate> Certificates() =>
    [
        new Certificate
        {
            Title = "Full Stack .NET Developer Yetiştirme Programı",
            Issuer = "M&Y Yazılım Akademi",
            IssuedAt = new DateOnly(2026, 1, 1),
            DisplayOrder = 1
        },
        new Certificate
        {
            Title = "Modern C# Kursu: .NET",
            Issuer = "Udemy",
            IssuedAt = new DateOnly(2022, 6, 1),
            DisplayOrder = 2
        },
        new Certificate
        {
            Title = "ComSoc Akademi C# Eğitimi",
            Issuer = "IEEE Doğuş Üniversitesi Öğrenci Kolu",
            IssuedAt = new DateOnly(2022, 5, 1),
            DisplayOrder = 3
        },
        new Certificate
        {
            Title = "ComSoc Akademi Java Eğitimi",
            Issuer = "IEEE Doğuş Üniversitesi Öğrenci Kolu",
            IssuedAt = new DateOnly(2022, 5, 1),
            DisplayOrder = 4
        },
        new Certificate
        {
            Title = "Java ile Programlamaya Giriş",
            Issuer = "BTK Akademi",
            IssuedAt = new DateOnly(2022, 5, 1),
            DisplayOrder = 5
        },
        new Certificate
        {
            Title = "Kodla, Kariyerine Başla: Hazırlık",
            Issuer = "Kodluyoruz",
            IssuedAt = new DateOnly(2022, 2, 1),
            DisplayOrder = 6
        },
        new Certificate
        {
            Title = "Microsoft Yazılım Uzmanlığı (MCSD)",
            Issuer = "BT Akademi",
            IssuedAt = new DateOnly(2020, 6, 1),
            DisplayOrder = 7
        }
    ];

    // ---------------- Projeler ----------------
    // NOT: Projeler ProjectDate'e göre sıralanıyor (en yeni en üstte). Tarihler GitHub'daki repo
    // oluşturulma tarihlerinden alındı. Aynı aydaki projelerde DisplayOrder küçük olan önce geliyor.
    private static IEnumerable<Project> Projects() =>
    [
        P("portfolio-pro", new DateOnly(2026, 10, 1), ProjectCategory.Web, 1, featured: true,
            T("PortfolioPro: Kişisel Portfolyo", "PortfolioPro: Personal Portfolio"),
            T("Şu an gezdiğin site. .NET 9 ve Angular 20 ile geliştirip Docker ile yayına aldım.",
              "The site you're browsing. Built with .NET 9 and Angular 20 and deployed with Docker."),
            T("## Mimari\n\n- .NET 9 Web API, Clean Architecture (Domain, Application, Infrastructure, API)\n- CQRS + MediatR, FluentValidation\n" +
              "- EF Core + PostgreSQL\n- Angular 20: standalone component, signals\n\n" +
              "## Güvenlik\n\n- JWT ile admin girişi, refresh token httpOnly cookie'de\n" +
              "- Giriş ve iletişim formunda rate limiting\n\n" +
              "## Yayın\n\n- Angular derlenip API'nin içine konuyor, tek Docker imajı\n- Render'da yayında, veritabanı Neon (PostgreSQL)\n" +
              "- GitHub Actions ile her push'ta build ve test, push sonrası otomatik deploy",
              "## Architecture\n\n- .NET 9 Web API, Clean Architecture (Domain, Application, Infrastructure, API)\n- CQRS + MediatR, FluentValidation\n" +
              "- EF Core + PostgreSQL\n- Angular 20: standalone components, signals\n\n" +
              "## Security\n\n- JWT admin login, refresh token in an httpOnly cookie\n" +
              "- Rate limiting on login and the contact form\n\n" +
              "## Deployment\n\n- Angular is built into the API, a single Docker image\n- Hosted on Render, database on Neon (PostgreSQL)\n" +
              "- GitHub Actions builds and tests every push, deploys automatically after push"),
            [".NET 9", "Angular 20", "PostgreSQL", "Docker", "JWT"], github: Repo("PortfolioPro"), live: "https://erselgemici.com"),

        P("devlog", new DateOnly(2026, 10, 1), ProjectCategory.Web, 2, featured: true,
            T("DevLog: Çok Yazarlı Blog Platformu", "DevLog: Multi-Author Blog Platform"),
            T(".NET 9 Minimal API, Onion Architecture ve CQRS ile yazılmış, Angular 20 arayüzlü, iki dilli blog platformu.",
              "A bilingual blog platform built with .NET 9 Minimal API, Onion Architecture and CQRS, with an Angular 20 UI."),
            T("## Proje hakkında\n\nTemelini Erhan Gündüz'ün Udemy eğitimindeki ZenBlog projesinden aldım, kendime uyarlayıp DevLog olarak geliştirdim.\n\n" +
              "## Mimari\n\n- .NET 9 Minimal API, Onion Architecture\n- CQRS + MediatR, FluentValidation, AutoMapper\n- EF Core + SQL Server, Generic Repository ve Unit of Work\n- ASP.NET Core Identity + JWT, rol bazlı (Admin/Yazar) yetkilendirme\n- Angular 20 arayüz, Türkçe/İngilizce dil desteği\n\n" +
              "## Neler ekledim\n\n- Etiket sistemi (çoka-çok ilişki) ve etiket sayfası\n- Arama ve sayfalama\n- Yazar rolü ve sahiplik kontrolü\n- İlgili yazılar ve yönetim paneli istatistikleri\n\n" +
              "## Öğrendiklerim\n\nGerçekçi veri yükleyince ortaya çıkan N+1 sorgu problemini lazy loading'i kaldırıp `Include` ve `AsSplitQuery` ile çözdüm.",
              "## About\n\nI started from the ZenBlog project in Erhan Gündüz's Udemy course and adapted it into my own platform, DevLog.\n\n" +
              "## Architecture\n\n- .NET 9 Minimal API, Onion Architecture\n- CQRS + MediatR, FluentValidation, AutoMapper\n- EF Core + SQL Server, Generic Repository and Unit of Work\n- ASP.NET Core Identity + JWT, role-based (Admin/Author) authorization\n- Angular 20 UI with Turkish/English support\n\n" +
              "## What I added\n\n- Tag system (many-to-many) and tag pages\n- Search and pagination\n- Author role with ownership checks\n- Related posts and admin dashboard statistics\n\n" +
              "## What I learned\n\nAfter loading realistic data I hit the N+1 query problem and fixed it by removing lazy loading and using `Include` and `AsSplitQuery`."),
            [".NET 9", "Angular 20", "EF Core", "SQL Server", "MediatR", "JWT"], github: Repo("DevLog")),

        P("101-cetele", new DateOnly(2026, 8, 1), ProjectCategory.Mobile, 3, featured: true,
            T("101 Çetele", "101 Çetele"),
            T("Okey 101 oyunu için skor takip uygulaması. Google Play'de yayında.",
              "A score-tracking app for the Okey 101 game. Live on Google Play."),
            T("## Proje hakkında\n\nOkey 101 oynarken kağıt kalemle tutulan çeteleyi telefona taşıdım. Gemici Studio adı altında Google Play'de yayınladım.\n\n" +
              "## Özellikler\n\n- Eşli oyunlarda takım isimleri\n- Puan hesaplamalı oyun içgörüleri\n- Rozet ve oyun hikayesi sistemi\n- Misafir girişinden kayıtlı hesaba geçiş",
              "## About\n\nI moved the pen-and-paper score sheet used in Okey 101 games onto the phone and published it on Google Play under Gemici Studio.\n\n" +
              "## Features\n\n- Team names for pair games\n- Game insights with exact point calculations\n- Badge and game story system\n- Upgrade path from guest to registered account"),
            ["React Native", "Expo", "Firebase", "SQLite"],
            store: "https://play.google.com/store/apps/details?id=com.cetele101.app"),

        P("carbook", new DateOnly(2026, 2, 1), ProjectCategory.Api, 4, featured: false,
            T("CarBook: Onion Architecture API", "CarBook: Onion Architecture API"),
            T("Araç kiralama senaryosu üzerine, Onion Architecture prensipleriyle geliştirilmiş backend.",
              "A car rental backend built with Onion Architecture principles."),
            T("## Proje hakkında\n\nAraç kiralama senaryosu üzerine kurulu, katmanlar arası bağımlılıkları minimize eden Onion Architecture prensipleriyle geliştirdiğim sürdürülebilir backend sistemi.",
              "## About\n\nA maintainable backend for a car rental scenario, built with Onion Architecture principles to minimize dependencies between layers."),
            [".NET 9", "Onion Architecture", "CQRS", "MediatR", "FluentValidation", "EF Core"], github: Repo("CarBook"), published: false),

        P("bagery-cqrs", new DateOnly(2026, 2, 1), ProjectCategory.Api, 5, featured: true,
            T("Bagery_CQRS: E-Ticaret Platformu", "Bagery_CQRS: E-Commerce Platform"),
            T("Clean Architecture ve saf CQRS deseniyle geliştirilmiş .NET 8.0 e-ticaret backend'i.",
              "A .NET 8.0 e-commerce backend built with Clean Architecture and a pure CQRS pattern."),
            T("## Proje hakkında\n\nClean Architecture ve saf CQRS deseninin (Manual CQRS, Pure Dependency Injection) uygulandığı kapsamlı e-ticaret backend'i. " +
              "MediatR kullanmadan her handler'ı doğrudan DI ile bağladım; bir isteğin hangi sınıftan geçtiği IDE'de baştan sona izlenebiliyor.\n\n" +
              "## Öne çıkanlar\n\n- Sipariş süreci **Chain of Responsibility** deseniyle bağımsız adımlara ayrıldı: stok kontrolü → ödeme → sipariş oluşturma\n" +
              "- Kritik olaylar **Observer** deseniyle asenkron loglanıyor\n- Görsel depolama için Google Cloud Storage entegrasyonu",
              "## About\n\nA comprehensive e-commerce backend applying Clean Architecture and a pure CQRS pattern (manual CQRS, pure dependency injection). " +
              "Handlers are wired directly through DI without MediatR, so every request can be traced end to end in the IDE.\n\n" +
              "## Highlights\n\n- The order flow is split into independent steps with the **Chain of Responsibility** pattern: stock check → payment → order creation\n" +
              "- Critical events are logged asynchronously with the **Observer** pattern\n- Google Cloud Storage integration for images"),
            [".NET 8", "Clean Architecture", "CQRS", "Design Patterns", "Google Cloud Storage"], github: Repo("Bagery")),

        P("bepop", new DateOnly(2026, 2, 1), ProjectCategory.Web, 6, featured: true,
            T("Bepop: AI Destekli Müzik Platformu", "Bepop: AI-Powered Music Platform"),
            T("JWT ile ayrık backend/frontend mimarisi, paket bazlı yetkilendirme ve ML.NET tabanlı kişisel müzik önerileri.",
              "Decoupled backend/frontend with JWT, tier-based authorization and personalized music recommendations with ML.NET."),
            T("## Proje hakkında\n\n.NET 8 ile geliştirdiğim, kullanıcının dinleme davranışlarını öğrenip ona göre öneri yapan müzik platformu.\n\n" +
              "## Özellikler\n\n- Backend (REST API) ve frontend (WebUI) tamamen ayrık, tüm iletişim JWT ile\n" +
              "- Paket bazlı yetkilendirme (Free, Premium, Elite): şarkılar kullanıcının paketine göre açılıyor\n" +
              "- ML.NET ile dinleme alışkanlıklarını analiz eden öneri motoru (Recommendation Engine)\n- ASP.NET Core Identity ve EF Core",
              "## About\n\nA music platform built with .NET 8 that learns from listening behavior and recommends songs accordingly.\n\n" +
              "## Features\n\n- Fully decoupled backend (REST API) and frontend (WebUI), all communication over JWT\n" +
              "- Tier-based authorization (Free, Premium, Elite): songs unlock based on the user's package\n" +
              "- An ML.NET recommendation engine that analyzes listening habits\n- ASP.NET Core Identity and EF Core"),
            [".NET 8", "ML.NET", "JWT", "Identity", "EF Core", "JavaScript"], github: Repo("Bepop")),

        P("aitech", new DateOnly(2026, 1, 1), ProjectCategory.Web, 7, featured: false,
            T("AITech: AI Destekli Kurumsal Platform", "AITech: AI-Powered Corporate Platform"),
            T("Google Gemini entegrasyonlu iş asistanı içeren, 5 katmanlı mimariyle geliştirilmiş kurumsal web sitesi.",
              "A corporate website with a Google Gemini business assistant, built on a 5-layer architecture."),
            T("## Proje hakkında\n\nKlasik kurumsal web sitesi yapısını yapay zekâ ile birleştiren bir proje. .NET 8 ve N-Tier mimariyle geliştirdim.\n\n" +
              "## Özellikler\n\n- **AI Business Assistant:** Google Gemini API ile kullanıcıdan gelen istemlere cevap üretip Markdown'dan HTML'e çevirerek gösteriyor\n" +
              "- WebUI, Business, DataAccess, Entity ve DTO olmak üzere 5 katman\n- EF Core Code First, DTO ile veri güvenliği\n- Dinamik yönetim paneli",
              "## About\n\nA project that combines a classic corporate website with AI, built with .NET 8 and an N-Tier architecture.\n\n" +
              "## Features\n\n- **AI Business Assistant:** answers prompts via the Google Gemini API and renders the Markdown response as HTML\n" +
              "- 5 layers: WebUI, Business, DataAccess, Entity and DTO\n- EF Core Code First, DTOs for data safety\n- Dynamic admin panel"),
            [".NET 8", "N-Tier", "EF Core", "Google Gemini"], github: Repo("AITech")),

        P("lifesure", new DateOnly(2026, 1, 1), ProjectCategory.Web, 8, featured: true,
            T("LifeSure: AI & ML Destekli Sigorta Platformu", "LifeSure: AI & ML-Powered Insurance Platform"),
            T("ChatGPT ile çok dilli otomatik yanıt, ML.NET satış tahmini ve Elasticsearch/Kibana loglama içeren yönetim paneli.",
              "An admin platform with multilingual ChatGPT auto-replies, ML.NET sales forecasting and Elasticsearch/Kibana logging."),
            T("## Proje hakkında\n\nKlasik sigortacılık işlemlerini yapay zekâ, makine öğrenimi ve loglama altyapısıyla birleştiren bir yönetim paneli.\n\n" +
              "## Özellikler\n\n- İletişim formundan gelen mesajın dilini AI algılıyor ve ChatGPT ile müşteriye **kendi dilinde** otomatik e-posta yanıtı gönderiliyor\n" +
              "- ML.NET ile geçmiş verilerden satış tahmini (regresyon), sonuçlar panelde grafik olarak\n" +
              "- Docker üzerinde Elasticsearch ve Kibana ile loglama; kimin hangi veriyi ne zaman değiştirdiği (audit log) JSON olarak saklanıyor\n- Çoklu dil desteği, EF Code First",
              "## About\n\nAn admin platform that combines classic insurance operations with AI, machine learning and a logging infrastructure.\n\n" +
              "## Features\n\n- AI detects the language of contact messages and ChatGPT sends an automatic email reply **in the customer's own language**\n" +
              "- Sales forecasting (regression) with ML.NET, shown as charts in the panel\n" +
              "- Logging with Elasticsearch and Kibana on Docker; audit logs of who changed what and when are stored as JSON\n- Localization, EF Code First"),
            ["ASP.NET MVC", "ML.NET", "OpenAI API", "Elasticsearch", "Kibana", "Docker"], github: Repo("LifeSure")),

        P("travel-booking", new DateOnly(2026, 1, 1), ProjectCategory.Web, 9, featured: false,
            T("TravelBooking: AI Destekli Seyahat Platformu", "TravelBooking: AI-Supported Travel Platform"),
            T("Booking.com ve TripAdvisor API'leri, canlı dashboard ve Claude AI ile rota önerisi sunan seyahat sitesi.",
              "A travel site with Booking.com and TripAdvisor APIs, a live dashboard and route suggestions from Claude AI."),
            T("## Proje hakkında\n\nASP.NET Core MVC ile geliştirdiğim, birden fazla dış API'yi bir araya getiren seyahat platformu.\n\n" +
              "## Özellikler\n\n- **AI seyahat asistanı:** Anthropic Claude ile şehre özel \"Tarih & Kültür\" ve \"Gurme\" rotaları, öneriler Google Maps linklerine dönüşüyor\n" +
              "- **Canlı dashboard:** hava durumu, döviz/kripto kurları ve akaryakıt fiyatları (ViewComponent mimarisi)\n" +
              "- **Booking.com** ile gerçek zamanlı otel arama, **TripAdvisor** ile çevredeki gezilecek yerler (RapidAPI)",
              "## About\n\nA travel platform built with ASP.NET Core MVC that brings together several third-party APIs.\n\n" +
              "## Features\n\n- **AI travel assistant:** city-specific \"History & Culture\" and \"Food\" routes from Anthropic Claude, turned into Google Maps links\n" +
              "- **Live dashboard:** weather, currency/crypto rates and fuel prices (ViewComponent architecture)\n" +
              "- Real-time hotel search with **Booking.com** and nearby attractions with **TripAdvisor** (RapidAPI)"),
            ["ASP.NET Core MVC", "RapidAPI", "Claude API", "ViewComponent"], github: Repo("TravelBooking")),

        P("medinova", new DateOnly(2025, 12, 1), ProjectCategory.Web, 10, featured: true,
            T("Medinova: AI & ML Destekli Hastane Yönetim Sistemi", "Medinova: AI & ML-Powered Hospital Management"),
            T("Gemini ile semptom analizi, ML.NET ile randevu yoğunluğu tahmini ve otomatik e-posta bildirimleri sunan hastane sistemi.",
              "A hospital system with Gemini symptom analysis, ML.NET appointment forecasting and automatic email notifications."),
            T("## Proje hakkında\n\n.NET 8 ile geliştirdiğim hastane randevu ve yönetim sistemi. EF Core **Database First** yaklaşımı ve N-Tier mimari.\n\n" +
              "## Özellikler\n\n- **AI hasta asistanı (Google Gemini):** hastanın şikayetini analiz edip hastanenin gerçek bölümlerinden doğru polikliniğe yönlendiriyor\n" +
              "- **ML.NET** ile geçmiş randevulardan önümüzdeki 3 ayın yoğunluk tahmini\n- Randevu alınınca SMTP ile HTML şablonlu e-posta bildirimi\n" +
              "- Hazır Identity kullanmadan kendi User/Role tablolarıyla rol bazlı yetkilendirme (RBAC)",
              "## About\n\nA hospital appointment and management system built with .NET 8, using the EF Core **Database First** approach and an N-Tier architecture.\n\n" +
              "## Features\n\n- **AI patient assistant (Google Gemini):** analyzes symptoms and routes the patient to the right clinic among the hospital's real departments\n" +
              "- 3-month appointment volume forecasting with **ML.NET**\n- HTML email notifications over SMTP when an appointment is booked\n" +
              "- Role-based access control built on custom User/Role tables instead of ASP.NET Identity"),
            [".NET 8", "EF Core", "Database First", "Google Gemini", "ML.NET"], github: Repo("Medinova")),

        P("sonicmusic-analytics", new DateOnly(2025, 12, 1), ProjectCategory.Web, 11, featured: false,
            T("SonicMusic Analytics Dashboard", "SonicMusic Analytics Dashboard"),
            T("1 milyondan fazla satır veriyi Dapper ve SQL ile analiz eden müzik platformu admin paneli.",
              "A music platform admin dashboard that analyzes 1M+ rows of data with Dapper and SQL."),
            T("## Proje hakkında\n\nSpotify benzeri bir müzik platformunun arka planını simüle eden veri analitiği ve iş zekâsı projesi.\n\n" +
              "## Özellikler\n\n- 1 milyon+ satırlık gerçekçi veri (sanatçı, albüm, şarkı, dinleme logu)\n" +
              "- Entity Framework yerine **Dapper** ile hızlı veri erişimi\n" +
              "- Prime-time trafik analizi, sanatçıların haftalık büyüme oranı, süper fan tespiti gibi SQL raporları\n- Chart.js ile radar, polar ve çizgi grafikler",
              "## About\n\nA data analytics and BI project simulating the back end of a Spotify-like music platform.\n\n" +
              "## Features\n\n- 1M+ rows of realistic data (artists, albums, songs, listening logs)\n" +
              "- Fast data access with **Dapper** instead of Entity Framework\n" +
              "- SQL reports such as prime-time traffic, weekly artist growth rate and super-fan detection\n- Radar, polar and line charts with Chart.js"),
            ["ASP.NET Core", "Dapper", "SQL Server", "Chart.js"], github: Repo("SonicMusic")),

        P("kaira", new DateOnly(2025, 12, 1), ProjectCategory.Web, 12, featured: false,
            T("Kaira: AI Destekli Moda E-Ticaret", "Kaira: AI-Powered Fashion E-Commerce"),
            T("Dapper ile hızlı veri erişimi ve Google Gemini tabanlı akıllı alışveriş asistanı içeren e-ticaret sitesi.",
              "An e-commerce site with fast Dapper data access and a Google Gemini shopping assistant."),
            T("## Proje hakkında\n\nASP.NET Core 8 MVC ve Dapper ile geliştirdiğim moda e-ticaret uygulaması.\n\n" +
              "## Özellikler\n\n- **AI alışveriş asistanı:** \"Hafta sonu doğa yürüyüşü için ne giymeliyim?\" gibi sorulara veritabanındaki ürünlerden kombin öneriyor (Google Gemini)\n" +
              "- Dapper ile yüksek performanslı SQL sorguları\n- Ürün, kategori ve AI ayarlarının yönetildiği admin paneli",
              "## About\n\nA fashion e-commerce app built with ASP.NET Core 8 MVC and Dapper.\n\n" +
              "## Features\n\n- **AI shopping assistant:** answers questions like \"What should I wear for a weekend hike?\" with outfits from the product catalog (Google Gemini)\n" +
              "- High-performance SQL queries with Dapper\n- Admin panel for products, categories and AI settings"),
            ["ASP.NET Core 8 MVC", "Dapper", "SQL Server", "Google Gemini"], github: Repo("Kaira")),

        P("jadoo-travel-core", new DateOnly(2025, 10, 1), ProjectCategory.Web, 13, featured: false,
            T("JadooTravelCore: AI Destekli Seyahat Rezervasyon Sistemi", "JadooTravelCore: AI-Powered Travel Booking System"),
            T("ASP.NET Core 9.0 ve MongoDB ile geliştirilen, OpenAI API ile yapay zekâ destekli gezi önerileri sunan rezervasyon platformu.",
              "A booking platform built with ASP.NET Core 9.0 and MongoDB, offering AI-powered trip suggestions via the OpenAI API."),
            T("## Proje hakkında\n\nASP.NET Core 9.0 ve MongoDB ile geliştirdiğim full-stack seyahat rezervasyon platformu.\n\n" +
              "## Özellikler\n\n- OpenAI API ile şehir/ülke bazlı yapay zekâ destekli gezi önerileri\n- Translation API ile çok dilli içerik\n- Chart.js ile rezervasyon analizleri\n- Admin panelinden rezervasyon takibi ve onaylama\n\n" +
              "## Mimari\n\nController – Service – DTO – Entity katmanlı mimari, AutoMapper.",
              "## About\n\nA full-stack travel booking platform I built with ASP.NET Core 9.0 and MongoDB.\n\n" +
              "## Features\n\n- AI-powered city/country trip suggestions with the OpenAI API\n- Multilingual content with a Translation API\n- Booking analytics with Chart.js\n- Booking tracking and approval from the admin panel\n\n" +
              "## Architecture\n\nLayered Controller – Service – DTO – Entity architecture with AutoMapper."),
            ["ASP.NET Core 9", "MongoDB", "OpenAI API", "Chart.js"], github: Repo("JadooTravelCore")),

        P("blogy", new DateOnly(2025, 10, 1), ProjectCategory.Web, 14, featured: false,
            T("Blogy: AI Destekli Blog / CMS", "Blogy: AI-Powered Blog / CMS"),
            T("Makale üretimi, kurumsal içerik ve içerik moderasyonu için üç farklı OpenAI servisi kullanan N-Tier blog sistemi.",
              "An N-Tier blog system using three OpenAI services for article generation, corporate content and moderation."),
            T("## Proje hakkında\n\nN-Tier mimariyle geliştirdiğim, yapay zekâyı içerik süreçlerinin merkezine koyan bir içerik yönetim sistemi.\n\n" +
              "## Özellikler\n\n- **AI makale üretici:** yazar panelinde konu başlığından HTML formatında blog yazısı üretiliyor\n" +
              "- **Kurumsal içerik AI:** Hakkımızda, vizyon, misyon gibi alanlar için metin üretimi\n- **AI içerik moderatörü:** kullanıcı içeriklerini kontrol eden güvenlik katmanı",
              "## About\n\nA content management system built with an N-Tier architecture that puts AI at the center of the content workflow.\n\n" +
              "## Features\n\n- **AI article generator:** creates HTML blog posts from a topic in the writer panel\n" +
              "- **Corporate content AI:** generates text for About, Vision and Mission sections\n- **AI content moderator:** a safety layer that checks user-submitted content"),
            ["ASP.NET Core", "N-Tier", "OpenAI API"], github: Repo("Blogy")),

        P("financial-crm", new DateOnly(2025, 5, 1), ProjectCategory.Tool, 15, featured: false,
            T("FinancialCrm: Masaüstü Finans Yönetimi", "FinancialCrm: Desktop Finance Management"),
            T("Banka hesapları, harcama takibi ve fatura yönetimini tek dashboard'da birleştiren Windows Forms uygulaması.",
              "A Windows Forms app that brings bank accounts, expense tracking and bill management into a single dashboard."),
            T("## Proje hakkında\n\nWindows Forms ve .NET Framework ile geliştirdiğim masaüstü finans uygulaması. Veri katmanı SQL Server.\n\n" +
              "## Özellikler\n\n- Banka hesapları ve son işlemler\n- Kategori bazlı harcama takibi ve filtreleme\n- Fatura yönetimi\n- Genel durumu gösteren dashboard",
              "## About\n\nA desktop finance app built with Windows Forms and .NET Framework, with a SQL Server data layer.\n\n" +
              "## Features\n\n- Bank accounts and recent transactions\n- Category-based expense tracking and filtering\n- Bill management\n- A dashboard with the overall picture"),
            ["Windows Forms", ".NET Framework", "SQL Server"], github: Repo("FinancialCrm"))
    ];

    // ---------------- yardımcılar ----------------
    private static LocalizedText T(string tr, string en) => LocalizedText.Create(tr, en);

    private static string Repo(string name) => $"{Github}/{name}";

    private static Skill S(string name, SkillCategory category, int order, bool highlighted = false) =>
        new() { Name = name, Category = category, DisplayOrder = order, IsHighlighted = highlighted };

    private static Project P(
        string slug, DateOnly projectDate, ProjectCategory category, int order, bool featured,
        LocalizedText title, LocalizedText summary, LocalizedText content,
        List<string> technologies, string? github = null, string? store = null, bool published = true, string? live = null) => new()
    {
        Slug = slug,
        ProjectDate = projectDate,
        Category = category,
        DisplayOrder = order,
        IsFeatured = featured,
        IsPublished = published, // CarBook gibi devam eden projeler yayında değil, bitince admin panelinden açılır
        Title = title,
        Summary = summary,
        Content = content,
        Technologies = technologies,
        GithubUrl = github,
        LiveUrl = live,
        StoreUrl = store
    };
}
