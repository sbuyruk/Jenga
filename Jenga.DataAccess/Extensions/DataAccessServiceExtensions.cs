using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.FTK;
using Jenga.DataAccess.Services.IKYS;
using Jenga.DataAccess.Services.Inventory;
using Jenga.DataAccess.Services.NBYS;
using Jenga.DataAccess.Services.Search;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jenga.DataAccess.Extensions
{
    public static class DataAccessServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            services.AddSingleton<IDbContextScopeFactory, DbContextScopeFactory>();

            // ILogService kayıtlı değilse (ör. test ortamı, standalone kullanım) NullLogService devreye girer.
            services.TryAddSingleton<ILogService, NullLogService>();

            return services;
        }

        public static IServiceCollection AddCommonDataServices(this IServiceCollection services)
        {
            services.AddScoped<IMenuItemService, MenuItemService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IIlService, IlService>();
            services.AddScoped<IIlceService, IlceService>();
            services.AddScoped<IBolgeService, BolgeService>();

            return services;
        }

        public static IServiceCollection AddInventoryServices(this IServiceCollection services)
        {
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
            services.AddScoped<IMaterialBrandService, MaterialBrandService>();
            services.AddScoped<IMaterialModelService, MaterialModelService>();
            services.AddScoped<IMaterialEntryService, MaterialEntryService>();
            services.AddScoped<IMaterialUnitService, MaterialUnitService>();
            services.AddScoped<IMaterialInventoryService, MaterialInventoryService>();
            services.AddScoped<IMaterialMovementService, MaterialMovementService>();
            services.AddScoped<IMaterialExitService, MaterialExitService>();
            services.AddScoped<IMaterialTransferService, MaterialTransferService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IPersonelLocationService, PersonelLocationService>();
            services.AddScoped<IMaterialAssetService, MaterialAssetService>();
            services.AddScoped<IMaterialAssetLogService, MaterialAssetLogService>();

            return services;
        }

        public static IServiceCollection AddTbysServices(this IServiceCollection services)
        {
            services.AddScoped<ITasinmazService, TasinmazService>();
            services.AddScoped<ITasinmazBagisciService, TasinmazBagisciService>();
            services.AddScoped<IKiraciService, KiraciService>();
            services.AddScoped<IKiraSozlesmeService, KiraSozlesmeService>();
            services.AddScoped<ISozlesmeTasinmazService, SozlesmeTasinmazService>();
            services.AddScoped<IOdemePlaniService, OdemePlaniService>();
            services.AddScoped<IOdemeService, OdemeService>();
            services.AddScoped<IBagisciTalepleriService, BagisciTalepleriService>();
            services.AddScoped<IBagisciYakinlariService, BagisciYakinlariService>();
            services.AddScoped<ITasinmazTaahhutService, TasinmazTaahhutService>();
            services.AddScoped<IVasiyetciService, VasiyetciService>();
            services.AddScoped<IBagisService, BagisService>();
            services.AddScoped<ITbysSearchService, TbysSearchService>();

            return services;
        }

        public static IServiceCollection AddSearchServices(this IServiceCollection services)
        {
            services.AddScoped<IGlobalSearchService, GlobalSearchService>();

            return services;
        }

        public static IServiceCollection AddIkysServices(this IServiceCollection services)
        {
            services.AddScoped<IPersonelService, PersonelService>();
            services.AddScoped<IKimlikService, KimlikService>();
            services.AddScoped<IIsBilgileriService, IsBilgileriService>();
            services.AddScoped<IIletisimBilgileriService, IletisimBilgileriService>();
            services.AddScoped<IAileService, AileService>();
            services.AddScoped<IDereceKademeDegisimService, DereceKademeDegisimService>();
            services.AddScoped<IEgitimSeviyesiService, EgitimSeviyesiService>();
            services.AddScoped<IGorevOnayService, GorevOnayService>();
            services.AddScoped<IBirimTanimService, BirimTanimService>();
            services.AddScoped<IGorevTanimService, GorevTanimService>();
            services.AddScoped<IIzinTanimService, IzinTanimService>();
            services.AddScoped<IIzinDonemService, IzinDonemService>();
            services.AddScoped<IIzinTalepService, IzinTalepService>();
            services.AddScoped<IIzinHareketService, IzinHareketService>();
            services.AddScoped<IYabanciDilService, YabanciDilService>();
            services.AddScoped<ITahsilTanimService, TahsilTanimService>();

            return services;
        }

        public static IServiceCollection AddNbysServices(this IServiceCollection services)
        {
            services.AddScoped<INakitBagisciService, NakitBagisciService>();
            services.AddScoped<INakitBagisHareketService, NakitBagisHareketService>();
            services.AddScoped<IBankaTanimService, BankaTanimService>();
            services.AddScoped<IArmaganService, ArmaganService>();
            services.AddScoped<IArmaganTanimService, ArmaganTanimService>();
            services.AddScoped<IDuzenliNakitBagisciService, DuzenliNakitBagisciService>();
            services.AddScoped<IYasalFaizService, YasalFaizService>();

            return services;
        }

        public static IServiceCollection AddFtkServices(this IServiceCollection services)
        {
            services.AddScoped<IFtkService, FtkService>();
            services.AddScoped<IFtkIslemService, FtkIslemService>();
            services.AddScoped<IFtkKisiService, FtkKisiService>();

            return services;
        }
    }
}
