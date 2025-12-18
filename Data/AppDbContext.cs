using System;
using System.Collections.Generic;
using Eventify.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) // DbContext yapılandırıcısı, DI ile veritabanı ayarlarını alır
    {
    }

    public DbSet<Attendance> Attendances { get; set; } // Katılım tablosu

    public DbSet<Event> Events { get; set; } // Etkinlik tablosu

    public DbSet<User> Users { get; set; } // Kullanıcı tablosu

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //=> optionsBuilder.UseSqlServer("Server=OZKAN\\SQLEXPRESS;Database=EventifyDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"); // SQL Server bağlantı dizesi

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(d => d.Event).WithMany(p => p.Attendances) // Attendance ile Event arasında ilişki
                .HasForeignKey(d => d.EventId) // Foreign key: EventId
                .OnDelete(DeleteBehavior.Cascade) // Silme davranışı: null yap
                .HasConstraintName("FK_Attendances_Events"); // Kısıtlama adı

            entity.HasOne(d => d.User).WithMany(p => p.Attendances) // Attendance ile User arasında ilişki
                .HasForeignKey(d => d.UserId) // Foreign key: UserId
                .OnDelete(DeleteBehavior.Cascade) // Silme davranışı: null yap
                .HasConstraintName("FK_Attendances_Users"); // Kısıtlama adı
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Date).HasColumnType("datetime"); // Tarih alanı tipi
            entity.Property(e => e.Location).HasMaxLength(50); // Lokasyon max 50 karakter
            entity.Property(e => e.Name).HasMaxLength(50); // İsim max 50 karakter
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50); // E-posta max 50 karakter
            entity.Property(e => e.Password).HasMaxLength(50); // Şifre max 50 karakter
            entity.Property(e => e.Username).HasMaxLength(50); // Kullanıcı adı max 50 karakter
            entity.Property(e => e.Role).HasMaxLength(50); // Rol max 50 karakter
        });

    }

}
