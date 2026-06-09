# Guía de Desarrollo - Controllers, Services e Interfaces

## Configuración inicial después de clonar el repositorio

### Paso obligatorio: Configurar tu conexión segura (User Secrets)
Se migró la base de datos de SQL Server local a **PostgreSQL en la nube usando Supabase**. 
Para no exponer contraseñas en GitHub, usamos una herramienta nativa de Visual Studio llamada "User Secrets".

1. En el Explorador de Soluciones, hace **clic derecho sobre el proyecto** (`SistemaSaludGoya`).
2. Selecciona **"Administrar secretos del usuario"** (Manage User Secrets).
3. Se abrirá un archivo `secrets.json` vacío. Pega la estructura con la contraseña real que está en nuestro canal de comunicación.
4. Guarda y cierra el archivo secrets.json.

Ya podes ejecutar el proyecto, y este se va a conectar automáticamente a la base de datos en la nube.

## Arquitectura utilizada

El proyecto utiliza una arquitectura basada en:

```text
Controller
    ↓
Service
    ↓
Entity Framework Core
    ↓
Base de Datos
```

La idea es mantener los controladores limpios y delegar toda la lógica de negocio a los servicios.

---

# Creación de Interfaces

Cada servicio debe tener una interfaz asociada.

Las interfaces permiten desacoplar la implementación del controlador.

## Ejemplo

### IUsuarioService.cs

```csharp
public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerTodosAsync();

    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task CrearAsync(Usuario usuario);

    Task ActualizarAsync(Usuario usuario);

    Task EliminarAsync(int id);
}
```

---

# Creación de Services

Los Services contienen la lógica de negocio y el acceso a los datos.

## Ejemplo

### UsuarioService.cs

```csharp
public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.IdUsuario == id);
    }

    public async Task CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);

        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return;

        _context.Usuarios.Remove(usuario);

        await _context.SaveChangesAsync();
    }
}
```

---

# Registro de Servicios en Program.cs

Para que ASP.NET Core pueda inyectar los servicios, es necesario registrarlos.

## Ejemplo

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IMedicoService, MedicoService>();

builder.Services.AddScoped<IPacienteService, PacienteService>();

builder.Services.AddScoped<ITurnoService, TurnoService>();
```

---

# Inyección de Dependencias

Una vez registrado el servicio, puede utilizarse en cualquier controlador mediante el constructor.

## Ejemplo

```csharp
public class UsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(
        IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }
}
```

ASP.NET Core se encargará automáticamente de crear la instancia correspondiente.

---

# Implementación en Controladores

Los controladores únicamente reciben solicitudes HTTP y llaman a los servicios.

## Ejemplo

### UsuarioController

```csharp
[Authorize]
public class UsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(
        IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios =
            await _usuarioService.ObtenerTodosAsync();

        return View(usuarios);
    }

    public async Task<IActionResult> Details(int id)
    {
        var usuario =
            await _usuarioService.ObtenerPorIdAsync(id);

        if (usuario == null)
            return NotFound();

        return View(usuario);
    }
}
```

---

# Ejemplo completo del flujo

## 1. El usuario accede a una URL

```text
/Usuario/Index
```

---

## 2. El Controller recibe la solicitud

```csharp
public async Task<IActionResult> Index()
{
    var usuarios =
        await _usuarioService.ObtenerTodosAsync();

    return View(usuarios);
}
```

---

## 3. El Service ejecuta la lógica

```csharp
public async Task<List<Usuario>> ObtenerTodosAsync()
{
    return await _context.Usuarios.ToListAsync();
}
```

---

## 4. Entity Framework consulta la base de datos

```sql
SELECT * FROM usuario
```

---

## 5. El resultado vuelve al controlador

```csharp
return View(usuarios);
```

---

## 6. La vista muestra los datos

```cshtml
@model List<Usuario>
```

---

# Flujo recomendado para crear un nuevo módulo

Cada vez que se agregue una nueva funcionalidad se recomienda seguir el siguiente orden:

1. Crear la interfaz (`IEntidadService`)
2. Crear el servicio (`EntidadService`)
3. Registrar el servicio en `Program.cs`
4. Crear el controlador (`EntidadController`)
5. Inyectar el servicio en el controlador
6. Implementar las acciones del controlador
7. Crear las vistas correspondientes
8. Probar la funcionalidad

---

# Ejemplo para cualquier módulo

Supongamos que se desea implementar especialidades.

### Paso 1

Crear:

```text
Interfaces
└── IEspecialidadService.cs
```

### Paso 2

Crear:

```text
Services
└── EspecialidadService.cs
```

### Paso 3

Registrar:

```csharp
builder.Services.AddScoped<
    IEspecialidadService,
    EspecialidadService>();
```

### Paso 4

Crear:

```text
Controllers
└── EspecialidadController.cs
```

### Paso 5

Inyectar:

```csharp
private readonly IEspecialidadService _especialidadService;
```

### Paso 6

Utilizar el servicio dentro de las acciones.

Este mismo patrón debe repetirse para Médicos, Pacientes, Turnos, Consultas, Recetas, Estudios y cualquier otra funcionalidad del sistema.
