using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class ProviderConfigurationConfiguration : IEntityTypeConfiguration<ProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<ProviderConfiguration> builder)
    {
        builder.ToTable("ProviderConfigurations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ProviderName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProviderType).HasMaxLength(30);
        builder.Property(x => x.ClientId).HasMaxLength(200);
        builder.Property(x => x.ClientSecret).HasMaxLength(200);
        builder.Property(x => x.EndpointUrl).HasMaxLength(200);
        builder.Property(x => x.Scopes).HasMaxLength(200);
        builder.Property(x => x.UserUpdate).HasColumnType("text");
        builder.Property(x => x.UserCreate).HasColumnType("text");

        builder.HasIndex(x => x.ProviderName).IsUnique();
    }
}