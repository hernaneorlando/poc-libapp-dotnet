namespace Auth.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the UserEntity.
/// Maps User relational entity to the "Users" table without domain logic.
/// </summary>
public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        // Key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255);

        // Address columns
        builder.Property(u => u.AddressStreet)
            .HasMaxLength(256);

        builder.Property(u => u.AddressCity)
            .HasMaxLength(100);

        builder.Property(u => u.AddressState)
            .HasMaxLength(50);

        builder.Property(u => u.AddressCountry)
            .HasMaxLength(100);

        builder.Property(u => u.AddressZipCode)
            .HasMaxLength(20);

        builder.Property(u => u.UserType)
            .IsRequired();

        // Concurrency and timestamps
        builder.Property(u => u.Version)
            .IsConcurrencyToken();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Navigation properties
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
