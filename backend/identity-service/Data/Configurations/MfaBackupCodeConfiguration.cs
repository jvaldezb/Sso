using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class MfaBackupCodeConfiguration : IEntityTypeConfiguration<MfaBackupCode>
{
    public void Configure(EntityTypeBuilder<MfaBackupCode> builder)
    {
        builder.ToTable("MfaBackupCodes");

        builder.HasKey(e => e.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.UsedAt);        

        // Navigation
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.Code })
            .IsUnique()
            .HasDatabaseName("uix_mfa_backup_codes_user_code");        
    }
}
