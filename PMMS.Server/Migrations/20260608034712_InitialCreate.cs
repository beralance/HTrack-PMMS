using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMMS.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "aspnetroles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "projecttypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Meaning = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projecttypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    IsTemporary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedById = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aspnetroleclaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroleclaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aspnetroleclaims_aspnetroles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "aspnetroles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MunicipalityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_municipalities_provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserclaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserclaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aspnetuserclaims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserlogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserlogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_aspnetuserlogins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetuserroles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserroles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_aspnetroles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "aspnetroles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aspnetusertokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetusertokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_aspnetusertokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignments_provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assignments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateIssued = table.Column<DateOnly>(type: "date", nullable: false),
                    ProjectName = table.Column<string>(type: "citext", maxLength: 256, nullable: false),
                    Developer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CrNo = table.Column<string>(type: "text", nullable: false),
                    LsNo = table.Column<string>(type: "text", nullable: false),
                    Barangay = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Salable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsFm = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasCoc = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    HasDod = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DodDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DateOfCompletion = table.Column<DateOnly>(type: "date", nullable: false),
                    ExtensionOfTime = table.Column<DateOnly>(type: "date", nullable: true),
                    PerformanceBond = table.Column<DateOnly>(type: "date", nullable: true),
                    SemestralReport = table.Column<DateOnly>(type: "date", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AddedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true),
                    MunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false),
                    PublishedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drafts_municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drafts_projecttypes_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "projecttypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drafts_users_AddedById",
                        column: x => x.AddedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drafts_users_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drafts_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateIssued = table.Column<DateOnly>(type: "date", nullable: false),
                    ProjectName = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Developer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CrNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LsNo = table.Column<string>(type: "text", nullable: false),
                    Barangay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Salable = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsFm = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasCoc = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    HasDod = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DodDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "None"),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "Normal"),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsExtended = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsModified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SetupStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "None"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AddedById = table.Column<string>(type: "text", nullable: false),
                    DeletedById = table.Column<string>(type: "text", nullable: true),
                    MunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects_municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_projecttypes_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "projecttypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_users_AddedById",
                        column: x => x.AddedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expirations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "None"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "None"),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    LastExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CompletedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    HandledAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    HandledById = table.Column<string>(type: "text", nullable: true),
                    CancelledById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expirations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expirations_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expirations_users_CancelledById",
                        column: x => x.CancelledById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expirations_users_HandledById",
                        column: x => x.HandledById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "None"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    DeletedById = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "projecttypes",
                columns: new[] { "Id", "Meaning", "Type" },
                values: new object[,]
                {
                    { 1, "Open Market Subdivision", "OMS" },
                    { 2, "Open Market Condominium", "OMC" },
                    { 3, "Medium Cost Housing Subdivision", "MCHS" },
                    { 4, "Medium Cost Housing Condominium", "MCHC" },
                    { 5, "Economic Housing Subdivision", "EHS" },
                    { 6, "Economic Housing Condominium", "EHC" },
                    { 7, "Socialized Housing Subdivision", "SHS" },
                    { 8, "Socialized Housing Condominium", "SHC" },
                    { 9, "Commercial Subdivision", "CS" },
                    { 10, "Commercial Condominium", "CC" },
                    { 11, "Industrial Subdivision", "IS" },
                    { 12, "Farm Lot Subdivision", "FLS" },
                    { 13, "Memorial Park", "MP" },
                    { 14, "Columbarium", "Col" }
                });

            migrationBuilder.InsertData(
                table: "provinces",
                columns: new[] { "Id", "ProvinceName" },
                values: new object[,]
                {
                    { 1, "Albay" },
                    { 2, "Camarines Norte" },
                    { 3, "Camarines Sur" },
                    { 4, "Catanduanes" },
                    { 5, "Masbate" },
                    { 6, "Sorsogon" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_aspnetroleclaims_RoleId",
                table: "aspnetroleclaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "aspnetroles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserclaims_UserId",
                table: "aspnetuserclaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserlogins_UserId",
                table: "aspnetuserlogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserroles_RoleId",
                table: "aspnetuserroles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_assignments_ProvinceId",
                table: "assignments",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_assignments_UserId",
                table: "assignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_AddedById",
                table: "drafts",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_Developer",
                table: "drafts",
                column: "Developer");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_MunicipalityId",
                table: "drafts",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_ProjectName",
                table: "drafts",
                column: "ProjectName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drafts_ProjectTypeId",
                table: "drafts",
                column: "ProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_PublishedById",
                table: "drafts",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_UpdatedById",
                table: "drafts",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_expirations_CancelledById",
                table: "expirations",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_expirations_HandledById",
                table: "expirations",
                column: "HandledById");

            migrationBuilder.CreateIndex(
                name: "IX_expirations_ProjectId",
                table: "expirations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_expirations_Status",
                table: "expirations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_expirations_Type",
                table: "expirations",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_municipalities_MunicipalityName",
                table: "municipalities",
                column: "MunicipalityName");

            migrationBuilder.CreateIndex(
                name: "IX_municipalities_ProvinceId",
                table: "municipalities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_DeletedById",
                table: "notifications",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_IsDeleted",
                table: "notifications",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ProjectId",
                table: "notifications",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ProvinceId",
                table: "notifications",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_SenderId",
                table: "notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_Title",
                table: "notifications",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_AddedById",
                table: "projects",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_projects_DeletedById",
                table: "projects",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_projects_IsDeleted",
                table: "projects",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ModifiedById",
                table: "projects",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_projects_MunicipalityId",
                table: "projects",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ProjectName",
                table: "projects",
                column: "ProjectName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_ProjectTypeId",
                table: "projects",
                column: "ProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_projecttypes_Type",
                table: "projecttypes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_DeletedAt",
                table: "users",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_users_IsDeleted",
                table: "users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_users_IsTemporary",
                table: "users",
                column: "IsTemporary");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aspnetroleclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserlogins");

            migrationBuilder.DropTable(
                name: "aspnetuserroles");

            migrationBuilder.DropTable(
                name: "aspnetusertokens");

            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "drafts");

            migrationBuilder.DropTable(
                name: "expirations");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "aspnetroles");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "municipalities");

            migrationBuilder.DropTable(
                name: "projecttypes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "provinces");
        }
    }
}
