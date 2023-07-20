using smart_home_server.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using smart_home_server.Home.Models;
using smart_home_server.Mqtt.Client.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace smart_home_server.Db;

public class AppDbContext : IdentityUserContext<ApplicationUser>
{
    public DbSet<SmartHome> Homes { get; set; } = null!;
    public DbSet<Floor> Floors { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<JwtBlackList> JwtBlackList { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Scene> Scenes { get; set; } = null!;
    public DbSet<SceneAction> SceneActions { get; set; } = null!;
    public DbSet<MqttClient> MqttClients { get; set; } = null!;
    public DbSet<SmartDevice> SmartDevices { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var mainCategoryConverter = new EnumToStringConverter<MainCategory>();
        var subCategoryConverter = new EnumToStringConverter<SubCategory>();

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

        // One-to-many (MqttClient to User/Home)
        builder.Entity<MqttClient>()
            .HasOne(m => m.User)
            .WithMany(u => u.MqttClients)
            .IsRequired();
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.MqttClients)
            .WithOne(m => m.User)
            .IsRequired();

        // One-to-many (SmartHome to Scene)
        builder.Entity<SmartHome>()
            .HasMany(home => home.Scenes)
            .WithOne(scene => scene.Home)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Scene>()
            .HasOne(scene => scene.Home)
            .WithMany(home => home.Scenes)
            .IsRequired();
        builder.Entity<Scene>().HasIndex(scene => new { scene.Name, scene.HomeId }).IsUnique();

        // One-to-many (Scene to SceneAction)
        builder.Entity<SceneAction>()
            .HasOne(action => action.Scene)
            .WithMany(scene => scene.Actions)
            .IsRequired();
        builder.Entity<SceneAction>()
            .Property(a => a.Action)
            .HasColumnType("BLOB")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                s => JsonConvert.DeserializeObject<Dictionary<string, object?>>(s) ?? new(),
                ValueComparer.CreateDefault(typeof(Dictionary<string, object>), true)
            );
        builder.Entity<SceneAction>()
            .HasIndex(action => new { action.DeviceId, action.SceneId })
            .IsUnique();
        // OnDelete cascade (when scene/device is deleted, the action will be deleted as well)
        builder.Entity<Scene>()
            .HasMany(s => s.Actions)
            .WithOne(a => a.Scene)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SmartDevice>()
            .HasMany(d => d.Actions)
            .WithOne(a => a.Device)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Smart Devices
        builder.Entity<SmartDevice>()
            .Property(d => d.Properties)
            .HasColumnType("BLOB")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                s => JsonConvert.DeserializeObject<Dictionary<string, object?>>(s) ?? new(),
                ValueComparer.CreateDefault(typeof(Dictionary<string, object>), true)
            );
        builder.Entity<SmartDevice>()
            .Property(d => d.Capabilities)
            .HasColumnType("BLOB")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                s => JsonConvert.DeserializeObject<Dictionary<string, object?>>(s) ?? new(),
                ValueComparer.CreateDefault(typeof(Dictionary<string, object>), true)
            );
        builder.Entity<SmartDevice>()
            .Property(d => d.MainCategory)
            .HasConversion(mainCategoryConverter);
        builder.Entity<SmartDevice>()
            .Property(d => d.SubCategory)
            .HasConversion(subCategoryConverter);
        builder.Entity<SmartDevice>()
            .HasOne(d => d.Home)
            .WithMany(h => h.SmartDevices)
            .IsRequired();
        builder.Entity<SmartDevice>()
            .HasOne(d => d.Room)
            .WithMany(r => r.SmartDevices)
            .IsRequired();
    }
}