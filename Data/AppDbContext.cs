using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medico> Medicos => Set<Medico>();

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuariosRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();

    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<MedicoEspecialidad> MedicosEspecialidades => Set<MedicoEspecialidad>();

    public DbSet<HorarioAtencion> HorariosAtencion => Set<HorarioAtencion>();

    public DbSet<Turno> Turnos => Set<Turno>();

    public DbSet<HistorialMedico> HistorialesMedicos => Set<HistorialMedico>();

    public DbSet<Consulta> Consultas => Set<Consulta>();

    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<Estudio> Estudios => Set<Estudio>();
    public DbSet<Documento> Documentos => Set<Documento>();

    public DbSet<Pago> Pagos => Set<Pago>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //--------------------------------------------------
        // USUARIO
        //--------------------------------------------------

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario");

            entity.HasKey(x => x.IdUsuario);

            entity.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Apellido)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        //--------------------------------------------------
        // PACIENTE
        //--------------------------------------------------

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("paciente");

            entity.HasKey(x => x.IdPaciente);

            entity.Property(x => x.Dni)
                .HasMaxLength(15)
                .IsRequired();

            entity.HasIndex(x => x.Dni)
                .IsUnique();

            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Paciente)
                .HasForeignKey<Paciente>(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IdUsuario)
                .IsUnique();
        });

        //--------------------------------------------------
        // MEDICO
        //--------------------------------------------------

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.ToTable("medico");

            entity.HasKey(x => x.IdMedico);

            entity.Property(x => x.Matricula)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(x => x.Matricula)
                .IsUnique();

            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Medico)
                .HasForeignKey<Medico>(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IdUsuario)
                .IsUnique();
        });

        //--------------------------------------------------
        // ROL
        //--------------------------------------------------

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("rol");

            entity.HasKey(x => x.IdRol);

            entity.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        //--------------------------------------------------
        // PERMISO
        //--------------------------------------------------

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("permiso");

            entity.HasKey(x => x.IdPermiso);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        //--------------------------------------------------
        // USUARIO_ROL
        //--------------------------------------------------

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("usuario_rol");

            entity.HasKey(x => new
            {
                x.IdUsuario,
                x.IdRol
            });

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.IdUsuario);

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.IdRol);
        });

        //--------------------------------------------------
        // ROL_PERMISO
        //--------------------------------------------------

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.ToTable("rol_permiso");

            entity.HasKey(x => new
            {
                x.IdRol,
                x.IdPermiso
            });

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.IdRol);

            entity.HasOne(x => x.Permiso)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.IdPermiso);
        });

        //--------------------------------------------------
        // ESPECIALIDAD
        //--------------------------------------------------

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.ToTable("especialidad");

            entity.HasKey(x => x.IdEspecialidad);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        //--------------------------------------------------
        // MEDICO_ESPECIALIDAD
        //--------------------------------------------------

        modelBuilder.Entity<MedicoEspecialidad>(entity =>
        {
            entity.ToTable("medico_especialidad");

            entity.HasKey(x => new
            {
                x.IdMedico,
                x.IdEspecialidad
            });

            entity.HasOne(x => x.Medico)
                .WithMany(x => x.Especialidades)
                .HasForeignKey(x => x.IdMedico);

            entity.HasOne(x => x.Especialidad)
                .WithMany(x => x.Medicos)
                .HasForeignKey(x => x.IdEspecialidad);
        });

        //--------------------------------------------------
        // HORARIO_ATENCION
        //--------------------------------------------------

        modelBuilder.Entity<HorarioAtencion>(entity =>
        {
            entity.ToTable("horario_atencion");

            entity.HasKey(x => x.IdHorarioAtencion);

            entity.HasOne(x => x.Medico)
                .WithMany(x => x.Horarios)
                .HasForeignKey(x => x.IdMedico);
        });

        //--------------------------------------------------
        // TURNO
        //--------------------------------------------------

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("turno");

            entity.HasKey(x => x.IdTurno);

            entity.Property(x => x.Estado)
                .HasConversion<int>();

            entity.HasOne(x => x.Paciente)
                .WithMany(x => x.Turnos)
                .HasForeignKey(x => x.IdPaciente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Medico)
                .WithMany(x => x.Turnos)
                .HasForeignKey(x => x.IdMedico)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //--------------------------------------------------
        // HISTORIAL_MEDICO
        //--------------------------------------------------

        modelBuilder.Entity<HistorialMedico>(entity =>
        {
            entity.ToTable("historial_medico");

            entity.HasKey(x => x.IdHistorialMedico);

            entity.HasOne(x => x.Paciente)
                .WithOne(x => x.HistorialMedico)
                .HasForeignKey<HistorialMedico>(x => x.IdPaciente);

            entity.HasIndex(x => x.IdPaciente)
                .IsUnique();
        });

        //--------------------------------------------------
        // CONSULTA
        //--------------------------------------------------

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.ToTable("consulta");

            entity.HasKey(x => x.IdConsulta);

            entity.Property(x => x.Diagnostico)
                .HasMaxLength(1000)
                .IsRequired();

            entity.HasOne(x => x.Turno)
                .WithOne(x => x.Consulta)
                .HasForeignKey<Consulta>(x => x.IdTurno);

            entity.HasIndex(x => x.IdTurno)
                .IsUnique();
        });

        //--------------------------------------------------
        // RECETA
        //--------------------------------------------------

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.ToTable("receta");

            entity.HasKey(x => x.IdReceta);

            entity.HasOne(x => x.Consulta)
                .WithMany(x => x.Recetas)
                .HasForeignKey(x => x.IdConsulta);
        });

        //--------------------------------------------------
        // ESTUDIO
        //--------------------------------------------------

        modelBuilder.Entity<Estudio>(entity =>
        {
            entity.ToTable("estudio");

            entity.HasKey(x => x.IdEstudio);

            entity.HasOne(x => x.Consulta)
                .WithMany(x => x.Estudios)
                .HasForeignKey(x => x.IdConsulta);
        });

        //--------------------------------------------------
        // DOCUMENTO
        //--------------------------------------------------

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("documento");

            entity.HasKey(x => x.IdDocumento);

            entity.HasOne(x => x.Consulta)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.IdConsulta);
        });

        //--------------------------------------------------
        // PAGO
        //--------------------------------------------------

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("pago");

            entity.HasKey(x => x.IdPago);

            entity.Property(x => x.Monto)
                .HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.Turno)
                .WithOne(x => x.Pago)
                .HasForeignKey<Pago>(x => x.IdTurno);

            entity.HasIndex(x => x.IdTurno)
                .IsUnique();
        });
    }
}
