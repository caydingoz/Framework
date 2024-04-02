﻿// <auto-generated />
using System;
using Framework.Test.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Framework.Test.API.Migrations
{
    [DbContext(typeof(TestDbContext))]
    partial class TestDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Framework.Test.API.Models.CachableTestModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("CachableTestModels");
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithManyTestModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SqlTestModels");
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithManyTestRelationModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SqlWithManyTestRelationModels");
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithOneTestModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SqlWithOneTestModel");
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithOneTestRelationModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SqlWithOneTestModelId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("SqlWithOneTestModelId");

                    b.ToTable("SqlWithOneTestRelationModels");
                });

            modelBuilder.Entity("RelationJoinTable", b =>
                {
                    b.Property<int>("SqlWithManyTestModelsId")
                        .HasColumnType("int");

                    b.Property<int>("SqlWithManyTestRelationModelsId")
                        .HasColumnType("int");

                    b.HasKey("SqlWithManyTestModelsId", "SqlWithManyTestRelationModelsId");

                    b.HasIndex("SqlWithManyTestRelationModelsId");

                    b.ToTable("RelationJoinTable");
                });

            modelBuilder.Entity("Framework.Test.API.Models.CachableTestModel", b =>
                {
                    b.OwnsMany("Framework.Test.API.Models.CachableTestChildModel", "Childs", b1 =>
                        {
                            b1.Property<int>("CachableTestModelId")
                                .HasColumnType("int");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("CachableTestModelId", "Id");

                            b1.ToTable("CachableTestChildModel");

                            b1.WithOwner()
                                .HasForeignKey("CachableTestModelId");
                        });

                    b.Navigation("Childs");
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithOneTestRelationModel", b =>
                {
                    b.HasOne("Framework.Test.API.Models.SqlWithOneTestModel", "SqlWithOneTestModel")
                        .WithMany("SqlWithOneTestRelationModels")
                        .HasForeignKey("SqlWithOneTestModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SqlWithOneTestModel");
                });

            modelBuilder.Entity("RelationJoinTable", b =>
                {
                    b.HasOne("Framework.Test.API.Models.SqlWithManyTestModel", null)
                        .WithMany()
                        .HasForeignKey("SqlWithManyTestModelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Framework.Test.API.Models.SqlWithManyTestRelationModel", null)
                        .WithMany()
                        .HasForeignKey("SqlWithManyTestRelationModelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Framework.Test.API.Models.SqlWithOneTestModel", b =>
                {
                    b.Navigation("SqlWithOneTestRelationModels");
                });
#pragma warning restore 612, 618
        }
    }
}
