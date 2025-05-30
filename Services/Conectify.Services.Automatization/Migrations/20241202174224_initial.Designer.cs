﻿// <auto-generated />
using System;
using Conectify.Services.Automatization.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Conectify.Services.Automatization.Migrations
{
    [DbContext(typeof(AutomatizationDb))]
    [Migration("20241202174224_initial")]
    partial class initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.InputPoint", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<Guid>("RuleId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RuleId");

                    b.ToTable("InputConnectors");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.OutputPoint", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<Guid>("RuleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RuleId");

                    b.ToTable("OutputConnectors");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.Rule", b =>
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

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.RuleConnector", b =>
                {
                    b.Property<Guid>("SourceRuleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetRuleId")
                        .HasColumnType("uuid");

                    b.HasKey("SourceRuleId", "TargetRuleId");

                    b.HasIndex("TargetRuleId");

                    b.ToTable("RuleConnector");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.InputPoint", b =>
                {
                    b.HasOne("Conectify.Services.Automatization.Models.Database.Rule", "Rule")
                        .WithMany("InputConnectors")
                        .HasForeignKey("RuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rule");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.OutputPoint", b =>
                {
                    b.HasOne("Conectify.Services.Automatization.Models.Database.Rule", "Rule")
                        .WithMany("OutputConnectors")
                        .HasForeignKey("RuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rule");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.RuleConnector", b =>
                {
                    b.HasOne("Conectify.Services.Automatization.Models.Database.OutputPoint", "SourceRule")
                        .WithMany("ContinousRules")
                        .HasForeignKey("SourceRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conectify.Services.Automatization.Models.Database.InputPoint", "TargetRule")
                        .WithMany("PreviousRules")
                        .HasForeignKey("TargetRuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SourceRule");

                    b.Navigation("TargetRule");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.InputPoint", b =>
                {
                    b.Navigation("PreviousRules");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.OutputPoint", b =>
                {
                    b.Navigation("ContinousRules");
                });

            modelBuilder.Entity("Conectify.Services.Automatization.Models.Database.Rule", b =>
                {
                    b.Navigation("InputConnectors");

                    b.Navigation("OutputConnectors");
                });
#pragma warning restore 612, 618
        }
    }
}
