using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens");
        builder.HasKey(e => e.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Token).IsRequired();
        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.UsedAt);        

        // Navigation
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);        
    }
}
