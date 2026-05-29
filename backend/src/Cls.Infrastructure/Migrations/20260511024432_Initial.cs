using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    symbol = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_file_group_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_file_group_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_file_group_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_file_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_file_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_payment_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_payment_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_payment_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_type = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    second_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    profile_file_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extra_provider_payment_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    extra_provider_payment_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_provider_payment_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extra_provider_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    extra_provider_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_type = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_provider_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extra_providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    original_filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    stored_filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    file_size = table.Column<int>(type: "integer", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    uploaded_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Employee"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    file_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_sequences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    year = table.Column<int>(type: "integer", nullable: false),
                    last_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    prefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ORD"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_sequences", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_sequences_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_sequences_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_sequences_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    second_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    profile_file_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.id);
                    table.ForeignKey(
                        name: "FK_providers_files_profile_file_id",
                        column: x => x.profile_file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_providers_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_providers_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_providers_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    order_position = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stages", x => x.id);
                    table.ForeignKey(
                        name: "FK_stages_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stages_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stages_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_file_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_file_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_file_groups_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_file_groups_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_file_groups_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_file_groups_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stage_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    order_position = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_final_step = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_steps_stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "stages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_steps_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_steps_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_steps_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_file_group_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider_file_group_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_file_group_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_file_group_items_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_file_group_items_provider_file_groups_provider_fil~",
                        column: x => x.provider_file_group_id,
                        principalTable: "provider_file_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_file_group_items_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_file_group_items_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_file_group_items_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    buy_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    buy_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sell_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sell_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    client_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    provider_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    balances_last_calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<int>(type: "integer", nullable: false),
                    current_step_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    first_action_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    suspended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.CheckConstraint("CK_orders_order_date_first_action", "(\"first_action_date\" IS NULL) OR (\"order_date\" <= \"first_action_date\")");
                    table.ForeignKey(
                        name: "FK_orders_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_steps_current_step_id",
                        column: x => x.current_step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    step_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    note_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    action_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_notes_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notes_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notes_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notes_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notes_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_buy_invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_buy_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_buy_invoices_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_buy_invoices_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_buy_invoices_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_buy_invoices_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_buy_invoices_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_employees_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_employees_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_employees_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_employees_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_employees_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_sell_invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_sell_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_sell_invoices_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_sell_invoices_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_sell_invoices_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_sell_invoices_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_sell_invoices_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_status_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    order_status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_files_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_status_files_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_status_files_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_status_files_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_status_files_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_step_historys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    step_id = table.Column<int>(type: "integer", nullable: false),
                    entered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    exited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    entry_type = table.Column<int>(type: "integer", nullable: false),
                    exit_reason = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_step_historys", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_step_historys_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_step_historys_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_step_historys_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_step_historys_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_step_historys_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_unique_numbers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_unique_numbers", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_unique_numbers_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_unique_numbers_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_unique_numbers_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_unique_numbers_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_type = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_payments_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_payments_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_payments_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "note_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    note_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_note_files_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_note_files_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_note_files_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_note_files_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_note_files_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    step_id = table.Column<int>(type: "integer", nullable: true),
                    note_id = table.Column<int>(type: "integer", nullable: true),
                    log_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    from_step_id = table.Column<int>(type: "integer", nullable: true),
                    to_step_id = table.Column<int>(type: "integer", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    actor_user_id = table.Column<int>(type: "integer", nullable: false),
                    log_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_logs_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_logs_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_logs_steps_from_step_id",
                        column: x => x.from_step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_logs_steps_step_id",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_logs_steps_to_step_id",
                        column: x => x.to_step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_logs_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_payment_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider_payment_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_payment_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_payment_files_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_payment_files_provider_payments_provider_payment_id",
                        column: x => x.provider_payment_id,
                        principalTable: "provider_payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_payment_files_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_payment_files_users_deleted_by_user_id",
                        column: x => x.deleted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_payment_files_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_log_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_log_id = table.Column<int>(type: "integer", nullable: false),
                    file_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_log_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_log_files_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_log_files_order_logs_order_log_id",
                        column: x => x.order_log_id,
                        principalTable: "order_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_log_files_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_client_file_group_items_file_id",
                table: "client_file_group_items",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_file_group_items_group_id",
                table: "client_file_group_items",
                column: "client_file_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_file_group_items_created_by_user_id",
                table: "client_file_group_items",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_file_group_items_deleted_by_user_id",
                table: "client_file_group_items",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_file_group_items_updated_by_user_id",
                table: "client_file_group_items",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_file_groups_client_id",
                table: "client_file_groups",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_file_groups_unique_label",
                table: "client_file_groups",
                columns: new[] { "client_id", "label", "is_deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_file_groups_created_by_user_id",
                table: "client_file_groups",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_file_groups_deleted_by_user_id",
                table: "client_file_groups",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_file_groups_updated_by_user_id",
                table: "client_file_groups",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_payment_files_file_id",
                table: "client_payment_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_payment_files_payment_id",
                table: "client_payment_files",
                column: "client_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_payment_files_created_by_user_id",
                table: "client_payment_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_payment_files_deleted_by_user_id",
                table: "client_payment_files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_payment_files_updated_by_user_id",
                table: "client_payment_files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_payments_order_date",
                table: "client_payments",
                columns: new[] { "order_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "idx_client_payments_order_id",
                table: "client_payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_payments_payment_date",
                table: "client_payments",
                column: "payment_date");

            migrationBuilder.CreateIndex(
                name: "idx_client_payments_payment_type",
                table: "client_payments",
                column: "payment_type");

            migrationBuilder.CreateIndex(
                name: "IX_client_payments_created_by_user_id",
                table: "client_payments",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_payments_deleted_by_user_id",
                table: "client_payments",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_payments_updated_by_user_id",
                table: "client_payments",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_clients_email",
                table: "clients",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_clients_is_active",
                table: "clients",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_clients_name",
                table: "clients",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_clients_created_by_user_id",
                table: "clients",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_deleted_by_user_id",
                table: "clients",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_profile_file_id",
                table: "clients",
                column: "profile_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_clients_updated_by_user_id",
                table: "clients",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payment_files_file_id",
                table: "extra_provider_payment_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payment_files_payment_id",
                table: "extra_provider_payment_files",
                column: "extra_provider_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payment_files_created_by_user_id",
                table: "extra_provider_payment_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payment_files_deleted_by_user_id",
                table: "extra_provider_payment_files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payment_files_updated_by_user_id",
                table: "extra_provider_payment_files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payments_extra_provider_id",
                table: "extra_provider_payments",
                column: "extra_provider_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payments_order_id",
                table: "extra_provider_payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payments_payment_date",
                table: "extra_provider_payments",
                column: "payment_date");

            migrationBuilder.CreateIndex(
                name: "idx_extra_provider_payments_payment_type",
                table: "extra_provider_payments",
                column: "payment_type");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payments_created_by_user_id",
                table: "extra_provider_payments",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payments_deleted_by_user_id",
                table: "extra_provider_payments",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_provider_payments_updated_by_user_id",
                table: "extra_provider_payments",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_providers_order_id",
                table: "extra_providers",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_extra_providers_provider_id",
                table: "extra_providers",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_providers_created_by_user_id",
                table: "extra_providers",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_providers_deleted_by_user_id",
                table: "extra_providers",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_providers_updated_by_user_id",
                table: "extra_providers",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_files_stored_filename",
                table: "files",
                column: "stored_filename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_files_uploaded_by_user_id",
                table: "files",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_created_by_user_id",
                table: "files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_deleted_by_user_id",
                table: "files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_updated_by_user_id",
                table: "files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_note_files_file_id",
                table: "note_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_note_files_note_id",
                table: "note_files",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "IX_note_files_created_by_user_id",
                table: "note_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_note_files_deleted_by_user_id",
                table: "note_files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_note_files_updated_by_user_id",
                table: "note_files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_notes_note_date",
                table: "notes",
                column: "note_date");

            migrationBuilder.CreateIndex(
                name: "idx_notes_order_id",
                table: "notes",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_notes_step_id",
                table: "notes",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_created_by_user_id",
                table: "notes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_deleted_by_user_id",
                table: "notes",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_updated_by_user_id",
                table: "notes",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_buy_invoices_file_id",
                table: "order_buy_invoices",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_buy_invoices_order_id",
                table: "order_buy_invoices",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_buy_invoices_created_by_user_id",
                table: "order_buy_invoices",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_buy_invoices_deleted_by_user_id",
                table: "order_buy_invoices",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_buy_invoices_updated_by_user_id",
                table: "order_buy_invoices",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_employees_order_id",
                table: "order_employees",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_employees_user_id",
                table: "order_employees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_employees_created_by_user_id",
                table: "order_employees",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_employees_deleted_by_user_id",
                table: "order_employees",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_employees_updated_by_user_id",
                table: "order_employees",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_log_files_file_id",
                table: "order_log_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_log_files_order_log_id",
                table: "order_log_files",
                column: "order_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_log_files_created_by_user_id",
                table: "order_log_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_actor_user_id",
                table: "order_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_log_date",
                table: "order_logs",
                column: "log_date");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_log_type",
                table: "order_logs",
                column: "log_type");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_note_id",
                table: "order_logs",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_order_id",
                table: "order_logs",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_order_log_date",
                table: "order_logs",
                columns: new[] { "order_id", "log_date" });

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_step_id",
                table: "order_logs",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_logs_type_date",
                table: "order_logs",
                columns: new[] { "log_type", "log_date" });

            migrationBuilder.CreateIndex(
                name: "IX_order_logs_from_step_id",
                table: "order_logs",
                column: "from_step_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_logs_to_step_id",
                table: "order_logs",
                column: "to_step_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_sell_invoices_file_id",
                table: "order_sell_invoices",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_sell_invoices_order_id",
                table: "order_sell_invoices",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_sell_invoices_created_by_user_id",
                table: "order_sell_invoices",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_sell_invoices_deleted_by_user_id",
                table: "order_sell_invoices",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_sell_invoices_updated_by_user_id",
                table: "order_sell_invoices",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_sequences_year",
                table: "order_sequences",
                column: "year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_sequences_created_by_user_id",
                table: "order_sequences",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_sequences_deleted_by_user_id",
                table: "order_sequences",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_sequences_updated_by_user_id",
                table: "order_sequences",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_status_files_file_id",
                table: "order_status_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_status_files_order_id",
                table: "order_status_files",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_files_created_by_user_id",
                table: "order_status_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_files_deleted_by_user_id",
                table: "order_status_files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_files_updated_by_user_id",
                table: "order_status_files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_step_history_order_entered",
                table: "order_step_historys",
                columns: new[] { "order_id", "entered_at" });

            migrationBuilder.CreateIndex(
                name: "idx_order_step_history_order_id",
                table: "order_step_historys",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_step_history_step_id",
                table: "order_step_historys",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_step_historys_created_by_user_id",
                table: "order_step_historys",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_step_historys_deleted_by_user_id",
                table: "order_step_historys",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_step_historys_updated_by_user_id",
                table: "order_step_historys",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_unique_numbers_label",
                table: "order_unique_numbers",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "idx_order_unique_numbers_order_id",
                table: "order_unique_numbers",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_unique_numbers_unique_label",
                table: "order_unique_numbers",
                columns: new[] { "order_id", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_order_unique_numbers_value",
                table: "order_unique_numbers",
                column: "value");

            migrationBuilder.CreateIndex(
                name: "IX_order_unique_numbers_created_by_user_id",
                table: "order_unique_numbers",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_unique_numbers_deleted_by_user_id",
                table: "order_unique_numbers",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_unique_numbers_updated_by_user_id",
                table: "order_unique_numbers",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_client_id",
                table: "orders",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_current_step_id",
                table: "orders",
                column: "current_step_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_date_amount",
                table: "orders",
                columns: new[] { "order_date", "sell_amount" });

            migrationBuilder.CreateIndex(
                name: "idx_orders_order_date",
                table: "orders",
                column: "order_date");

            migrationBuilder.CreateIndex(
                name: "idx_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_orders_provider_id",
                table: "orders",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_orders_status_date",
                table: "orders",
                columns: new[] { "status", "order_date" });

            migrationBuilder.CreateIndex(
                name: "idx_orders_user_date",
                table: "orders",
                columns: new[] { "created_by_user_id", "order_date" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_deleted_by_user_id",
                table: "orders",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_updated_by_user_id",
                table: "orders",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_processed",
                table: "outbox_messages",
                column: "processed_on_utc");

            migrationBuilder.CreateIndex(
                name: "idx_provider_file_group_items_file_id",
                table: "provider_file_group_items",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_file_group_items_group_id",
                table: "provider_file_group_items",
                column: "provider_file_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_group_items_created_by_user_id",
                table: "provider_file_group_items",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_group_items_deleted_by_user_id",
                table: "provider_file_group_items",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_group_items_updated_by_user_id",
                table: "provider_file_group_items",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_file_groups_provider_id",
                table: "provider_file_groups",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_file_groups_unique_label",
                table: "provider_file_groups",
                columns: new[] { "provider_id", "label", "is_deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_groups_created_by_user_id",
                table: "provider_file_groups",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_groups_deleted_by_user_id",
                table: "provider_file_groups",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_file_groups_updated_by_user_id",
                table: "provider_file_groups",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_payment_files_file_id",
                table: "provider_payment_files",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_payment_files_payment_id",
                table: "provider_payment_files",
                column: "provider_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payment_files_created_by_user_id",
                table: "provider_payment_files",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payment_files_deleted_by_user_id",
                table: "provider_payment_files",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payment_files_updated_by_user_id",
                table: "provider_payment_files",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_payments_order_date",
                table: "provider_payments",
                columns: new[] { "order_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "idx_provider_payments_order_id",
                table: "provider_payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_provider_payments_payment_date",
                table: "provider_payments",
                column: "payment_date");

            migrationBuilder.CreateIndex(
                name: "idx_provider_payments_payment_type",
                table: "provider_payments",
                column: "payment_type");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payments_created_by_user_id",
                table: "provider_payments",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payments_deleted_by_user_id",
                table: "provider_payments",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_payments_updated_by_user_id",
                table: "provider_payments",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_providers_email",
                table: "providers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_providers_is_active",
                table: "providers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_providers_name",
                table: "providers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_providers_created_by_user_id",
                table: "providers",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_providers_deleted_by_user_id",
                table: "providers",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_providers_profile_file_id",
                table: "providers",
                column: "profile_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_providers_updated_by_user_id",
                table: "providers",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_stages_is_active",
                table: "stages",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_stages_order_position",
                table: "stages",
                column: "order_position",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stages_created_by_user_id",
                table: "stages",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stages_deleted_by_user_id",
                table: "stages",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stages_updated_by_user_id",
                table: "stages",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_steps_is_final",
                table: "steps",
                column: "is_final_step");

            migrationBuilder.CreateIndex(
                name: "idx_steps_order_position",
                table: "steps",
                column: "order_position");

            migrationBuilder.CreateIndex(
                name: "idx_steps_stage_id",
                table: "steps",
                column: "stage_id");

            migrationBuilder.CreateIndex(
                name: "idx_steps_stage_order",
                table: "steps",
                columns: new[] { "stage_id", "order_position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_steps_created_by_user_id",
                table: "steps",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_steps_deleted_by_user_id",
                table: "steps",
                column: "deleted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_steps_updated_by_user_id",
                table: "steps",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_active_role",
                table: "users",
                columns: new[] { "is_active", "role" });

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_users_role",
                table: "users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_users_file_id",
                table: "users",
                column: "file_id");

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_group_items_client_file_groups_client_file_grou~",
                table: "client_file_group_items",
                column: "client_file_group_id",
                principalTable: "client_file_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_group_items_files_file_id",
                table: "client_file_group_items",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_group_items_users_created_by_user_id",
                table: "client_file_group_items",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_group_items_users_deleted_by_user_id",
                table: "client_file_group_items",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_group_items_users_updated_by_user_id",
                table: "client_file_group_items",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_groups_clients_client_id",
                table: "client_file_groups",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_groups_users_created_by_user_id",
                table: "client_file_groups",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_groups_users_deleted_by_user_id",
                table: "client_file_groups",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_file_groups_users_updated_by_user_id",
                table: "client_file_groups",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payment_files_client_payments_client_payment_id",
                table: "client_payment_files",
                column: "client_payment_id",
                principalTable: "client_payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payment_files_files_file_id",
                table: "client_payment_files",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payment_files_users_created_by_user_id",
                table: "client_payment_files",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payment_files_users_deleted_by_user_id",
                table: "client_payment_files",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payment_files_users_updated_by_user_id",
                table: "client_payment_files",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payments_orders_order_id",
                table: "client_payments",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payments_users_created_by_user_id",
                table: "client_payments",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payments_users_deleted_by_user_id",
                table: "client_payments",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_client_payments_users_updated_by_user_id",
                table: "client_payments",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_clients_files_profile_file_id",
                table: "clients",
                column: "profile_file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_clients_users_created_by_user_id",
                table: "clients",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_clients_users_deleted_by_user_id",
                table: "clients",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_clients_users_updated_by_user_id",
                table: "clients",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payment_files_extra_provider_payments_extra_~",
                table: "extra_provider_payment_files",
                column: "extra_provider_payment_id",
                principalTable: "extra_provider_payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payment_files_files_file_id",
                table: "extra_provider_payment_files",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payment_files_users_created_by_user_id",
                table: "extra_provider_payment_files",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payment_files_users_deleted_by_user_id",
                table: "extra_provider_payment_files",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payment_files_users_updated_by_user_id",
                table: "extra_provider_payment_files",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payments_extra_providers_extra_provider_id",
                table: "extra_provider_payments",
                column: "extra_provider_id",
                principalTable: "extra_providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payments_orders_order_id",
                table: "extra_provider_payments",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payments_users_created_by_user_id",
                table: "extra_provider_payments",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payments_users_deleted_by_user_id",
                table: "extra_provider_payments",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_provider_payments_users_updated_by_user_id",
                table: "extra_provider_payments",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_providers_orders_order_id",
                table: "extra_providers",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_providers_providers_provider_id",
                table: "extra_providers",
                column: "provider_id",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_providers_users_created_by_user_id",
                table: "extra_providers",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_providers_users_deleted_by_user_id",
                table: "extra_providers",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_extra_providers_users_updated_by_user_id",
                table: "extra_providers",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_created_by_user_id",
                table: "files",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_deleted_by_user_id",
                table: "files",
                column: "deleted_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_files_users_updated_by_user_id",
                table: "files",
                column: "updated_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ─── Seed Data ───────────────────────────────────────────
            var pwd = BCrypt.Net.BCrypt.HashPassword("Admin@2026!");
            migrationBuilder.Sql($@"
        INSERT INTO users
        (id, name, email, password_hash, phone, address, description, role,
         is_active, last_login_at, file_id,
         created_at, created_by_user_id,
         updated_at, updated_by_user_id,
         is_deleted, deleted_at, deleted_by_user_id)
        VALUES
        (1, 'Admin User', 'admin@cls.local', '{pwd}', '+1-555-0001', NULL, 'System administrator with full access', 'Admin', TRUE, NULL, NULL, NOW(), 1, NOW(), 1, FALSE, NULL, NULL)
        ON CONFLICT (id) DO NOTHING;

        INSERT INTO currencies (id, code, name, symbol, created_at, created_by_user_id, updated_at, updated_by_user_id)
        VALUES
        (1, 'USD', 'US Dollar', '$', NOW(), 1, NOW(), 1),
        (2, 'EUR', 'Euro', '€', NOW(), 1, NOW(), 1),
        (3, 'GBP', 'UK Pound Sterling', '£', NOW(), 1, NOW(), 1),
        (4, 'JPY', 'Japanese Yen', '¥', NOW(), 1, NOW(), 1),
        (5, 'AED', 'United Arab Emirates Dirham', 'د.إ', NOW(), 1, NOW(), 1),
        (6, 'AUD', 'Australian Dollar', '$', NOW(), 1, NOW(), 1),
        (7, 'CAD', 'Canadian Dollar', '$', NOW(), 1, NOW(), 1),
        (8, 'CHF', 'Swiss Franc', 'CHF', NOW(), 1, NOW(), 1),
        (9, 'CNY', 'Chinese Yuan', '¥', NOW(), 1, NOW(), 1)
        ON CONFLICT (id) DO NOTHING;

        INSERT INTO stages
        (id, name, order_position, description, is_active,
         created_at, created_by_user_id, updated_at, updated_by_user_id,
         is_deleted, deleted_at, deleted_by_user_id)
        VALUES
        (1, 'Order Placement', 1, 'Stage 1 Desc', TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (2, 'Pre-Production', 2, 'Stage 2 Desc', TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (3, 'Manufacturing', 3, 'Stage 3 Desc', TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (4, 'Post-Production', 4, 'Stage 4 Desc', TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (5, 'Shipping & Delivery', 5, 'Stage 5 Desc', TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL)
        ON CONFLICT (id) DO NOTHING;

        INSERT INTO steps
        (id, stage_id, name, order_position, description,
         is_final_step, is_active,
         created_at, created_by_user_id, updated_at, updated_by_user_id,
         is_deleted, deleted_at, deleted_by_user_id)
        VALUES
        (1, 1, 'Confirming Product Specifications', 1, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (2, 1, 'Purchase Order Sending', 2, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (3, 1, 'Price, Quantity, and Lead time Negotiation', 3, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (4, 1, 'Getting Order Confirmation from Factory', 4, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (5, 2, 'Pre-Production Sample Approval', 5, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (6, 2, 'Raw Materials Availability and Colour Swatches Confirmation', 6, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (7, 2, 'Receiving Production Timeline from Factory', 7, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (8, 2, 'Initial Deposit Payment', 8, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (9, 3, 'Raw Material Procurement', 9, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (10, 3, 'Initial Production Stage', 10, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (11, 3, 'Mid-Production', 11, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (12, 3, 'Mid-Production Inspection', 12, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (13, 3, 'Final Production', 13, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (14, 3, 'Final Production Quality Control', 14, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (15, 3, '3rd Party Inspection', 15, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (16, 3, 'Final Product Approval', 16, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (17, 4, 'Receive Production Completion Notice', 17, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (18, 4, 'Arrange Shipment Logistics', 18, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (19, 4, 'Get Packing List & Invoice', 19, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (20, 4, 'Book Freight Forwarder / Shipping Agent', 20, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (21, 5, 'Goods Loaded & Shipped', 21, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (22, 5, 'Receive Tracking / Bill of Lading', 22, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (23, 5, 'Customs Clearance', 23, 'Nothing', FALSE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL),
        (24, 5, 'Final Delivery to Your Customer', 24, 'Nothing', TRUE, TRUE, NOW(), 1, NOW(), 1, FALSE, NULL, NULL)
        ON CONFLICT (id) DO NOTHING;

        SELECT setval(pg_get_serial_sequence('users', 'id'), GREATEST((SELECT MAX(id) FROM users), 1), true);
        SELECT setval(pg_get_serial_sequence('currencies','id'), (SELECT MAX(id) FROM currencies), true);
        SELECT setval(pg_get_serial_sequence('stages','id'), (SELECT MAX(id) FROM stages), true);
        SELECT setval(pg_get_serial_sequence('steps','id'), (SELECT MAX(id) FROM steps), true);
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_files_file_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "client_file_group_items");

            migrationBuilder.DropTable(
                name: "client_payment_files");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "extra_provider_payment_files");

            migrationBuilder.DropTable(
                name: "note_files");

            migrationBuilder.DropTable(
                name: "order_buy_invoices");

            migrationBuilder.DropTable(
                name: "order_employees");

            migrationBuilder.DropTable(
                name: "order_log_files");

            migrationBuilder.DropTable(
                name: "order_sell_invoices");

            migrationBuilder.DropTable(
                name: "order_sequences");

            migrationBuilder.DropTable(
                name: "order_status_files");

            migrationBuilder.DropTable(
                name: "order_step_historys");

            migrationBuilder.DropTable(
                name: "order_unique_numbers");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "provider_file_group_items");

            migrationBuilder.DropTable(
                name: "provider_payment_files");

            migrationBuilder.DropTable(
                name: "client_file_groups");

            migrationBuilder.DropTable(
                name: "client_payments");

            migrationBuilder.DropTable(
                name: "extra_provider_payments");

            migrationBuilder.DropTable(
                name: "order_logs");

            migrationBuilder.DropTable(
                name: "provider_file_groups");

            migrationBuilder.DropTable(
                name: "provider_payments");

            migrationBuilder.DropTable(
                name: "extra_providers");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "providers");

            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.DropTable(
                name: "stages");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
