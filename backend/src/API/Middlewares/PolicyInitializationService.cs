using Auth.Application.Common.Security;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Enums;

namespace LibraryApp.API.Middlewares;

/// <summary>
/// Hosted service for initializing authorization policies on application startup.
/// </summary>
public class PolicyInitializationService(IPolicyLoader policyLoader, IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        // var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        
        // Verificar se precisa popular as permissions iniciais
        // if (!await permissionService.AnyAsync())
        // {
        //     await SeedDefaultPermissions(permissionService);
        // }

        // Carregar políticas na inicialização
        await policyLoader.LoadPoliciesAsync();
    }

    // private async Task SeedDefaultPermissions(IPermissionService permissionService)
    // {
    //     var defaultPermissions = new[]
    //     {
    //         Permission.Create(PermissionFeature.Book, PermissionAction.Create, "Create books"),
    //         Permission.Create(PermissionFeature.Book, PermissionAction.Read, "Read books"),
    //         Permission.Create(PermissionFeature.Book, PermissionAction.Update, "Update books"),
    //         Permission.Create(PermissionFeature.Book, PermissionAction.Delete, "Delete books"),
    //         // Adicione outras permissions padrão
    //     };

    //     foreach (var permission in defaultPermissions)
    //     {
    //         // await permissionService.AddAsync(permission);
    //     }
    // }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
