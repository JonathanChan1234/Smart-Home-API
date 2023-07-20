﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using smart_home_server.Db;

#nullable disable

namespace smarthomeserver.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230623035116_ChangeSceneAction")]
    partial class ChangeSceneAction
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ApplicationUserSmartHome", b =>
                {
                    b.Property<Guid>("UserHomeId")
                        .HasColumnType("char(36)");

                    b.Property<string>("UsersId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserHomeId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("HomeUsers", (string)null);
                });

            modelBuilder.Entity("ApplicationUserSmartHome1", b =>
                {
                    b.Property<Guid>("InstallerHomeId")
                        .HasColumnType("char(36)");

                    b.Property<string>("InstallersId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("InstallerHomeId", "InstallersId");

                    b.HasIndex("InstallersId");

                    b.ToTable("HomeInstallers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("smart_home_server.Auth.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("smart_home_server.Auth.Models.JwtBlackList", b =>
                {
                    b.Property<string>("Jti")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("ExpireOn")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Jti");

                    b.ToTable("JwtBlackList");
                });

            modelBuilder.Entity("smart_home_server.Auth.Models.RefreshToken", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("JwtId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("StatusLastUpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("RoomId", "Name")
                        .IsUnique();

                    b.ToTable("Devices");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Floor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("HomeId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("HomeId");

                    b.HasIndex("Name", "HomeId")
                        .IsUnique();

                    b.ToTable("Floors");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("FloorId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("IsFavorite")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("FloorId");

                    b.HasIndex("Name", "FloorId")
                        .IsUnique();

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.SmartHome", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("InstallerPassword")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UserPassword")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("Name", "OwnerId")
                        .IsUnique();

                    b.ToTable("SmartHome");
                });

            modelBuilder.Entity("smart_home_server.Mqtt.Client.Models.MqttClient", b =>
                {
                    b.Property<int>("ClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("HomeId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("Revoked")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("ClientId");

                    b.HasIndex("HomeId");

                    b.HasIndex("UserId");

                    b.ToTable("MqttClients");
                });

            modelBuilder.Entity("smart_home_server.Scenes.Models.Scene", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("HomeId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("HomeId");

                    b.HasIndex("Name", "HomeId")
                        .IsUnique();

                    b.ToTable("scenes");
                });

            modelBuilder.Entity("smart_home_server.Scenes.Models.SceneAction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("SceneId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("SceneId");

                    b.HasIndex("DeviceId", "SceneId")
                        .IsUnique();

                    b.ToTable("actions");
                });

            modelBuilder.Entity("smart_home_server.SmartDevices.Models.SmartDevice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Capabilities")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<Guid>("HomeId")
                        .HasColumnType("char(36)");

                    b.Property<string>("MainCategory")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<bool>("OnlineStatus")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Properties")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("StatusLastUpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SubCategory")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("HomeId");

                    b.HasIndex("RoomId");

                    b.ToTable("SmartDevices");
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Light", b =>
                {
                    b.HasBaseType("smart_home_server.Devices.Models.Device");

                    b.Property<bool>("Dimmable")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.ToTable("Lights");
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Shade", b =>
                {
                    b.HasBaseType("smart_home_server.Devices.Models.Device");

                    b.Property<bool>("HasLevel")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.ToTable("Shades");
                });

            modelBuilder.Entity("ApplicationUserSmartHome", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", null)
                        .WithMany()
                        .HasForeignKey("UserHomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ApplicationUserSmartHome1", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", null)
                        .WithMany()
                        .HasForeignKey("InstallerHomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("InstallersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("smart_home_server.Auth.Models.RefreshToken", b =>
                {
                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Device", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.Room", "Room")
                        .WithMany("Devices")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Floor", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", "Home")
                        .WithMany("Floors")
                        .HasForeignKey("HomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Home");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Room", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.Floor", "Floor")
                        .WithMany("Rooms")
                        .HasForeignKey("FloorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Floor");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.SmartHome", b =>
                {
                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", "Owner")
                        .WithMany("OwnerHome")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("smart_home_server.Mqtt.Client.Models.MqttClient", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", "Home")
                        .WithMany()
                        .HasForeignKey("HomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("smart_home_server.Auth.Models.ApplicationUser", "User")
                        .WithMany("MqttClients")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Home");

                    b.Navigation("User");
                });

            modelBuilder.Entity("smart_home_server.Scenes.Models.Scene", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", "Home")
                        .WithMany("Scenes")
                        .HasForeignKey("HomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Home");
                });

            modelBuilder.Entity("smart_home_server.Scenes.Models.SceneAction", b =>
                {
                    b.HasOne("smart_home_server.SmartDevices.Models.SmartDevice", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("smart_home_server.Scenes.Models.Scene", "Scene")
                        .WithMany("Actions")
                        .HasForeignKey("SceneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Scene");
                });

            modelBuilder.Entity("smart_home_server.SmartDevices.Models.SmartDevice", b =>
                {
                    b.HasOne("smart_home_server.Home.Models.SmartHome", "Home")
                        .WithMany("SmartDevices")
                        .HasForeignKey("HomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("smart_home_server.Home.Models.Room", "Room")
                        .WithMany("SmartDevices")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Home");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Light", b =>
                {
                    b.HasOne("smart_home_server.Devices.Models.Device", null)
                        .WithOne()
                        .HasForeignKey("smart_home_server.Devices.Models.Light", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("smart_home_server.Devices.Models.Shade", b =>
                {
                    b.HasOne("smart_home_server.Devices.Models.Device", null)
                        .WithOne()
                        .HasForeignKey("smart_home_server.Devices.Models.Shade", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("smart_home_server.Auth.Models.ApplicationUser", b =>
                {
                    b.Navigation("MqttClients");

                    b.Navigation("OwnerHome");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Floor", b =>
                {
                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.Room", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("SmartDevices");
                });

            modelBuilder.Entity("smart_home_server.Home.Models.SmartHome", b =>
                {
                    b.Navigation("Floors");

                    b.Navigation("Scenes");

                    b.Navigation("SmartDevices");
                });

            modelBuilder.Entity("smart_home_server.Scenes.Models.Scene", b =>
                {
                    b.Navigation("Actions");
                });
#pragma warning restore 612, 618
        }
    }
}
