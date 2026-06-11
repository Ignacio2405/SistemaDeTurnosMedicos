using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.Services;

// Permite usar DateTime sin especificar UTC con PostgreSQL/Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Buscar wwwroot ANTES de crear el builder.
static string FindWwwroot()
{
    var baseDir = AppContext.BaseDirectory;
    var candidate = Path.Combine(baseDir, "wwwroot");
    if (Directory.Exists(candidate)) return candidate;
    var dir = new DirectoryInfo(baseDir);
    while (dir != null)
    {
        candidate = Path.Combine(dir.FullName, "wwwroot");
        if (Directory.Exists(candidate)) return candidate;
        dir = dir.Parent;
    }
    return Path.Combine(baseDir, "wwwroot");
}

var wwwrootPath = FindWwwroot();

var builder = WebApplication.CreateBuilder(args);

// Setear ContentRoot si corremos desde bin\
if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot")))
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
        dir = dir.Parent;
    if (dir != null)
        builder.WebHost.UseContentRoot(dir.FullName);
}

builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IEspecialidadService, EspecialidadService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
builder.Services.AddScoped<IEstudioService, EstudioService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ConexionSQL")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── Seed de datos iniciales ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed roles
    var rolesRequeridos = new[] { "Paciente", "Medico", "Recepcionista", "Administrador" };
    foreach (var rolNombre in rolesRequeridos)
    {
        if (!db.Roles.Any(r => r.Nombre == rolNombre))
            db.Roles.Add(new Rol { Nombre = rolNombre });
    }
    db.SaveChanges();

    // Seed especialidades médicas
    var especialidadesRequeridas = new[]
    {
        "Clínica Médica", "Cardiología", "Pediatría", "Ginecología",
        "Traumatología", "Neurología", "Dermatología", "Oftalmología",
        "Urología", "Psiquiatría", "Odontología", "Nutrición"
    };
    foreach (var espNombre in especialidadesRequeridas)
    {
        if (!db.Especialidades.Any(e => e.Nombre == espNombre))
            db.Especialidades.Add(new Especialidad { Nombre = espNombre });
    }
    db.SaveChanges();

    // Seed usuario administrador por defecto
    const string adminEmail = "admin@sistemasaludgoya.com";
    if (!db.Usuarios.Any(u => u.Email == adminEmail))
    {
        var adminUsuario = new Usuario
        {
            Nombre       = "Admin",
            Apellido     = "Sistema",
            Email        = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo       = true
        };
        db.Usuarios.Add(adminUsuario);
        db.SaveChanges();

        var rolAdmin = db.Roles.First(r => r.Nombre == "Administrador");
        db.UsuariosRoles.Add(new UsuarioRol { IdUsuario = adminUsuario.IdUsuario, IdRol = rolAdmin.IdRol });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
