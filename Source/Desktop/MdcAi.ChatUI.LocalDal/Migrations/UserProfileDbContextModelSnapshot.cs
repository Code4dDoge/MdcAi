﻿// <auto-generated />
using System;
using MdcAi.ChatUI.LocalDal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MdcAi.ChatUI.LocalDal.Migrations
{
    [DbContext(typeof(UserProfileDbContext))]
    partial class UserProfileDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbCategory", b =>
                {
                    b.Property<string>("IdCategory")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("IconGlyph")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdSettings")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("IdCategory");

                    b.HasIndex("IdSettings");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            IdCategory = "default",
                            Description = "General Purpose AI Assistant",
                            IdSettings = "general",
                            Name = "General"
                        });
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbChatSettings", b =>
                {
                    b.Property<string>("IdSettings")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("FrequencyPenalty")
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .HasColumnType("TEXT");

                    b.Property<string>("Premise")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("PresencePenalty")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Streaming")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Temperature")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TopP")
                        .HasColumnType("TEXT");

                    b.HasKey("IdSettings");

                    b.ToTable("ChatSettings");

                    b.HasData(
                        new
                        {
                            IdSettings = "general",
                            FrequencyPenalty = 1m,
                            Model = "gpt-4-1106-preview",
                            Premise = "You are a helpful but cynical and humorous assistant (but not over the top). You give short answers, straight, to the point answers. Use md syntax and be sure to specify language for code blocks.",
                            PresencePenalty = 1m,
                            Streaming = true,
                            Temperature = 1m,
                            TopP = 1m
                        });
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbConversation", b =>
                {
                    b.Property<string>("IdConversation")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedTs")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdCategory")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdSettingsOverride")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsTrash")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("IdConversation");

                    b.HasIndex("IdCategory");

                    b.HasIndex("IdSettingsOverride");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbMessage", b =>
                {
                    b.Property<string>("IdMessage")
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedTs")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdConversation")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdMessageParent")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCurrentVersion")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsTrash")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Role")
                        .HasColumnType("TEXT");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("IdMessage");

                    b.HasIndex("IdConversation");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbCategory", b =>
                {
                    b.HasOne("MdcAi.ChatUI.LocalDal.DbChatSettings", "Settings")
                        .WithMany()
                        .HasForeignKey("IdSettings")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbConversation", b =>
                {
                    b.HasOne("MdcAi.ChatUI.LocalDal.DbCategory", "Category")
                        .WithMany()
                        .HasForeignKey("IdCategory");

                    b.HasOne("MdcAi.ChatUI.LocalDal.DbChatSettings", "SettingsOverride")
                        .WithMany()
                        .HasForeignKey("IdSettingsOverride");

                    b.Navigation("Category");

                    b.Navigation("SettingsOverride");
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbMessage", b =>
                {
                    b.HasOne("MdcAi.ChatUI.LocalDal.DbConversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("IdConversation");

                    b.Navigation("Conversation");
                });

            modelBuilder.Entity("MdcAi.ChatUI.LocalDal.DbConversation", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
