using identity_service.Data.Configurations;
using identity_service.Extensions;
using identity_service.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;  
using System;

namespace identity_service.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DbSet<UserAuthenticationProvider> UserAuthenticationProviders => Set<UserAuthenticationProvider>();
    public DbSet<AuthAuditLog> AuthAuditLogs => Set<AuthAuditLog>();
    public DbSet<ProviderConfiguration> ProviderConfigurations => Set<ProviderConfiguration>();
    public DbSet<SystemRegistry> SystemRegistries => Set<SystemRegistry>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<MfaBackupCode> MfaBackupCodes => Set<MfaBackupCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ExchangeToken> ExchangeTokens => Set<ExchangeToken>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        builder.Entity<ApplicationRole>().ToTable("AspNetRoles");

        builder.ApplyConfiguration(new UserAuthenticationProviderConfiguration());
        builder.ApplyConfiguration(new AuthAuditLogConfiguration());
        builder.ApplyConfiguration(new ProviderConfigurationConfiguration());
        builder.ApplyConfiguration(new SystemRegistryConfiguration());
        builder.ApplyConfiguration(new MenuConfiguration());
        builder.ApplyConfiguration(new RoleMenuConfiguration());
        builder.ApplyConfiguration(new UserPasswordHistoryConfiguration());
        builder.ApplyConfiguration(new EmailVerificationTokenConfiguration());
        builder.ApplyConfiguration(new MfaBackupCodeConfiguration());
        builder.ApplyConfiguration(new UserSessionConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
        builder.ApplyConfiguration(new ExchangeTokenConfiguration());

        // Aplicar convención snake_case a tablas y columnas
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToSnakeCase());
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName()?.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }
        }
    }
}
