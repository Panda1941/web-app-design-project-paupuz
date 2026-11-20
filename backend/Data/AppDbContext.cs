using LolBuilds.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LolBuilds.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Champion> Champions => Set<Champion>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // SQLite doesn't remember that a date is UTC, so mark every DateTime read from the DB as UTC.
    // That way the JSON contains "...Z" and clients convert it to local time correctly.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc)) { }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Deleting a champion deletes its builds, and deleting a build deletes its comments.
        modelBuilder.Entity<Build>()
            .HasOne(b => b.Champion)
            .WithMany(c => c.Builds)
            .HasForeignKey(b => b.ChampionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Build)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BuildId)
            .OnDelete(DeleteBehavior.Cascade);

        // Users can't be deleted while they still own content.
        modelBuilder.Entity<Build>()
            .HasOne(b => b.Author)
            .WithMany()
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();

        modelBuilder.Entity<Champion>().HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
