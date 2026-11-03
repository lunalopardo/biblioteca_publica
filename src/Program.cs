using BibliotecaPublica.Helpers;
using BibliotecaPublica.Repositories;
using BibliotecaPublica.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Usuarios/Restringido";

        options.Events.OnRedirectToLogin = context =>
            EsAjax(context.Request)
                ? ResponderJson(context.Response, StatusCodes.Status401Unauthorized, "Tu sesión expiró. Volvé a iniciar sesión.")
                : RedirigirA(context);

        options.Events.OnRedirectToAccessDenied = context =>
            EsAjax(context.Request)
                ? ResponderJson(context.Response, StatusCodes.Status403Forbidden, "Solo un administrador puede realizar esta acción.")
                : RedirigirA(context);
    });

builder.Services.AddAuthorization(options =>
{
    // Todo requiere sesión iniciada, salvo lo marcado con [AllowAnonymous] (login y página de error)
    // Si alguien sin sesión entra a un link, la cookie lo manda a LoginPath con ?ReturnUrl=...
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Se usa con [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    options.AddPolicy(UsuarioActual.PolicyAdministrador, policy =>
        policy.RequireRole(nameof(EnRoles.Administrador)));
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioAfiliado>();
builder.Services.AddScoped<RepositorioAutor>();
builder.Services.AddScoped<RepositorioNacionalidad>();
builder.Services.AddScoped<RepositorioGenero>();
builder.Services.AddScoped<RepositorioLibro>();
builder.Services.AddScoped<RepositorioEditorial>();
builder.Services.AddScoped<RepositorioEjemplar>();
builder.Services.AddScoped<RepositorioSuscripcion>();
builder.Services.AddScoped<RepositorioPago>();
builder.Services.AddScoped<RepositorioPrestamo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// jQuery (Select2) manda este header solo; los fetch de Vue lo agregan en su función request
static bool EsAjax(HttpRequest request) =>
    request.Headers.XRequestedWith == "XMLHttpRequest";

static Task ResponderJson(HttpResponse response, int statusCode, string mensaje)
{
    response.StatusCode = statusCode;
    return response.WriteAsJsonAsync(new { message = mensaje });
}

// Comportamiento normal de la cookie para las páginas
static Task RedirigirA(Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions> context)
{
    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
}