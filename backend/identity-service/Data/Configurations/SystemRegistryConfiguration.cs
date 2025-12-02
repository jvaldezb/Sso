using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class SystemRegistryConfiguration : IEntityTypeConfiguration<SystemRegistry>
{
    public void Configure(EntityTypeBuilder<SystemRegistry> builder)
    {
        builder.ToTable("SystemRegistries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.SystemCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SystemName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.BaseUrl).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IconUrl).HasMaxLength(200);
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property(x => x.ContactEmail).HasMaxLength(100);
        builder.Property(x => x.UserUpdate).HasColumnType("text");
        builder.Property(x => x.UserCreate).HasColumnType("text");
        builder.Property(x => x.IsCentralAdmin).IsRequired();

        builder.HasIndex(x => x.SystemCode).IsUnique();
    }
}