using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaMVC.Modelos.DTOs;

namespace PruebaTecnicaMVC.Areas.Identity.Controllers;
[Area("Identity")]
public class LoginController : Controller
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IConfiguration configuration;

    public LoginController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

        if (result.Succeeded)
        {
            var user = await userManager.FindByNameAsync(model.UserName);
            await signInManager.SignInAsync(user, isPersistent: false); // Esto crea la cookie de autenticación
            return RedirectToAction("Index", "Home", new { area = "Dashboard" });
        }


        return Unauthorized();
    }
}
