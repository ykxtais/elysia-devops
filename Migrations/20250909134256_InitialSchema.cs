using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElysiaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotoCsharp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Placa = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    Marca = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Ano = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotoCsharp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioCsharp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    Senha = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioCsharp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VagaCsharp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Numero = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Patio = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VagaCsharp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UK_UsuarioCsharp_Cpf",
                table: "UsuarioCsharp",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_UsuarioCsharp_Email",
                table: "UsuarioCsharp",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Vaga_Patio_Numero",
                table: "VagaCsharp",
                columns: new[] { "Patio", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotoCsharp");

            migrationBuilder.DropTable(
                name: "UsuarioCsharp");

            migrationBuilder.DropTable(
                name: "VagaCsharp");
        }
    }
}
