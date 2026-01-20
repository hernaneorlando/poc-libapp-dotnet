namespace Auth.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the RefreshTokenEntity.
/// Maps RefreshToken to the "RefreshTokens" table.
/// </summary>
public sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens");

        // Key
        builder.HasKey(rt => rt.Id);

        // Properties
        builder.Property(rt => rt.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt);

        builder.Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Foreign Key
        builder.HasOne<UserEntity>()
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
