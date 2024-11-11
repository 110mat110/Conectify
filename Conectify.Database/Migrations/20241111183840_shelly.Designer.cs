﻿// <auto-generated />
using System;
using Conectify.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Conectify.Database.Migrations
{
    [DbContext(typeof(ConectifyDb))]
    [Migration("20241111183840_shelly")]
    partial class shelly
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Conectify.Database.Models.Actuator", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsKnown")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SensorId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SourceDeviceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SensorId");

                    b.HasIndex("SourceDeviceId");

                    b.ToTable("Actuators");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.Rule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ParametersJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RuleType")
                        .HasColumnType("uuid");

                    b.Property<int>("X")
                        .HasColumnType("integer");

                    b.Property<int>("Y")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Rules");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.RuleConnector", b =>
                {
                    b.Property<Guid>("PreviousRuleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ContinuingRuleId")
                        .HasColumnType("uuid");

                    b.HasKey("PreviousRuleId", "ContinuingRuleId");

                    b.HasIndex("ContinuingRuleId");

                    b.ToTable("RuleConnector");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.RuleParameter", b =>
                {
                    b.Property<Guid>("SourceRuleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetRuleId")
                        .HasColumnType("uuid");

                    b.HasKey("SourceRuleId", "TargetRuleId");

                    b.HasIndex("TargetRuleId");

                    b.ToTable("RuleParameter");
                });

            modelBuilder.Entity("Conectify.Database.Models.Dashboard.Dashboard", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Background")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Position")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Dashboards");
                });

            modelBuilder.Entity("Conectify.Database.Models.Dashboard.DashboardDevice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DashBoardId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<int>("PosX")
                        .HasColumnType("integer");

                    b.Property<int>("PosY")
                        .HasColumnType("integer");

                    b.Property<string>("SourceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DashboardsDevice");
                });

            modelBuilder.Entity("Conectify.Database.Models.Dashboard.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("UserMail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Conectify.Database.Models.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IPAdress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsKnown")
                        .HasColumnType("boolean");

                    b.Property<string>("MacAdress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("SubscribeToAll")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Conectify.Database.Models.Metadata", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Exclusive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Metadatas");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Actuator>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MaxVal")
                        .HasColumnType("real");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MinVal")
                        .HasColumnType("real");

                    b.Property<float?>("NumericValue")
                        .HasColumnType("real");

                    b.Property<string>("StringValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TypeValue")
                        .HasColumnType("integer");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("MetadataId");

                    b.ToTable("ActuatorMetadatas");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Device>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MaxVal")
                        .HasColumnType("real");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MinVal")
                        .HasColumnType("real");

                    b.Property<float?>("NumericValue")
                        .HasColumnType("real");

                    b.Property<string>("StringValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TypeValue")
                        .HasColumnType("integer");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("MetadataId");

                    b.ToTable("DeviceMetadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Sensor>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MaxVal")
                        .HasColumnType("real");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uuid");

                    b.Property<float?>("MinVal")
                        .HasColumnType("real");

                    b.Property<float?>("NumericValue")
                        .HasColumnType("real");

                    b.Property<string>("StringValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TypeValue")
                        .HasColumnType("integer");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("MetadataId");

                    b.ToTable("SensorMetadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.Preference", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("SubscibeeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SubscriberId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SubscriberId");

                    b.ToTable("Preference");
                });

            modelBuilder.Entity("Conectify.Database.Models.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsKnown")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SourceDeviceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SourceDeviceId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("Conectify.Database.Models.Shelly.Shelly", b =>
                {
                    b.Property<string>("ShellyId")
                        .HasColumnType("text");

                    b.Property<string>("Json")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ShellyId");

                    b.ToTable("Shellys");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.DeviceVersion", b =>
                {
                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uuid");

                    b.Property<string>("ChipVersion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SoftwareId")
                        .HasColumnType("uuid");

                    b.HasKey("DeviceId");

                    b.HasIndex("SoftwareId");

                    b.ToTable("DeviceVersions");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.Software", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Softwares");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.SoftwareVersion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SoftwareId")
                        .HasColumnType("uuid");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SoftwareId");

                    b.ToTable("SoftwareVersions");
                });

            modelBuilder.Entity("Conectify.Database.Models.Values.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("DestinationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float?>("NumericValue")
                        .HasColumnType("real");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("uuid");

                    b.Property<string>("StringValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("TimeCreated")
                        .HasColumnType("bigint");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Conectify.Database.Models.Actuator", b =>
                {
                    b.HasOne("Conectify.Database.Models.Sensor", "Sensor")
                        .WithMany()
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Device", "SourceDevice")
                        .WithMany("Actuators")
                        .HasForeignKey("SourceDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");

                    b.Navigation("SourceDevice");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.RuleConnector", b =>
                {
                    b.HasOne("Conectify.Database.Models.Automatization.Rule", "ContinuingRule")
                        .WithMany("PreviousRules")
                        .HasForeignKey("ContinuingRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Automatization.Rule", "PreviousRule")
                        .WithMany("ContinuingRules")
                        .HasForeignKey("PreviousRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ContinuingRule");

                    b.Navigation("PreviousRule");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.RuleParameter", b =>
                {
                    b.HasOne("Conectify.Database.Models.Automatization.Rule", "SourceRule")
                        .WithMany("TargetParameters")
                        .HasForeignKey("SourceRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Automatization.Rule", "TargetRule")
                        .WithMany("SourceParameters")
                        .HasForeignKey("TargetRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SourceRule");

                    b.Navigation("TargetRule");
                });

            modelBuilder.Entity("Conectify.Database.Models.Dashboard.Dashboard", b =>
                {
                    b.HasOne("Conectify.Database.Models.Dashboard.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Actuator>", b =>
                {
                    b.HasOne("Conectify.Database.Models.Actuator", "Device")
                        .WithMany("Metadata")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Metadata", "Metadata")
                        .WithMany()
                        .HasForeignKey("MetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Device>", b =>
                {
                    b.HasOne("Conectify.Database.Models.Device", "Device")
                        .WithMany("Metadata")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Metadata", "Metadata")
                        .WithMany()
                        .HasForeignKey("MetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.MetadataConnector<Conectify.Database.Models.Sensor>", b =>
                {
                    b.HasOne("Conectify.Database.Models.Sensor", "Device")
                        .WithMany("Metadata")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Metadata", "Metadata")
                        .WithMany()
                        .HasForeignKey("MetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.Preference", b =>
                {
                    b.HasOne("Conectify.Database.Models.Device", "Subscriber")
                        .WithMany("Preferences")
                        .HasForeignKey("SubscriberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subscriber");
                });

            modelBuilder.Entity("Conectify.Database.Models.Sensor", b =>
                {
                    b.HasOne("Conectify.Database.Models.Device", "SourceDevice")
                        .WithMany("Sensors")
                        .HasForeignKey("SourceDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SourceDevice");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.DeviceVersion", b =>
                {
                    b.HasOne("Conectify.Database.Models.Device", "Device")
                        .WithMany("DeviceVersions")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Database.Models.Updates.Software", "Software")
                        .WithMany()
                        .HasForeignKey("SoftwareId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Software");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.SoftwareVersion", b =>
                {
                    b.HasOne("Conectify.Database.Models.Updates.Software", "Software")
                        .WithMany("Versions")
                        .HasForeignKey("SoftwareId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Software");
                });

            modelBuilder.Entity("Conectify.Database.Models.Actuator", b =>
                {
                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.Automatization.Rule", b =>
                {
                    b.Navigation("ContinuingRules");

                    b.Navigation("PreviousRules");

                    b.Navigation("SourceParameters");

                    b.Navigation("TargetParameters");
                });

            modelBuilder.Entity("Conectify.Database.Models.Device", b =>
                {
                    b.Navigation("Actuators");

                    b.Navigation("DeviceVersions");

                    b.Navigation("Metadata");

                    b.Navigation("Preferences");

                    b.Navigation("Sensors");
                });

            modelBuilder.Entity("Conectify.Database.Models.Sensor", b =>
                {
                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("Conectify.Database.Models.Updates.Software", b =>
                {
                    b.Navigation("Versions");
                });
#pragma warning restore 612, 618
        }
    }
}
