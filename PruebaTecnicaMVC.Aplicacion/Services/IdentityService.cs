using Microsoft.AspNetCore.Http;
using PruebaTecnicaMVC.Aplicacion.Services.Contracts;
using System.Security.Claims;

namespace PruebaTecnicaMVC.Aplicacion.Services;
public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid ObtenerUsuarioId()
    {
        string userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "e16bfd7a-a24b-40c6-92d4-dbddb739fb47";

        return Guid.Parse(userId);
    }
}
