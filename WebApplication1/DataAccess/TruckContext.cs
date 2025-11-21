namespace WebApplication1.DataAccess;

public class TruckContext : DbContext
{
        public TruckContext(DbContextOptions<TruckContext> options): base(options)
        { }

        // Glavni entiteti
        public DbSet<Tura> Ture { get; set; } = null!;
        public DbSet<NasaVozila> NasaVozila { get; set; } = null!;
        public DbSet<Vinjeta> Vinjete { get; set; } = null!;
        public DbSet<Poslovnica> Poslovnice { get; set; } = null!;
        public DbSet<Prevoznik> Prevoznici { get; set; } = null!;
        public DbSet<TuraStatusLog> TuraStatusLogs { get; set; } = null!;
        // Lookup tabele
        public DbSet<StatusKonacni> StatusiKonacni { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Log> Logs { get; set; } = null!;
        public DbSet<Roles> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Roles relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Roles)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Employee relationship (1:1 optional)
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Employee - Poslovnica relationship
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Poslovnica)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PoslovnicaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tura - Prevoznik relationship
        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Prevoznik)
            .WithMany()
            .HasForeignKey(t => t.PrevoznikId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tura - NasaVozila relationship (optional)
        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Vozilo)
            .WithMany(v => v.Ture)
            .HasForeignKey(t => t.VoziloId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tura - TuraStatusLog relationship
        modelBuilder.Entity<TuraStatusLog>()
            .HasOne(tsl => tsl.Tura)
            .WithMany(t => t.StatusLogovi)
            .HasForeignKey(tsl => tsl.TuraId)
            .OnDelete(DeleteBehavior.Cascade);

        // Vinjeta - NasaVozila relationship
        modelBuilder.Entity<Vinjeta>()
            .HasOne(v => v.Vozilo)
            .WithMany(vo => vo.Vinjete)
            .HasForeignKey(v => v.VoziloId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.VoziloId)
            .HasFilter("[VoziloId] IS NOT NULL");

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.StatusTrenutni);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.UtovarDatum);

        modelBuilder.Entity<TuraStatusLog>()
            .HasIndex(tsl => tsl.TuraId);

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => v.VoziloId)
            .HasFilter("[VoziloId] IS NOT NULL");

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => v.DrzavaKod);

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => new { v.DatumPocetka, v.DatumIsteka });

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.PrevoznikId);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.RedniBroj)
            .IsUnique();

        // Business rule: One active vignette per vehicle per country
        // Note: This is enforced at application level, but we add a check constraint
        modelBuilder.Entity<Vinjeta>()
            .HasCheckConstraint("CK_Vinjeta_ValidDateRange", "[DatumIsteka] > [DatumPocetka]");

        // Note: StatusTrenutniVreme can be null initially, but should be set when status changes
        // This is enforced at application level via domain methods

        // Ensure valid date range for tours
        modelBuilder.Entity<Tura>()
            .HasCheckConstraint("CK_Tura_ValidDateRange", "[IstovarDatum] >= [UtovarDatum]");

        modelBuilder.Entity<NasaVozila>()
            .HasIndex(v => v.Naziv);

        modelBuilder.Entity<Prevoznik>()
            .HasIndex(p => p.Naziv);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.UserId)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeNumber)
            .HasFilter("[EmployeeNumber] IS NOT NULL");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.PoslovnicaId)
            .HasFilter("[PoslovnicaId] IS NOT NULL");

        // Poslovnica entity configuration
        // IMPORTANT: Ako kolona u bazi nije "BrojTelefona", 
        // promeni HasColumnName na pravo ime kolone (npr. "Phone" ili "Telefon")
        // 
        // Primer ako je kolona u bazi "Phone":
        // modelBuilder.Entity<Poslovnica>()
        //     .Property(p => p.BrojTelefona)
        //     .HasColumnName("Phone");
        //
        // Primer ako je kolona u bazi "Telefon":
        // modelBuilder.Entity<Poslovnica>()
        //     .Property(p => p.BrojTelefona)
        //     .HasColumnName("Telefon");
        //
        // Ako kolona uopšte ne postoji u bazi, pokreni FIX_BROJTELEFONA_COLUMN.sql
        // ili kreiraj kolonu ručno:
        // ALTER TABLE Poslovnice ADD BrojTelefona NVARCHAR(50) NULL;
        
        modelBuilder.Entity<Poslovnica>()
            .ToTable("Poslovnice"); // Eksplicitno ime tabele

        // Log - User relationship (optional)
        modelBuilder.Entity<Log>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
}
