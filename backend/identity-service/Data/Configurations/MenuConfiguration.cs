using identity_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace identity_service.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.MenuLabel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(150);
        builder.Property(x => x.Module).HasColumnType("text").IsRequired();
        builder.Property(x => x.ModuleType).HasColumnType("text");
        builder.Property(x => x.MenuType).HasColumnType("text");
        builder.Property(x => x.RequiredClaimType).HasColumnType("text");
        builder.Property(x => x.IconUrl).HasColumnType("text");
        builder.Property(x => x.AccessScope).HasMaxLength(25);
        builder.Property(x => x.UserUpdate).HasColumnType("text");
        builder.Property(x => x.UserCreate).HasColumnType("text");

        // Índice único sobre (SystemId, Module)
        builder.HasIndex(x => new { x.SystemId, x.Module })
            .IsUnique()
            .HasDatabaseName("IX_Menu_SystemId_Module");

        builder.HasOne(x => x.ParentMenu)
            .WithMany(x => x.ChildMenus)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.System)
            .WithMany(x => x.Menus)
            .HasForeignKey(x => x.SystemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}