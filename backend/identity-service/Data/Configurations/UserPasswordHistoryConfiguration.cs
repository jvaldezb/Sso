using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class UserPasswordHistoryConfiguration : IEntityTypeConfiguration<UserPasswordHistory>
{
    public void Configure(EntityTypeBuilder<UserPasswordHistory> builder)
    {
        builder.ToTable("UserPasswordHistories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.UserUpdate).HasColumnType("text");
        builder.Property(x => x.UserCreate).HasColumnType("text");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);        

        builder.HasIndex(x => new { x.UserId, x.PasswordHash })
            .IsUnique()
            .HasDatabaseName("uq_user_password");   
    }
}