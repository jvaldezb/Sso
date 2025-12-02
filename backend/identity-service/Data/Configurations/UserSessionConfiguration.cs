using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.JwtId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SystemName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Device).HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.Audience).HasMaxLength(100);
        builder.Property(x => x.Scope).HasMaxLength(100);
        builder.Property(x => x.IssuedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsRevoked).HasDefaultValue(false);
        builder.Property(x => x.RevokedAt);


        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // unique for UserId and JwtId
        builder.HasIndex(x => new { x.UserId, x.JwtId }).IsUnique();    
    }
}

