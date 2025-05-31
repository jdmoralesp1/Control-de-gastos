using Microsoft.Extensions.DependencyInjection;
using PruebaTecnicaMVC.AccesoDatos.Data;
using PruebaTecnicaMVC.Modelos.Entities;


namespace PruebaTecnicaMVC.AccesoDatos.Seed;

public static class PruebaTecnicaDbContextInitialiser
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<PruebaTecnicaDbContext>();

        if(context.TiposGasto.FirstOrDefault() is null)
        {
            context.TiposGasto.Add(new TipoGasto
            {
                Nombre = "Seleccione una opción",
                UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"),
            });

            context.FondosMonetarios.Add(new FondoMonetario
            {
                Nombre = "Seleccione una opción",
                Tipo = "Por defecto",
                UsuarioId = Guid.Parse("e16bfd7a-a24b-40c6-92d4-dbddb739fb47"),
            });

            context.SaveChanges();
        }

    }
}
