using PortfolioPro.Application.Common.Models;
using PortfolioPro.Application.Features.Auth;
using PortfolioPro.Application.Features.Messages;
using PortfolioPro.Application.Features.Projects;
using PortfolioPro.Domain.Enums;

namespace PortfolioPro.UnitTests;

public class ValidatorTests
{
    private static SaveProjectCommand ValidProject() => new(
        Id: null,
        Slug: null,
        Title: new LocalizedDto("Proje", "Project"),
        Summary: new LocalizedDto("Özet", "Summary"),
        Content: new LocalizedDto("", ""),
        Category: ProjectCategory.Web,
        CoverImageUrl: null,
        GithubUrl: "https://github.com/erselgemici",
        LiveUrl: null,
        StoreUrl: null,
        Technologies: [".NET", "Angular"],
        ProjectDate: new DateOnly(2026, 1, 1),
        IsFeatured: false,
        IsPublished: true,
        DisplayOrder: 1);

    [Fact]
    public void SaveProject_ShouldPass_WhenCommandIsValid()
    {
        var result = new SaveProjectCommandValidator().Validate(ValidProject());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void SaveProject_ShouldFail_WhenLinkIsJavascript()
    {
        // XSS denemesi: link alanına javascript: yazılırsa reddedilmeli
        var command = ValidProject() with { LiveUrl = "javascript:alert(1)" };

        var result = new SaveProjectCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveProjectCommand.LiveUrl));
    }

    [Fact]
    public void SaveProject_ShouldFail_WhenSlugHasInvalidCharacters()
    {
        var command = ValidProject() with { Slug = "Benim Projem!" };

        var result = new SaveProjectCommandValidator().Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void SaveProject_ShouldFail_WhenEnglishTitleMissing()
    {
        var command = ValidProject() with { Title = new LocalizedDto("Proje", "") };

        var result = new SaveProjectCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == "Title.En");
    }

    [Fact]
    public void ContactMessage_ShouldFail_WhenEmailInvalid()
    {
        var command = new SendContactMessageCommand("Ersel", "gecersiz-mail", null, "Merhaba, bir proje hakkında konuşalım.", null);

        var result = new SendContactMessageCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SendContactMessageCommand.Email));
    }

    [Theory]
    [InlineData("kisa1A")]            // 10 karakterden kısa
    [InlineData("tamamenkucuk123")]   // büyük harf yok
    [InlineData("SifreRakamsiz")]     // rakam yok
    public void ChangePassword_ShouldRejectWeakPasswords(string newPassword)
    {
        var command = new ChangePasswordCommand(Guid.NewGuid(), "EskiSifre123", newPassword);

        var result = new ChangePasswordCommandValidator().Validate(command);

        Assert.False(result.IsValid);
    }
}
