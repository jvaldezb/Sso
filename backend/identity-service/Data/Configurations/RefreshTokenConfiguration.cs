using System;
using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class RefreshTokenConfiguration: IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rt => rt.UserId).IsRequired();
        builder.Property(rt => rt.Token).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.IsRevoked).IsRequired();

        builder.Property(rt => rt.SystemId)
            .HasColumnType("uuid")
            .IsRequired(false);
            
        builder.Property(rt => rt.SessionId)
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.System)
            .WithMany()
            .HasForeignKey(rt => rt.SystemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(rt => rt.Session)
            .WithMany()
            .HasForeignKey(rt => rt.SessionId)
            .OnDelete(DeleteBehavior.SetNull);   

        builder.HasIndex(rt => rt.SystemId);
        builder.HasIndex(rt => rt.SessionId);     
    }
}
