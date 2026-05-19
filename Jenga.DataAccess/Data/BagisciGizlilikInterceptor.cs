using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Jenga.DataAccess.Data
{
    /// <summary>
    /// EF Core, TasinmazBagisci nesnesini DB'den her materialize ettiğinde
    /// otomatik olarak Maskele() çağırır. Hangi servis veya sorgu olursa olsun
    /// gizli bağışçıların hassas alanları açığa çıkmaz.
    /// </summary>
    public sealed class BagisciGizlilikInterceptor : IMaterializationInterceptor
    {
        public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
        {
            if (instance is TasinmazBagisci bagisci)
                bagisci.Maskele();

            return instance;
        }
    }
}
