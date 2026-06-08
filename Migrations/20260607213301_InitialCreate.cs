using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSaludGoya.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "especialidad",
                columns: table => new
                {
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_especialidad", x => x.IdEspecialidad);
                });

            migrationBuilder.CreateTable(
                name: "permiso",
                columns: table => new
                {
                    IdPermiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permiso", x => x.IdPermiso);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "rol_permiso",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    IdPermiso = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permiso", x => new { x.IdRol, x.IdPermiso });
                    table.ForeignKey(
                        name: "FK_rol_permiso_permiso_IdPermiso",
                        column: x => x.IdPermiso,
                        principalTable: "permiso",
                        principalColumn: "IdPermiso",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rol_permiso_rol_IdRol",
                        column: x => x.IdRol,
                        principalTable: "rol",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medico",
                columns: table => new
                {
                    IdMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medico", x => x.IdMedico);
                    table.ForeignKey(
                        name: "FK_medico_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "paciente",
                columns: table => new
                {
                    IdPaciente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suspendido = table.Column<bool>(type: "bit", nullable: false),
                    FechaFinSuspension = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoSuspension = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paciente", x => x.IdPaciente);
                    table.ForeignKey(
                        name: "FK_paciente_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_rol",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_rol", x => new { x.IdUsuario, x.IdRol });
                    table.ForeignKey(
                        name: "FK_usuario_rol_rol_IdRol",
                        column: x => x.IdRol,
                        principalTable: "rol",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_rol_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horario_atencion",
                columns: table => new
                {
                    IdHorarioAtencion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraDesde = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraHasta = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horario_atencion", x => x.IdHorarioAtencion);
                    table.ForeignKey(
                        name: "FK_horario_atencion_medico_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "medico",
                        principalColumn: "IdMedico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medico_especialidad",
                columns: table => new
                {
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medico_especialidad", x => new { x.IdMedico, x.IdEspecialidad });
                    table.ForeignKey(
                        name: "FK_medico_especialidad_especialidad_IdEspecialidad",
                        column: x => x.IdEspecialidad,
                        principalTable: "especialidad",
                        principalColumn: "IdEspecialidad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_medico_especialidad_medico_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "medico",
                        principalColumn: "IdMedico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historial_medico",
                columns: table => new
                {
                    IdHistorialMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPaciente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_medico", x => x.IdHistorialMedico);
                    table.ForeignKey(
                        name: "FK_historial_medico_paciente_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "paciente",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "turno",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPaciente = table.Column<int>(type: "int", nullable: false),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turno", x => x.IdTurno);
                    table.ForeignKey(
                        name: "FK_turno_medico_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "medico",
                        principalColumn: "IdMedico",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_turno_paciente_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "paciente",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consulta",
                columns: table => new
                {
                    IdConsulta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTurno = table.Column<int>(type: "int", nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Indicaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaConsulta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consulta", x => x.IdConsulta);
                    table.ForeignKey(
                        name: "FK_consulta_turno_IdTurno",
                        column: x => x.IdTurno,
                        principalTable: "turno",
                        principalColumn: "IdTurno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pago",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTurno = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pagado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_pago_turno_IdTurno",
                        column: x => x.IdTurno,
                        principalTable: "turno",
                        principalColumn: "IdTurno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documento",
                columns: table => new
                {
                    IdDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConsulta = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento", x => x.IdDocumento);
                    table.ForeignKey(
                        name: "FK_documento_consulta_IdConsulta",
                        column: x => x.IdConsulta,
                        principalTable: "consulta",
                        principalColumn: "IdConsulta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "estudio",
                columns: table => new
                {
                    IdEstudio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConsulta = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estudio", x => x.IdEstudio);
                    table.ForeignKey(
                        name: "FK_estudio_consulta_IdConsulta",
                        column: x => x.IdConsulta,
                        principalTable: "consulta",
                        principalColumn: "IdConsulta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "receta",
                columns: table => new
                {
                    IdReceta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConsulta = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receta", x => x.IdReceta);
                    table.ForeignKey(
                        name: "FK_receta_consulta_IdConsulta",
                        column: x => x.IdConsulta,
                        principalTable: "consulta",
                        principalColumn: "IdConsulta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consulta_IdTurno",
                table: "consulta",
                column: "IdTurno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documento_IdConsulta",
                table: "documento",
                column: "IdConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_especialidad_Nombre",
                table: "especialidad",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estudio_IdConsulta",
                table: "estudio",
                column: "IdConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_historial_medico_IdPaciente",
                table: "historial_medico",
                column: "IdPaciente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_horario_atencion_IdMedico",
                table: "horario_atencion",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_medico_IdUsuario",
                table: "medico",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medico_Matricula",
                table: "medico",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medico_especialidad_IdEspecialidad",
                table: "medico_especialidad",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "IX_paciente_Dni",
                table: "paciente",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paciente_IdUsuario",
                table: "paciente",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_IdTurno",
                table: "pago",
                column: "IdTurno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permiso_Nombre",
                table: "permiso",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_receta_IdConsulta",
                table: "receta",
                column: "IdConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_rol_Nombre",
                table: "rol",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_IdPermiso",
                table: "rol_permiso",
                column: "IdPermiso");

            migrationBuilder.CreateIndex(
                name: "IX_turno_IdMedico",
                table: "turno",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_turno_IdPaciente",
                table: "turno",
                column: "IdPaciente");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_Email",
                table: "usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_rol_IdRol",
                table: "usuario_rol",
                column: "IdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento");

            migrationBuilder.DropTable(
                name: "estudio");

            migrationBuilder.DropTable(
                name: "historial_medico");

            migrationBuilder.DropTable(
                name: "horario_atencion");

            migrationBuilder.DropTable(
                name: "medico_especialidad");

            migrationBuilder.DropTable(
                name: "pago");

            migrationBuilder.DropTable(
                name: "receta");

            migrationBuilder.DropTable(
                name: "rol_permiso");

            migrationBuilder.DropTable(
                name: "usuario_rol");

            migrationBuilder.DropTable(
                name: "especialidad");

            migrationBuilder.DropTable(
                name: "consulta");

            migrationBuilder.DropTable(
                name: "permiso");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropTable(
                name: "turno");

            migrationBuilder.DropTable(
                name: "medico");

            migrationBuilder.DropTable(
                name: "paciente");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
