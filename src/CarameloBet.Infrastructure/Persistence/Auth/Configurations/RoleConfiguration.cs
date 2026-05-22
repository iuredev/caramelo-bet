using CarameloBet.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.Auth.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(255);

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("idx_roles_name");
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("idx_user_roles_user_id");
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasIndex(rp => rp.RoleId)
            .HasDatabaseName("idx_role_permissions_role_id");
    }
}
