using Jenga.BlazorUI.Services.Common;
using Jenga.BlazorUI.Services.Presence;
using Jenga.Utility.Error;
using Jenga.Utility.Logging;
using Jenga.Utility.Toast;
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
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<ILogWriter, FileLogWriter>();
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
