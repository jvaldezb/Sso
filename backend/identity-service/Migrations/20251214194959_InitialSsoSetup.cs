using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace identity_service.Migrations
{
    /// <inheritdoc />
    public partial class InitialSsoSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asp_net_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    management_code = table.Column<string>(type: "text", nullable: true),
                    secret_question = table.Column<string>(type: "text", nullable: true),
                    secret_answer = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    document_type = table.Column<string>(type: "text", nullable: true),
                    document_number = table.Column<string>(type: "text", nullable: true),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    last_login_provider = table.Column<string>(type: "text", nullable: true),
                    last_login_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_tokens",
                columns: table => new
                {
                    jti = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_tokens", x => x.jti);
                });

            migrationBuilder.CreateTable(
                name: "provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    client_secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    endpoint_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    scopes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_registries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    system_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    system_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    base_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    icon_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_central_admin = table.Column<bool>(type: "boolean", nullable: false),
                    last_sync = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    api_key = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_registries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    event_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    details = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auth_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_auth_audit_logs_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_verification_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_verification_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mfa_backup_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mfa_backup_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_mfa_backup_codes_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_authentication_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    provider_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    external_user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_sync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    application_user_id = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_authentication_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_authentication_providers_asp_net_users_application_use~",
                        column: x => x.application_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_authentication_providers_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_password_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_histories_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    jwt_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    token_type = table.Column<string>(type: "text", nullable: false),
                    system_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    scope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_sessions_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    module = table.Column<string>(type: "text", nullable: true),
                    module_type = table.Column<string>(type: "text", nullable: true),
                    menu_type = table.Column<string>(type: "text", nullable: true),
                    required_claim_type = table.Column<string>(type: "text", nullable: true),
                    required_claim_min_value = table.Column<int>(type: "integer", nullable: false),
                    icon_url = table.Column<string>(type: "text", nullable: true),
                    access_scope = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    order_index = table.Column<short>(type: "smallint", nullable: false),
                    bit_position = table.Column<int>(type: "integer", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menus", x => x.id);
                    table.ForeignKey(
                        name: "fk_menus_menus_parent_id",
                        column: x => x.parent_id,
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_menus_system_registries_system_id",
                        column: x => x.system_id,
                        principalTable: "system_registries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token = table.Column<string>(type: "text", nullable: true),
                    device_info = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_create = table.Column<string>(type: "text", nullable: true),
                    date_create = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_update = table.Column<string>(type: "text", nullable: true),
                    date_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_system_registries_system_id",
                        column: x => x.system_id,
                        principalTable: "system_registries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_user_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "user_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "asp_net_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "asp_net_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "asp_net_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "asp_net_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "asp_net_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "asp_net_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auth_audit_logs_user_id",
                table: "auth_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_user_id",
                table: "email_verification_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_exchange_tokens_expires_at",
                table: "exchange_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_exchange_tokens_unused",
                table: "exchange_tokens",
                column: "jti",
                filter: "used_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_menus_parent_id",
                table: "menus",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_menus_system_id",
                table: "menus",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "uix_mfa_backup_codes_user_code",
                table: "mfa_backup_codes",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_configurations_provider_name",
                table: "provider_configurations",
                column: "provider_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_session_id",
                table: "refresh_tokens",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_system_id",
                table: "refresh_tokens",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_system_registries_system_code",
                table: "system_registries",
                column: "system_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_authentication_providers_application_user_id",
                table: "user_authentication_providers",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_authentication_providers_user_id_provider_name",
                table: "user_authentication_providers",
                columns: new[] { "user_id", "provider_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_user_password",
                table: "user_password_histories",
                columns: new[] { "user_id", "password_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id_jwt_id",
                table: "user_sessions",
                columns: new[] { "user_id", "jwt_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asp_net_role_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_logins");

            migrationBuilder.DropTable(
                name: "asp_net_user_roles");

            migrationBuilder.DropTable(
                name: "asp_net_user_tokens");

            migrationBuilder.DropTable(
                name: "auth_audit_logs");

            migrationBuilder.DropTable(
                name: "email_verification_tokens");

            migrationBuilder.DropTable(
                name: "exchange_tokens");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "mfa_backup_codes");

            migrationBuilder.DropTable(
                name: "provider_configurations");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_authentication_providers");

            migrationBuilder.DropTable(
                name: "user_password_histories");

            migrationBuilder.DropTable(
                name: "asp_net_roles");

            migrationBuilder.DropTable(
                name: "system_registries");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "asp_net_users");
        }
    }
}
