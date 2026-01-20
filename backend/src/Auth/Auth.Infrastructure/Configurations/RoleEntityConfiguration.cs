namespace Auth.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the RoleEntity.
/// Maps Role relational entity to the "Roles" table without domain logic.
/// </summary>
public sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");

        // Key
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.PermissionsJson)
            .IsRequired()
            .HasDefaultValue("[]");

        // Concurrency and timestamps
        builder.Property(r => r.Version)
            .IsConcurrencyToken();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Navigation properties
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
