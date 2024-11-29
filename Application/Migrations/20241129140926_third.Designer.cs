﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TgQueueTime.Application;

#nullable disable

namespace Application.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241129140926_third")]
    partial class third
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("Domain.Entities.ClientsEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<long>("QueueId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("QueueServiceId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("QueueServiceId");

                    b.ToTable("QueueClients");
                });

            modelBuilder.Entity("Domain.Entities.OrganizationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("Domain.Entities.QueueEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WindowNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Queues");
                });

            modelBuilder.Entity("Domain.Entities.QueueServicesEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("QueueId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ServiceId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("QueueId");

                    b.HasIndex("ServiceId");

                    b.ToTable("QueueServices");
                });

            modelBuilder.Entity("Domain.Entities.ServiceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AverageTime")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Domain.Entities.ClientsEntity", b =>
                {
                    b.HasOne("Domain.Entities.QueueServicesEntity", null)
                        .WithMany()
                        .HasForeignKey("QueueServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.QueueEntity", b =>
                {
                    b.HasOne("Domain.Entities.OrganizationEntity", null)
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.QueueServicesEntity", b =>
                {
                    b.HasOne("Domain.Entities.QueueEntity", null)
                        .WithMany()
                        .HasForeignKey("QueueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.ServiceEntity", null)
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.ServiceEntity", b =>
                {
                    b.HasOne("Domain.Entities.OrganizationEntity", null)
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
