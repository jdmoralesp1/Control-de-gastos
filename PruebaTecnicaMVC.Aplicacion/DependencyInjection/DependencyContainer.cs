using Microsoft.Extensions.DependencyInjection;
using PruebaTecnicaMVC.AccesoDatos.Repositories.Repository;
using PruebaTecnicaMVC.AccesoDatos.Repositories.TiposGasto;
using PruebaTecnicaMVC.AccesoDatos.UnitOfWork;
using PruebaTecnicaMVC.Aplicacion.Services;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;

namespace PruebaTecnicaMVC.Aplicacion.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<ITipoGastoRepository, TipoGastoRepository>();


        // Services
        services.AddScoped<IGastoService, GastoService>();
        return services;
    }
}
