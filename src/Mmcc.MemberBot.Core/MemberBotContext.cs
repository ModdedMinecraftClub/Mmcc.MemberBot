using Microsoft.EntityFrameworkCore;
using Mmcc.MemberBot.Core.Models;

#nullable disable

namespace Mmcc.MemberBot.Core
{
    public partial class MemberBotContext : DbContext
    {
        public MemberBotContext()
        {
        }

        public MemberBotContext(DbContextOptions<MemberBotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.AppId)
                    .HasName("PRIMARY");

                entity.ToTable("applications");

                entity.Property(e => e.AppId).HasColumnType("int(11)");

                entity.Property(e => e.AppStatus)
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppTime)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");

                entity.Property(e => e.AuthorDiscordId)
                    .IsRequired()
                    .HasColumnType("mediumtext")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");

                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");

                entity.Property(e => e.ImageUrl)
                    .HasColumnType("varchar(250)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");

                entity.Property(e => e.MessageContent)
                    .HasColumnType("varchar(800)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");

                entity.Property(e => e.MessageUrl)
                    .IsRequired()
                    .HasColumnType("varchar(250)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
