using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Common.Error;
using Jenga.BlazorUI.Services.Common.Toast;
using Jenga.BlazorUI.Services.Presence;
using Jenga.Utility.Logging;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Jenga.BlazorUI.Extensions
{
    public static class BlazorUIServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IToastService, ToastService>();
            // ILogWriter ve ILogService Singleton: FileLogWriter thread-safe (static Lock),
            // LogService yalnızca ILogWriter[] tutar — mutable state yok.
            // Her circuit için yeni instance oluşturmanın maliyeti ve gereksizliği ortadan kalkar.
            services.AddSingleton<ILogWriter, FileLogWriter>();
            services.AddSingleton<ILogService, LogService>();
            services.AddScoped<IErrorService, ErrorService>();

            // Global exception handling (HTTP pipeline). ExceptionHandlerMiddleware
            // IExceptionHandler'ı root provider üzerinden resolve ettiği için Singleton kaydı zorunlu.
            // Scoped servislere (ör. ILogService) IServiceScopeFactory ile erişilir.
            services.AddSingleton<IExceptionHandler, Infrastructure.GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddScoped<IModalService, ModalService>();
            services.AddScoped<MenuStateService>();
            services.AddScoped<CurrentUserService>();
            services.AddScoped<ImpersonationService>();

            return services;
        }

        public static IServiceCollection AddPresenceServices(this IServiceCollection services)
        {
            services.AddScoped<CircuitHandler, PresenceCircuitHandler>();
            services.AddScoped<UserPresenceQueryService>();
            services.AddScoped<PresenceHeartbeatState>();
            services.AddScoped<PresenceHeartbeatService>();
            services.AddScoped<UserNavigationLogService>();

            return services;
        }
    }
}
