namespace WebApplication1.DataAccess;

public class TruckContext : DbContext
{
    public TruckContext(DbContextOptions<TruckContext> options)
        : base(options)
    {
    }

    // === Glavni entiteti ===
    public DbSet<Tura> Ture { get; set; } = null!;
    public DbSet<Nalog> Nalozi { get; set; } = null!;
    public DbSet<NasaVozila> NasaVozila { get; set; } = null!;
    public DbSet<Vinjeta> Vinjete { get; set; } = null!;
    public DbSet<Poslovnica> Poslovnice { get; set; } = null!;
    public DbSet<Prevoznik> Prevoznici { get; set; } = null!;
    public DbSet<Klijent> Klijenti { get; set; } = null!;
    public DbSet<VrstaNadogradnje> VrsteNadogradnje { get; set; } = null!;

    // === Auth / Users ===
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Roles> Roles { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER / AUTH

        modelBuilder.Entity<User>()
            .HasOne(u => u.Roles)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Poslovnica)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PoslovnicaId)
            .OnDelete(DeleteBehavior.SetNull);

        // TURA

        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Prevoznik)
            .WithMany()
            .HasForeignKey(t => t.PrevoznikId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Vozilo)
            .WithMany(v => v.Ture)
            .HasForeignKey(t => t.VoziloId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Klijent)
            .WithMany()
            .HasForeignKey(t => t.KlijentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tura>()
            .HasOne(t => t.VrstaNadogradnje)
            .WithMany()
            .HasForeignKey(t => t.VrstaNadogradnjeId)
            .OnDelete(DeleteBehavior.Restrict);

        // INDEXI – SAMO REALNI

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.PrevoznikId);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.VoziloId)
            .HasFilter("[VoziloId] IS NOT NULL");

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.KlijentId);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.StatusTure);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.RedniBroj)
            .IsUnique();

        // VINJETA

        modelBuilder.Entity<Vinjeta>()
            .HasOne(v => v.Vozilo)
            .WithMany(vo => vo.Vinjete)
            .HasForeignKey(v => v.VoziloId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => v.VoziloId)
            .HasFilter("[VoziloId] IS NOT NULL");

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => v.DrzavaKod);

        modelBuilder.Entity<Vinjeta>()
            .HasIndex(v => new { v.DatumPocetka, v.DatumIsteka });

        modelBuilder.Entity<Vinjeta>()
            .HasCheckConstraint(
                "CK_Vinjeta_ValidDateRange",
                "[DatumIsteka] > [DatumPocetka]"
            );

        // OSTALO

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

        modelBuilder.Entity<Log>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
