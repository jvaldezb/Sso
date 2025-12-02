using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class UserAuthenticationProviderConfiguration : IEntityTypeConfiguration<UserAuthenticationProvider>
{
    public void Configure(EntityTypeBuilder<UserAuthenticationProvider> builder)
    {
        builder.ToTable("UserAuthenticationProviders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ProviderType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ProviderName).HasMaxLength(50);
        builder.Property(x => x.ExternalUserId).HasMaxLength(200);
        builder.Property(x => x.UserUpdate).HasColumnType("text");
        builder.Property(x => x.UserCreate).HasColumnType("text");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.UserId, x.ProviderName }).IsUnique();
    }
}