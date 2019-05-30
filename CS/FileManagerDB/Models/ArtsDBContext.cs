using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace FileManagerDB.Models {
    public partial class ArtsDBContext : DbContext {
        public IConfiguration Configuration { get; }
        public ArtsDBContext(IConfiguration configuration) {
            Configuration = configuration;
        }

        public virtual DbSet<Arts> Arts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("FileDatabase"));
            }
        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Arts>(entity => {
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Gcrecord).HasColumnName("GCRecord");
                entity.Property(e => e.IsFolder).HasDefaultValueSql("((0))");
                entity.Property(e => e.LastWriteTime).HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.ParentId).HasColumnName("ParentID");
                entity.Property(e => e.SsmaTimeStamp)
                    .IsRequired()
                    .HasColumnName("SSMA_TimeStamp")
                    .IsRowVersion();
            });
        }
    }
}
