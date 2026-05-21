using Jenga.DataAccess.Data;
using Jenga.DataAccess.Services.Common;
using Jenga.DataAccess.Services.FTK;
using Jenga.DataAccess.Services.IKYS;
using Jenga.DataAccess.Services.Inventory;
using Jenga.DataAccess.Services.NBYS;
using Jenga.DataAccess.Services.Search;
using Jenga.DataAccess.Services.TBYS;
using Jenga.Models.Enums;
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
        /// <summary>
        /// Servisi doğrudan kaydeder, ardından DI'dan çözümlendiğinde
        /// <see cref="ServiceAuthorizationProxy{TService}"/> ile sarar.
        /// AddAsync → Create, UpdateAsync → Edit, DeleteAsync → Delete yetki kontrolleri
        /// otomatik olarak eklenir; diğer metodlar değişmeden iletilir.
        /// </summary>
        private static IServiceCollection AddScopedWithAuthProxy<TInterface, TImpl>(
            this IServiceCollection services,
            ModuleName module)
            where TInterface : class
            where TImpl : class, TInterface
        {
            services.AddScoped<TImpl>();
            services.AddScoped<TInterface>(sp =>
            {
                var inner = sp.GetRequiredService<TImpl>();
                var authContext = sp.GetService<IServiceAuthorizationContext>();
                if (authContext is null)
                    return inner;
                return ServiceAuthorizationProxy<TInterface>.Create(inner, authContext, module);
            });
            return services;
        }

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
            services.AddScopedWithAuthProxy<IMenuItemService, MenuItemService>(ModuleName.Admin);
            services.AddScopedWithAuthProxy<IRoleService, RoleService>(ModuleName.Admin);
            // Il, Ilce, Bolge: salt okunur referans verisi — proxy gerekmez
            services.AddScoped<IIlService, IlService>();
            services.AddScoped<IIlceService, IlceService>();
            services.AddScoped<IBolgeService, BolgeService>();
            services.AddScopedWithAuthProxy<IModulePermissionService, ModulePermissionService>(ModuleName.Admin);
            services.AddScopedWithAuthProxy<IRoleModulePermissionService, RoleModulePermissionService>(ModuleName.Admin);
            services.AddScopedWithAuthProxy<IPersonnelRegionPermissionService, PersonnelRegionPermissionService>(ModuleName.Admin);

            return services;
        }

        public static IServiceCollection AddInventoryServices(this IServiceCollection services)
        {
            services.AddScopedWithAuthProxy<IMaterialService, MaterialService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialCategoryService, MaterialCategoryService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialBrandService, MaterialBrandService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialModelService, MaterialModelService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialEntryService, MaterialEntryService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialUnitService, MaterialUnitService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialInventoryService, MaterialInventoryService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialMovementService, MaterialMovementService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialExitService, MaterialExitService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialTransferService, MaterialTransferService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<ILocationService, LocationService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IPersonelLocationService, PersonelLocationService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialAssetService, MaterialAssetService>(ModuleName.Inventory);
            services.AddScopedWithAuthProxy<IMaterialAssetLogService, MaterialAssetLogService>(ModuleName.Inventory);

            return services;
        }

        public static IServiceCollection AddTbysServices(this IServiceCollection services)
        {
            services.AddScopedWithAuthProxy<ITasinmazService, TasinmazService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<ITasinmazBagisciService, TasinmazBagisciService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IKiraciService, KiraciService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IKiraSozlesmeService, KiraSozlesmeService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<ISozlesmeTasinmazService, SozlesmeTasinmazService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IOdemePlaniService, OdemePlaniService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IOdemeService, OdemeService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IBagisciTalepleriService, BagisciTalepleriService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IBagisciYakinlariService, BagisciYakinlariService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<ITasinmazTaahhutService, TasinmazTaahhutService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IVasiyetciService, VasiyetciService>(ModuleName.TBYS);
            services.AddScopedWithAuthProxy<IBagisService, BagisService>(ModuleName.TBYS);
            // TbysSearchService: salt okunur arama — proxy gerekmez
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
            services.AddScopedWithAuthProxy<IPersonelService, PersonelService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IKimlikService, KimlikService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIsBilgileriService, IsBilgileriService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIletisimBilgileriService, IletisimBilgileriService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IAileService, AileService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IDereceKademeDegisimService, DereceKademeDegisimService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IEgitimSeviyesiService, EgitimSeviyesiService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IGorevOnayService, GorevOnayService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IBirimTanimService, BirimTanimService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IGorevTanimService, GorevTanimService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIzinTanimService, IzinTanimService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIzinDonemService, IzinDonemService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIzinTalepService, IzinTalepService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IIzinHareketService, IzinHareketService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<IYabanciDilService, YabanciDilService>(ModuleName.IKYS);
            services.AddScopedWithAuthProxy<ITahsilTanimService, TahsilTanimService>(ModuleName.IKYS);

            return services;
        }

        public static IServiceCollection AddNbysServices(this IServiceCollection services)
        {
            services.AddScopedWithAuthProxy<INakitBagisciService, NakitBagisciService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<INakitBagisHareketService, NakitBagisHareketService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<IBankaTanimService, BankaTanimService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<IArmaganService, ArmaganService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<IArmaganTanimService, ArmaganTanimService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<IDuzenliNakitBagisciService, DuzenliNakitBagisciService>(ModuleName.NBYS);
            services.AddScopedWithAuthProxy<IYasalFaizService, YasalFaizService>(ModuleName.NBYS);

            return services;
        }

        public static IServiceCollection AddFtkServices(this IServiceCollection services)
        {
            services.AddScopedWithAuthProxy<IFtkService, FtkService>(ModuleName.FTK);
            services.AddScopedWithAuthProxy<IFtkIslemService, FtkIslemService>(ModuleName.FTK);
            services.AddScopedWithAuthProxy<IFtkKisiService, FtkKisiService>(ModuleName.FTK);

            return services;
        }
    }
}
