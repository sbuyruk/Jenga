using Jenga.Models.Enums;
using Jenga.Utility.Results;
using System.Reflection;

namespace Jenga.DataAccess.Services.Common
{
    /// <summary>
    /// Herhangi bir servis interface'inin önüne şeffaf olarak oturan yetki proxy'si.
    /// <see cref="IServiceAuthorizationContext"/> üzerinden izin kontrolü yapar;
    /// diğer metodlar doğrudan iletilir. Hiçbir servis koduna dokunulmaz.
    /// <para><b>Metod adı → Operasyon eşlemesi (prefix kuralı):</b></para>
    /// <list type="bullet">
    ///   <item><c>Get*</c> / <c>Any*</c> → <see cref="Operation.View"/></item>
    ///   <item><c>AddAsync</c>           → <see cref="Operation.Create"/></item>
    ///   <item><c>UpdateAsync</c>        → <see cref="Operation.Edit"/></item>
    ///   <item><c>DeleteAsync</c>        → <see cref="Operation.Delete"/></item>
    ///   <item>Diğer metodlar           → izin kontrolü yapılmaz, doğrudan iletilir</item>
    /// </list>
    /// <para>
    /// <see cref="Operation.Manage"/> üst-izni tüm operasyonları kapsar;
    /// dolayısıyla Manage iznine sahip kullanıcı proxy'nin engellediği hiçbir metoda takılmaz.
    /// </para>
    /// </summary>
    public class ServiceAuthorizationProxy<TService> : DispatchProxy
        where TService : class
    {
        private TService _inner = null!;
        private IServiceAuthorizationContext _authContext = null!;
        private ModuleName _module;

        /// <summary>
        /// Proxy örneği oluşturur ve iç bağımlılıkları enjekte eder.
        /// </summary>
        public static TService Create(
            TService inner,
            IServiceAuthorizationContext authContext,
            ModuleName module)
        {
            var proxy = Create<TService, ServiceAuthorizationProxy<TService>>()
                        as ServiceAuthorizationProxy<TService>
                        ?? throw new InvalidOperationException("Proxy oluşturulamadı.");

            proxy._inner = inner;
            proxy._authContext = authContext;
            proxy._module = module;

            return (proxy as TService)!;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                return null;

            var operation = ResolveOperation(targetMethod.Name);

            if (operation.HasValue)
            {
                // Async yetki kontrolü: Task dönen metod, await edilebilen bir Task'e sarılır.
                // Metodun son argümanı CancellationToken ise yetki sorgusuna da iletilir.
                var token = args is { Length: > 0 } && args[^1] is CancellationToken ct
                    ? ct
                    : CancellationToken.None;

                return InvokeWithAuthCheckAsync(targetMethod, args, operation.Value, token);
            }

            return targetMethod.Invoke(_inner, args);
        }

        private object InvokeWithAuthCheckAsync(
            MethodInfo targetMethod,
            object?[]? args,
            Operation operation,
            CancellationToken cancellationToken)
        {
            // Dönüş tipine göre doğru Task<Result> / Task<Result<T>> tipini üret.
            var returnType = targetMethod.ReturnType;

            // Task<Result<T>> mi yoksa Task<Result> mi?
            if (returnType.IsGenericType && returnType.GetGenericArguments()[0] is { } innerType
                && innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = innerType.GetGenericArguments()[0];
                var helperMethod = typeof(ServiceAuthorizationProxy<TService>)
                    .GetMethod(nameof(CheckAndInvokeGenericAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(valueType);

                return helperMethod.Invoke(this, [targetMethod, args, operation, cancellationToken])!;
            }

            // Task<Result>
            return CheckAndInvokeAsync(targetMethod, args, operation, cancellationToken);
        }

        private async Task<Result> CheckAndInvokeAsync(
            MethodInfo targetMethod,
            object?[]? args,
            Operation operation,
            CancellationToken cancellationToken)
        {
            var allowed = await _authContext.HasPermissionAsync(_module, operation, cancellationToken);
            if (!allowed)
                return Result.Failure(Error.Forbidden(
                    $"Bu işlem için gerekli yetki bulunamadı ({_module}/{operation}).",
                    $"{_module}.{operation}.Forbidden"));

            return await (Task<Result>)targetMethod.Invoke(_inner, args)!;
        }

        private async Task<Result<T>> CheckAndInvokeGenericAsync<T>(
            MethodInfo targetMethod,
            object?[]? args,
            Operation operation,
            CancellationToken cancellationToken)
        {
            var allowed = await _authContext.HasPermissionAsync(_module, operation, cancellationToken);
            if (!allowed)
                return Result.Failure<T>(Error.Forbidden(
                    $"Bu işlem için gerekli yetki bulunamadı ({_module}/{operation}).",
                    $"{_module}.{operation}.Forbidden"));

            return await (Task<Result<T>>)targetMethod.Invoke(_inner, args)!;
        }

        /// <summary>
        /// Metod adından operasyon türünü çözer.
        /// <c>Get*</c> / <c>Any*</c> → View; sabit isimler → Create/Edit/Delete.
        /// Eşleşme yoksa null döner ve metod izin kontrolü yapılmadan iletilir.
        /// </summary>
        private static Operation? ResolveOperation(string methodName)
        {
            if (methodName.StartsWith("Get", StringComparison.Ordinal) ||
                methodName.StartsWith("Any", StringComparison.Ordinal))
                return Operation.View;

            return methodName switch
            {
                "AddAsync"    => Operation.Create,
                "UpdateAsync" => Operation.Edit,
                "DeleteAsync" => Operation.Delete,
                _             => null
            };
        }
    }
}
