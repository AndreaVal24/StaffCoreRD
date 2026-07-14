using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StaffCoreRD.Data;

// Crear el builder de la aplicación
var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext: vincula la BD con la cadena de conexión "StaffCore" de appsettings.json
builder.Services.AddDbContext<StaffDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StaffCore")));

// Agregar Identity: configura usuarios, contraseñas, roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<StaffDbContext>(); // Usa StaffDbContext para guardar usuarios

// Agregar controladores y Razor Views
builder.Services.AddControllersWithViews();

// Configurar la cookie de autenticación: si intentas acceder a una ruta protegida sin login, redirige aquí
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

var app = builder.Build();

// Crear roles automáticamente al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Array de roles que necesitas
    string[] roles = { "Administrador", "RRHH", "Viewer" };

    // Por cada rol, verifica si existe; si no, lo crea
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Middleware: redirige HTTP a HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Middleware: sirve archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// Middleware: enrutamiento
app.UseRouting();

// Middleware: PRIMERO autenticación, LUEGO autorización (el orden importa)
app.UseAuthentication();
app.UseAuthorization();

// Mapear la ruta por defecto: Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Iniciar la app
app.Run();