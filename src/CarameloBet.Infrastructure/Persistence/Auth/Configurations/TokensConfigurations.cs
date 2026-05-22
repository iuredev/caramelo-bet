using CarameloBet.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarameloBet.Infrastructure.Persistence.Auth.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .HasDatabaseName("idx_refresh_tokens_token_hash");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("idx_refresh_tokens_user_id");
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .HasDatabaseName("idx_password_reset_tokens_token_hash");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("idx_password_reset_tokens_user_id");
    }
}
