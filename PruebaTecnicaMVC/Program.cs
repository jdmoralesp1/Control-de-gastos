using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaMVC.AccesoDatos.Data;
using PruebaTecnicaMVC.AccesoDatos.Seed;
using PruebaTecnicaMVC.Aplicacion.DependencyInjection;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddDbContext<PruebaTecnicaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PruebaTecnicaDbConnection"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4; // O el mínimo que desees
    })
.AddEntityFrameworkStores<PruebaTecnicaDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddCookie();

builder.Services.AddHttpContextAccessor();

#region Services

object value = builder.Services.AddApplicationLayer();

#endregion

WebApplication? app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<PruebaTecnicaDbContext>();
    context.Database.Migrate();
    await SeedUserData.Initialize(services);
    PruebaTecnicaDbContextInitialiser.Initialize(services);
}

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-ES");

var supportedCultures = new[] { new CultureInfo("es-ES") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Dashboard/Home/ErrorGenerico/{0}");

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Identity}/{controller=Login}/{action=Index}/{id?}");

app.Run();
