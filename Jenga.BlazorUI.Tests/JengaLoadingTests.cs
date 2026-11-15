using System.Net;
using Jenga.BlazorUI.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jenga.BlazorUI.Tests;

public sealed class JengaLoadingTests
{
    [Fact]
    public async Task SplashShowsBrandExpansionAndCreditWithDotsInsideTheHeading()
    {
        var html = await RenderAsync(new() { [nameof(JengaLoading.IsSplash)] = true });

        Assert.Contains("jenga-loading--splash", html);
        Assert.Contains("Çalışma alanınız hazırlanıyor", html);
        Assert.Contains("Jenga™ — Bilgi Sistemleri tarafından geliştirilmiştir.", html);
        Assert.Contains("<strong>J</strong>oint", html);
        Assert.Contains("<strong>A</strong>pplications", html);
        Assert.DoesNotContain("Jenga başlatılıyor", html);
        Assert.DoesNotContain("Lütfen bekleyin", html);
        var heading = html[html.IndexOf("<h2", StringComparison.Ordinal)..html.IndexOf("</h2>", StringComparison.Ordinal)];
        Assert.Contains("jenga-loading__dots", heading);
        Assert.Equal(1, html.Split("jenga-brand__symbol").Length - 1);
    }

    [Fact]
    public async Task WaitingShowsOnlyAnimatedSymbolAndDotsWithAccessibleOperationMessage()
    {
        var html = await RenderAsync(new() { [nameof(JengaLoading.Message)] = "Bağış kayıtları getiriliyor." });

        Assert.Contains("jenga-loading--waiting", html);
        Assert.Contains("aria-label=\"Bağış kayıtları getiriliyor.\"", html);
        Assert.Contains("jenga-brand__symbol", html);
        Assert.Contains("jenga-loading__dots", html);
        Assert.DoesNotContain("jenga-brand__wordmark", html);
        Assert.DoesNotContain("<img", html);
        Assert.DoesNotContain("jenga-loading__expansion", html);
        Assert.DoesNotContain("jenga-loading__title", html);
        Assert.DoesNotContain("jenga-loading__subtitle", html);
        Assert.DoesNotContain("İşleminiz devam ediyor", html);
        Assert.DoesNotContain("Lütfen bekleyin", html);
        Assert.DoesNotContain("jenga-loading__footer", html);
        Assert.DoesNotContain("Bilgi Sistemleri", html);
        Assert.DoesNotContain("Çalışma alanınız hazırlanıyor", html);
    }

    [Fact]
    public async Task InlineIndicatorCanBePlacedInsideAButtonWithoutAnOverlay()
    {
        var html = await RenderAsync(new() { [nameof(JengaLoading.Inline)] = true });

        Assert.StartsWith("<span", html);
        Assert.Contains("jenga-loading-inline", html);
        Assert.Contains("role=\"status\"", html);
        Assert.DoesNotContain("<div", html);
        Assert.DoesNotContain("jenga-loading--waiting", html);
        Assert.DoesNotContain("jenga-loading__footer", html);
    }

    private static async Task<string> RenderAsync(Dictionary<string, object?> parameters)
    {
        await using var provider = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync<JengaLoading>(ParameterView.FromDictionary(parameters));
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
    }
}
