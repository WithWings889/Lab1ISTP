using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebApplication1
{
    public partial class RecyclingNowContext : DbContext
    {
        public RecyclingNowContext()
        {
        }

        public RecyclingNowContext(DbContextOptions<RecyclingNowContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Factory> Factory { get; set; }
        public virtual DbSet<FactoryGarbageType> FactoryGarbageType { get; set; }
        public virtual DbSet<Garbage> Garbage { get; set; }
        public virtual DbSet<GarbageMaterial> GarbageMaterial { get; set; }
        public virtual DbSet<GarbageType> GarbageType { get; set; }
        public virtual DbSet<Material> Material { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=DESKTOP-P4TOSEA; Database=RecyclingNow; Trusted_Connection=True; ");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Factory>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Website).HasMaxLength(50);
            });

            modelBuilder.Entity<FactoryGarbageType>(entity =>
            {
                entity.ToTable("Factory_GarbageType");

                entity.HasOne(d => d.IdFactoryNavigation)
                    .WithMany(p => p.FactoryGarbageType)
                    .HasForeignKey(d => d.IdFactory)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Factory_GarbageType_Factory");

                entity.HasOne(d => d.IdGarbageTypeNavigation)
                    .WithMany(p => p.FactoryGarbageType)
                    .HasForeignKey(d => d.IdGarbageType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Factory_GarbageType_GarbageType");
            });

            modelBuilder.Entity<Garbage>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GarbageMaterial>(entity =>
            {
                entity.ToTable("Garbage_Material");

                entity.HasOne(d => d.IdGarbageNavigation)
                    .WithMany(p => p.GarbageMaterial)
                    .HasForeignKey(d => d.IdGarbage)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Garbage_Material_Garbage");

                entity.HasOne(d => d.IdMaterialNavigation)
                    .WithMany(p => p.GarbageMaterial)
                    .HasForeignKey(d => d.IdMaterial)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Garbage_Material_Material");
            });

            modelBuilder.Entity<GarbageType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.Property(e => e.Info).HasColumnType("ntext");

                entity.Property(e => e.MaterialCard).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.IdGarbageTypeNavigation)
                    .WithMany(p => p.Material)
                    .HasForeignKey(d => d.IdGarbageType)
                    .HasConstraintName("FK_Material_GarbageType");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
