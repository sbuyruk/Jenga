using Jenga.DataAccess.Services.TBYS;

namespace Jenga.BlazorUI.Endpoints
{
    /// <summary>
    /// Taşınmaz kontrol eklerini indirir. Yetki (TBYS/View) ve bölge denetimi
    /// <see cref="ITasinmazKontrolService.EkAcAsync"/> içinde sunucu tarafında yapılır.
    /// </summary>
    public static class KontrolEkEndpoints
    {
        public static IEndpointRouteBuilder MapKontrolEkIndir(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/tbys/kontrol-ek/{ekId:int}", async (int ekId, ITasinmazKontrolService service, CancellationToken ct) =>
            {
                var result = await service.EkAcAsync(ekId, ct);
                if (result.IsFailure)
                {
                    return result.Error.Code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
                        ? Results.Forbid()
                        : Results.NotFound();
                }

                var (icerik, ek) = result.Value;
                return Results.File(icerik, ek.IcerikTipi ?? "application/octet-stream", ek.DosyaAdi, enableRangeProcessing: false);
            })
            .RequireAuthorization();

            return app;
        }
    }
}
