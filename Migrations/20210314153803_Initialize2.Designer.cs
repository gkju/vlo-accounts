﻿// <auto-generated />
using System;
using AccountsData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VLO_BOARDS.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210314153803_Initialize2")]
    partial class Initialize2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Property", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BoardName")
                        .HasColumnType("text");

                    b.Property<string>("BoardName1")
                        .HasColumnType("text");

                    b.Property<Guid?>("PropertyId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("propertiesId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("BoardName");

                    b.HasIndex("BoardName1");

                    b.HasIndex("PropertyId");

                    b.HasIndex("RoleId");

                    b.ToTable("Property");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<Guid?>("scopeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("scopeId");

                    b.ToTable("BoardsRoles");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Role");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Scope", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<Guid?>("ScopeId")
                        .HasColumnType("uuid");

                    b.Property<string>("SubName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ScopeId");

                    b.ToTable("Scopes");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Scope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Board", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.Prefix", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("HexColor")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<Guid?>("PrefixesId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PrefixesId");

                    b.HasIndex("RoleId");

                    b.ToTable("Prefix");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Properties", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("PropertiesSets");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Thread", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Desc")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("boardId")
                        .HasColumnType("integer");

                    b.Property<string>("boardName")
                        .HasColumnType("text");

                    b.Property<bool>("inheritProperties")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("boardName");

                    b.ToTable("Threads");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.AuthorityProperty", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Property");

                    b.Property<int>("Data")
                        .HasColumnType("integer");

                    b.ToTable("AuthorityProperties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.BannedProperty", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Property");

                    b.Property<bool>("Data")
                        .HasColumnType("boolean");

                    b.ToTable("BannedProperties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.MayManageRolesProperty", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Property");

                    b.Property<bool>("Data")
                        .HasColumnType("boolean");

                    b.ToTable("MayManageRoleProperties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.MemberProperty", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Property");

                    b.Property<bool>("Data")
                        .HasColumnType("boolean");

                    b.ToTable("MemberProperties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.Prefixes", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Property");

                    b.ToTable("Prefixes");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Roles.BannedRole", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Role");

                    b.HasDiscriminator().HasValue("BannedRole");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Roles.BoardRole", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Role");

                    b.HasDiscriminator().HasValue("BoardRole");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Roles.GlobalRole", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Role");

                    b.HasDiscriminator().HasValue("GlobalRole");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.RoleScope.BoardScope", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Scope");

                    b.HasDiscriminator().HasValue("BoardScope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.RoleScope.GlobalBoardScope", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Scope");

                    b.HasDiscriminator().HasValue("GlobalBoardScope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.RoleScope.GlobalScope", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Scope");

                    b.HasDiscriminator().HasValue("GlobalScope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.RoleScope.NewsScope", b =>
                {
                    b.HasBaseType("AccountsData.Models.DataModels.Abstracts.Scope");

                    b.HasDiscriminator().HasValue("NewsScope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Property", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Board", null)
                        .WithMany("defaultProperties")
                        .HasForeignKey("BoardName");

                    b.HasOne("AccountsData.Models.DataModels.Board", null)
                        .WithMany("memberProperties")
                        .HasForeignKey("BoardName1");

                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithMany("properties")
                        .HasForeignKey("PropertyId");

                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Role", null)
                        .WithMany("properties")
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Role", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Scope", "scope")
                        .WithMany()
                        .HasForeignKey("scopeId");

                    b.Navigation("scope");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Scope", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Scope", null)
                        .WithMany("ParentScopes")
                        .HasForeignKey("ScopeId");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.Prefix", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Implementations.Properties.Prefixes", null)
                        .WithMany("Data")
                        .HasForeignKey("PrefixesId");

                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Role", "role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("role");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Thread", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Board", "board")
                        .WithMany()
                        .HasForeignKey("boardName");

                    b.Navigation("board");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AccountsData.Models.DataModels.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.AuthorityProperty", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithOne()
                        .HasForeignKey("AccountsData.Models.DataModels.Implementations.Properties.AuthorityProperty", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.BannedProperty", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithOne()
                        .HasForeignKey("AccountsData.Models.DataModels.Implementations.Properties.BannedProperty", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.MayManageRolesProperty", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithOne()
                        .HasForeignKey("AccountsData.Models.DataModels.Implementations.Properties.MayManageRolesProperty", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.MemberProperty", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithOne()
                        .HasForeignKey("AccountsData.Models.DataModels.Implementations.Properties.MemberProperty", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.Prefixes", b =>
                {
                    b.HasOne("AccountsData.Models.DataModels.Abstracts.Property", null)
                        .WithOne()
                        .HasForeignKey("AccountsData.Models.DataModels.Implementations.Properties.Prefixes", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Property", b =>
                {
                    b.Navigation("properties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Role", b =>
                {
                    b.Navigation("properties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Abstracts.Scope", b =>
                {
                    b.Navigation("ParentScopes");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Board", b =>
                {
                    b.Navigation("defaultProperties");

                    b.Navigation("memberProperties");
                });

            modelBuilder.Entity("AccountsData.Models.DataModels.Implementations.Properties.Prefixes", b =>
                {
                    b.Navigation("Data");
                });
#pragma warning restore 612, 618
        }
    }
}
