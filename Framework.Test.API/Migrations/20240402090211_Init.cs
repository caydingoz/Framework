using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Test.API.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachableTestModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachableTestModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SqlTestModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlTestModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SqlWithManyTestRelationModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlWithManyTestRelationModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SqlWithOneTestModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlWithOneTestModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachableTestChildModel",
                columns: table => new
                {
                    CachableTestModelId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachableTestChildModel", x => new { x.CachableTestModelId, x.Id });
                    table.ForeignKey(
                        name: "FK_CachableTestChildModel_CachableTestModels_CachableTestModelId",
                        column: x => x.CachableTestModelId,
                        principalTable: "CachableTestModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationJoinTable",
                columns: table => new
                {
                    SqlWithManyTestModelsId = table.Column<int>(type: "int", nullable: false),
                    SqlWithManyTestRelationModelsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationJoinTable", x => new { x.SqlWithManyTestModelsId, x.SqlWithManyTestRelationModelsId });
                    table.ForeignKey(
                        name: "FK_RelationJoinTable_SqlTestModels_SqlWithManyTestModelsId",
                        column: x => x.SqlWithManyTestModelsId,
                        principalTable: "SqlTestModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelationJoinTable_SqlWithManyTestRelationModels_SqlWithManyTestRelationModelsId",
                        column: x => x.SqlWithManyTestRelationModelsId,
                        principalTable: "SqlWithManyTestRelationModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SqlWithOneTestRelationModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SqlWithOneTestModelId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlWithOneTestRelationModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SqlWithOneTestRelationModels_SqlWithOneTestModel_SqlWithOneTestModelId",
                        column: x => x.SqlWithOneTestModelId,
                        principalTable: "SqlWithOneTestModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelationJoinTable_SqlWithManyTestRelationModelsId",
                table: "RelationJoinTable",
                column: "SqlWithManyTestRelationModelsId");

            migrationBuilder.CreateIndex(
                name: "IX_SqlWithOneTestRelationModels_SqlWithOneTestModelId",
                table: "SqlWithOneTestRelationModels",
                column: "SqlWithOneTestModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachableTestChildModel");

            migrationBuilder.DropTable(
                name: "RelationJoinTable");

            migrationBuilder.DropTable(
                name: "SqlWithOneTestRelationModels");

            migrationBuilder.DropTable(
                name: "CachableTestModels");

            migrationBuilder.DropTable(
                name: "SqlTestModels");

            migrationBuilder.DropTable(
                name: "SqlWithManyTestRelationModels");

            migrationBuilder.DropTable(
                name: "SqlWithOneTestModel");
        }
    }
}
