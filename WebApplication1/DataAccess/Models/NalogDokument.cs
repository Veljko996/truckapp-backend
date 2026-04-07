namespace WebApplication1.DataAccess.Models;

[Table("NalogDokumenti")]
public class NalogDokument : ITenantEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DokumentId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int NalogId { get; set; }
    public Nalog? Nalog { get; set; }

    public int TipDokumentaId { get; set; }
    public TipDokumenta? TipDokumenta { get; set; }

    [Required, MaxLength(500)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    [MaxLength(200)]
    public string? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    public string? ProcessingStatus { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
}
