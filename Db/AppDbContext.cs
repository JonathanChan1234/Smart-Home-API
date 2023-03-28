using smart_home_server.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using smart_home_server.Home.Models;
using smart_home_server.Devices.Models;

namespace smart_home_server.Db;

public class AppDbContext : IdentityUserContext<ApplicationUser>
{
    public DbSet<SmartHome> Homes { get; set; } = null!;
    public DbSet<Floor> Floors { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<JwtBlackList> JwtBlackList { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<Light> Lights { get; set; } = null!;
    public DbSet<Shade> Shades { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Many-to-Many (User to Home)
        builder.Entity<ApplicationUser>()
            .HasMany(user => user.UserHome)
            .WithMany(home => home.Users)
            .UsingEntity(join => join.ToTable("HomeUsers"));
        builder.Entity<SmartHome>()
            .HasMany(home => home.Users)
            .WithMany(user => user.UserHome)
            .UsingEntity(join => join.ToTable("HomeUsers"));

        // Many-to-Many (Installer to Home)
        builder.Entity<ApplicationUser>()
            .HasMany(user => user.InstallerHome)
            .WithMany(home => home.Installers)
            .UsingEntity(join => join.ToTable("HomeInstallers"));
        builder.Entity<SmartHome>()
            .HasMany(home => home.Installers)
            .WithMany(user => user.InstallerHome)
            .UsingEntity(join => join.ToTable("HomeInstallers"));


        // Many-to-One (User (owner) to home)
        builder.Entity<SmartHome>().HasIndex(home => new { home.Name, home.OwnerId }).IsUnique();
        builder.Entity<ApplicationUser>()
            .HasMany(user => user.OwnerHome)
            .WithOne(home => home.Owner)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SmartHome>()
            .HasOne(home => home.Owner)
            .WithMany(user => user.OwnerHome);

        // One-to-Many (Home to Floor)
        builder.Entity<Floor>().HasIndex(floor => new { floor.Name, floor.HomeId }).IsUnique();
        builder.Entity<SmartHome>()
            .HasMany(home => home.Floors)
            .WithOne(floor => floor.Home)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Floor>()
            .HasOne(floor => floor.Home)
            .WithMany(home => home.Floors)
            .IsRequired();

        // One-to-Many (Floor to Room)
        builder.Entity<Room>().HasIndex(room => new { room.Name, room.FloorId }).IsUnique();
        builder.Entity<Floor>()
            .HasMany(floor => floor.Rooms)
            .WithOne(room => room.Floor)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Room>()
            .HasOne(room => room.Floor)
            .WithMany(floor => floor.Rooms)
            .IsRequired();
        builder.Entity<Room>().Property(r => r.IsFavorite).HasDefaultValue(false);

        // One-to-many (Room to Device)
        builder.Entity<Device>().HasIndex(device => new { device.RoomId, device.Name }).IsUnique();
        builder.Entity<Device>()
            .HasOne(device => device.Room)
            .WithMany(room => room.Devices)
            .IsRequired();
        builder.Entity<Room>()
            .HasMany(room => room.Devices)
            .WithOne(device => device.Room)
            .IsRequired();
    }
}