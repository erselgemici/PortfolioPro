using System.Linq.Expressions;
using PortfolioPro.Application.Common.Models;
using PortfolioPro.Domain.Entities;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.Application.Features.Projects;

// Listeleme için hafif DTO. Uzun markdown içeriği (Content) burada YOK,
// 10 proje listelerken her birinin tam metnini çekmek gereksiz veri transferi olurdu.
public sealed record ProjectListItemDto(
    Guid Id,
    string Slug,
    LocalizedDto Title,
    LocalizedDto Summary,
    ProjectCategory Category,
    string? CoverImageUrl,
    string? GithubUrl,
    string? LiveUrl,
    string? StoreUrl,
    List<string> Technologies,
    DateOnly ProjectDate,
    bool IsFeatured)
{
    public static readonly Expression<Func<Project, ProjectListItemDto>> Projection = p => new ProjectListItemDto(
        p.Id,
        p.Slug,
        new LocalizedDto(p.Title.Tr, p.Title.En),
        new LocalizedDto(p.Summary.Tr, p.Summary.En),
        p.Category,
        p.CoverImageUrl,
        p.GithubUrl,
        p.LiveUrl,
        p.StoreUrl,
        p.Technologies,
        p.ProjectDate,
        p.IsFeatured);
}

// Detay sayfası için tam içerik
public sealed record ProjectDetailDto(
    Guid Id,
    string Slug,
    LocalizedDto Title,
    LocalizedDto Summary,
    LocalizedDto Content,
    ProjectCategory Category,
    string? CoverImageUrl,
    string? GithubUrl,
    string? LiveUrl,
    string? StoreUrl,
    List<string> Technologies,
    DateOnly ProjectDate)
{
    public static readonly Expression<Func<Project, ProjectDetailDto>> Projection = p => new ProjectDetailDto(
        p.Id,
        p.Slug,
        new LocalizedDto(p.Title.Tr, p.Title.En),
        new LocalizedDto(p.Summary.Tr, p.Summary.En),
        new LocalizedDto(p.Content.Tr, p.Content.En),
        p.Category,
        p.CoverImageUrl,
        p.GithubUrl,
        p.LiveUrl,
        p.StoreUrl,
        p.Technologies,
        p.ProjectDate);
}

// Admin paneli için tüm alanlar (yayında olmayanlar dahil)
public sealed record ProjectAdminDto(
    Guid Id,
    string Slug,
    LocalizedDto Title,
    LocalizedDto Summary,
    LocalizedDto Content,
    ProjectCategory Category,
    string? CoverImageUrl,
    string? GithubUrl,
    string? LiveUrl,
    string? StoreUrl,
    List<string> Technologies,
    DateOnly ProjectDate,
    bool IsFeatured,
    bool IsPublished,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static readonly Expression<Func<Project, ProjectAdminDto>> Projection = p => new ProjectAdminDto(
        p.Id,
        p.Slug,
        new LocalizedDto(p.Title.Tr, p.Title.En),
        new LocalizedDto(p.Summary.Tr, p.Summary.En),
        new LocalizedDto(p.Content.Tr, p.Content.En),
        p.Category,
        p.CoverImageUrl,
        p.GithubUrl,
        p.LiveUrl,
        p.StoreUrl,
        p.Technologies,
        p.ProjectDate,
        p.IsFeatured,
        p.IsPublished,
        p.DisplayOrder,
        p.CreatedAt,
        p.UpdatedAt);
}
