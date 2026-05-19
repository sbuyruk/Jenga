using Microsoft.Extensions.Configuration;

namespace Jenga.BlazorUI.Endpoints
{
    public static class FotoProxyEndpoints
    {
        // Tekil HttpClient — her istekte new HttpClient() yaratmak socket tükenmesine yol açar.
        // UseDefaultCredentials = true → IIS App Pool kimliğiyle portal'a Windows Auth yapar.
        private static readonly HttpClient _client = new(
            new HttpClientHandler { UseDefaultCredentials = true })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static IEndpointRouteBuilder MapFotoProxy(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/foto/{dosyaAdi}", async (
                string dosyaAdi,
                IConfiguration config) =>
                    await ProxyFoto(dosyaAdi, config["SharePoint:FotoBaseUrl"], config["SharePoint:FotoUzanti"] ?? ".jpg"))
            .RequireAuthorization();

            app.MapGet("/api/bagisci-foto/{dosyaAdi}", async (
                string dosyaAdi,
                IConfiguration config) =>
                    await ProxyFoto(dosyaAdi, config["SharePoint:BagisciFotoBaseUrl"], config["SharePoint:FotoUzanti"] ?? ".jpg"))
            .RequireAuthorization();

            return app;
        }

        private static async Task<IResult> ProxyFoto(string dosyaAdi, string? baseUrl, string uzanti)
        {
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                return Results.BadRequest();

            // Güvenlik: path traversal engellemesi
            if (dosyaAdi.Contains('/') || dosyaAdi.Contains('\\') || dosyaAdi.Contains(".."))
                return Results.BadRequest();

            if (string.IsNullOrWhiteSpace(baseUrl))
                return Results.Problem("Fotoğraf base URL yapılandırılmamış.", statusCode: 500);

            var fullUrl = $"{baseUrl.TrimEnd('/')}/{dosyaAdi}{uzanti}";

            try
            {
                var response = await _client.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                    return Results.NotFound();

                var bytes       = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                return Results.Bytes(bytes, contentType);
            }
            catch (TaskCanceledException)
            {
                return Results.Problem("Fotoğraf sunucusuna bağlanılamadı (zaman aşımı).", statusCode: 504);
            }
            catch (HttpRequestException)
            {
                return Results.Problem("Fotoğraf sunucusuna erişilemiyor.", statusCode: 502);
            }
        }
    }
}
