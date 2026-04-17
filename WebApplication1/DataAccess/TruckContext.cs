using WebApplication1.Utils.Tenant;

namespace WebApplication1.DataAccess;

public class TruckContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public TruckContext(DbContextOptions<TruckContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
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

    // === Troškovi ===
    public DbSet<TipTroska> TipoviTroskova { get; set; } = null!;
    public DbSet<NalogTrosak> NalogTroskovi { get; set; } = null!;

    // === Prihodi ===
    public DbSet<NalogPrihod> NalogPrihodi { get; set; } = null!;

    // === Gorivo ===
    public DbSet<GorivoZapis> GorivoZapisi { get; set; } = null!;

    // === Dodele vozača ===
    public DbSet<NasaVoziloVozacAssignment> NasaVoziloVozacAssignments { get; set; } = null!;

    // === Dokumenti ===
    public DbSet<TipDokumenta> TipoviDokumenata { get; set; } = null!;
    public DbSet<NalogDokument> NalogDokumenti { get; set; } = null!;

    // === Auth / Users ===
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Roles> Roles { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;

    // === Krug ===
    public DbSet<Krug> Krugovi { get; set; } = null!;
    public DbSet<KrugTrosak> KrugTroskovi { get; set; } = null!;

    // === Tenant ===
    public DbSet<Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== TENANT FK (all tenant-scoped entities) =====================

        modelBuilder.Entity<User>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Tura>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Nalog>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NasaVozila>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Prevoznik>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Klijent>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Poslovnica>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vinjeta>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NalogTrosak>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NalogPrihod>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NalogDokument>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GorivoZapis>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Log>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Krug>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<KrugTrosak>()
            .HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);

        // ===================== USER / AUTH =====================

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

        // ===================== TURA =====================

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
            .HasIndex(t => new { t.TenantId, t.RedniBroj })
            .IsUnique();

        modelBuilder.Entity<Tura>()
            .HasOne(t => t.Krug)
            .WithMany(k => k.Ture)
            .HasForeignKey(t => t.KrugId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tura>()
            .HasIndex(t => t.KrugId)
            .HasFilter("[KrugId] IS NOT NULL");

        // ===================== KRUG =====================

        modelBuilder.Entity<Krug>()
            .HasOne(k => k.Vozilo)
            .WithMany()
            .HasForeignKey(k => k.VoziloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Krug>()
            .HasIndex(k => k.VoziloId);

        modelBuilder.Entity<Krug>()
            .HasIndex(k => k.Status);

        // Samo jedan otvoren krug po vozilu (per tenant)
        modelBuilder.Entity<Krug>()
            .HasIndex(k => new { k.TenantId, k.VoziloId })
            .IsUnique()
            .HasFilter("[Status] = 'Otvoren'")
            .HasDatabaseName("UX_Krugovi_Otvoren_PoVozilu");

        // ===================== KRUG TROSKOVI =====================

        modelBuilder.Entity<KrugTrosak>()
            .HasOne(t => t.Krug)
            .WithMany(k => k.Troskovi)
            .HasForeignKey(t => t.KrugId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KrugTrosak>()
            .HasOne(t => t.TipTroska)
            .WithMany()
            .HasForeignKey(t => t.TipTroskaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KrugTrosak>()
            .HasIndex(t => t.KrugId);

        // ===================== NALOG =====================

        modelBuilder.Entity<Nalog>()
            .HasIndex(n => new { n.TenantId, n.TuraId })
            .IsUnique()
            .HasFilter("[StatusNaloga] <> 'Storniran' AND [StatusNaloga] <> 'Ponisten'");

        // ===================== NALOG PRIHODI =====================

        modelBuilder.Entity<NalogPrihod>()
            .HasOne(p => p.Nalog)
            .WithMany(n => n.Prihodi)
            .HasForeignKey(p => p.NalogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NalogPrihod>()
            .HasIndex(p => p.NalogId);

        modelBuilder.Entity<NalogPrihod>()
            .HasIndex(p => new { p.NalogId, p.IsSeededInitial })
            .IsUnique()
            .HasFilter("[IsSeededInitial] = 1");

        // ===================== NALOG TROSKOVI =====================

        modelBuilder.Entity<NalogTrosak>()
            .HasOne(t => t.Nalog)
            .WithMany(n => n.Troskovi)
            .HasForeignKey(t => t.NalogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NalogTrosak>()
            .HasOne(t => t.TipTroska)
            .WithMany(tp => tp.NalogTroskovi)
            .HasForeignKey(t => t.TipTroskaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NalogTrosak>()
            .HasIndex(t => t.NalogId);

        // ===================== NALOG DOKUMENTI =====================

        modelBuilder.Entity<NalogDokument>()
            .HasOne(d => d.Nalog)
            .WithMany(n => n.Dokumenti)
            .HasForeignKey(d => d.NalogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NalogDokument>()
            .HasOne(d => d.TipDokumenta)
            .WithMany(tp => tp.NalogDokumenti)
            .HasForeignKey(d => d.TipDokumentaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NalogDokument>()
            .HasIndex(d => d.NalogId)
            .HasFilter("[IsDeleted] = 0");

        // ===================== GORIVO ZAPISI =====================

        modelBuilder.Entity<GorivoZapis>()
            .HasOne(g => g.Vozilo)
            .WithMany(v => v.GorivoZapisi)
            .HasForeignKey(g => g.VoziloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GorivoZapis>()
            .HasOne(g => g.Nalog)
            .WithMany(n => n.GorivoZapisi)
            .HasForeignKey(g => g.NalogId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<GorivoZapis>()
            .HasIndex(g => g.VoziloId);

        modelBuilder.Entity<GorivoZapis>()
            .HasIndex(g => g.NalogId)
            .HasFilter("[NalogId] IS NOT NULL");

        // ===================== DODELE VOZACA =====================

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasOne(a => a.Vozilo)
            .WithMany(v => v.VozacAssignments)
            .HasForeignKey(a => a.VoziloId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.VoziloAssignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasCheckConstraint("CK_NasaVoziloVozacAssignment_Slot", "[SlotNumber] IN (1, 2)");

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasIndex(a => new { a.VoziloId, a.SlotNumber })
            .IsUnique()
            .HasFilter("[UnassignedAt] IS NULL");

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasIndex(a => a.EmployeeId)
            .IsUnique()
            .HasFilter("[UnassignedAt] IS NULL");

        modelBuilder.Entity<NasaVoziloVozacAssignment>()
            .HasIndex(a => a.VoziloId);

        // ===================== VINJETA =====================

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

        // ===================== TENANT =====================

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        // ===================== OSTALO =====================

        modelBuilder.Entity<NasaVozila>()
            .HasIndex(v => v.Naziv);

        modelBuilder.Entity<Prevoznik>()
            .HasIndex(p => p.Naziv);

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.TenantId, u.Username })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.UserId)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => new { e.TenantId, e.EmployeeNumber })
            .IsUnique()
            .HasFilter("[EmployeeNumber] IS NOT NULL");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.PoslovnicaId)
            .HasFilter("[PoslovnicaId] IS NOT NULL");

        modelBuilder.Entity<Log>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===================== GLOBAL QUERY FILTERS (tenant isolation) =====================

        modelBuilder.Entity<User>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Tura>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Nalog>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<NasaVozila>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Prevoznik>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Klijent>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Poslovnica>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Vinjeta>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<NalogTrosak>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<NalogPrihod>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<NalogDokument>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<GorivoZapis>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<NasaVoziloVozacAssignment>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Log>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<Krug>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
        modelBuilder.Entity<KrugTrosak>().HasQueryFilter(e => _tenantProvider == null || e.TenantId == _tenantProvider.CurrentTenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTenantId();
        return base.SaveChanges();
    }

    private void ApplyTenantId()
    {
        if (_tenantProvider is null) return;

        var tenantId = _tenantProvider.CurrentTenantId;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.TenantId = tenantId;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(ITenantEntity.TenantId)).IsModified = false;
                    break;
            }
        }
    }
}

