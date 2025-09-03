using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElysiaAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateSchemaCsharp : Migration
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
                name: "VagaCsharp");
        }
    }
}
