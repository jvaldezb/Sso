using System;
using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class ExchangeTokenConfiguration : IEntityTypeConfiguration<ExchangeToken>
{
    public void Configure(EntityTypeBuilder<ExchangeToken> builder)
    {
        builder.ToTable("exchange_tokens");

            builder.HasKey(x => x.Jti);
            builder.Property(x => x.Jti)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.SystemId).IsRequired();

            builder.Property(x => x.UserId)                   
                   .IsRequired();

            builder.Property(x => x.SessionId)
                   .IsRequired();

            builder.Property(x => x.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();

            builder.Property(x => x.UsedAt)
            .HasColumnType("timestamptz");                     
                   
            builder.Property(x => x.CreatedAt)                   
           .HasColumnType("timestamptz")
           .HasDefaultValueSql("now()")
           .IsRequired();

            builder.Property(x => x.IpAddress);

            builder.Property(x => x.UserAgent);                   

            // Índices
            builder.HasIndex(x => x.ExpiresAt)
                   .HasDatabaseName("idx_exchange_tokens_expires_at");

            builder.HasIndex(x => x.Jti)
                   .HasDatabaseName("idx_exchange_tokens_jti_unused")
                   .HasFilter("used_at IS NULL");
    }
}
