﻿// <auto-generated />
using lxEF.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace lxEF.Server.Migrations
{
    [DbContext(typeof(lxDbContext))]
    partial class lxDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("lxEF.Server.Data.Models.Character", b =>
                {
                    b.Property<string>("CitizenID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Age");

                    b.Property<int>("CharacterID");

                    b.Property<DateTime>("DateOfBirth");

                    b.Property<string>("FirstName");

                    b.Property<string>("Gender");

                    b.Property<bool>("IsDrunk");

                    b.Property<bool>("IsHigh");

                    b.Property<string>("LastName");

                    b.Property<string>("Nationality");

                    b.Property<string>("Ped");

                    b.Property<string>("UserId");

                    b.HasKey("CitizenID");

                    b.HasIndex("UserId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("lxEF.Server.Data.Models.DBUser", b =>
                {
                    b.Property<string>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("IP");

                    b.Property<bool>("IsAdmin");

                    b.Property<bool>("IsAuthenticated");

                    b.Property<bool>("IsBanned");

                    b.Property<string>("License");

                    b.Property<string>("SteamID");

                    b.Property<string>("Username");

                    b.HasKey("UserId");

                    b.ToTable("DBUsers");
                });

            modelBuilder.Entity("lxEF.Server.Data.Models.Character", b =>
                {
                    b.HasOne("lxEF.Server.Data.Models.DBUser", "User")
                        .WithMany("Characters")
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
