namespace Auth.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the UserRoleEntity (junction table).
/// Maps the many-to-many relationship between User and Role.
/// </summary>
public sealed class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.ToTable("UserRoles");

        // Key
        builder.HasKey(ur => ur.Id);

        // Properties
        builder.Property(ur => ur.AssignedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Composite index for uniqueness
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_UserId_RoleId");

        // Foreign Keys
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
