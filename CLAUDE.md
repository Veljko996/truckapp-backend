# CLAUDE.md — TruckApp Backend

ASP.NET Core Web API, **.NET 9**. Layered arhitektura. Solution: `WebApplication1.sln`.

## Projekti u solution-u

- `WebApplication1/` — glavni Web API.
- `WebApplication1.Tests/` — NUnit + Moq testovi (trenutno `Services/NalogServiceTests.cs`).
- `ArchiveOldOrders/` — Azure Function (.NET isolated) za arhiviranje starih naloga.

## Komande

```bash
# iz truckapp-backend/
dotnet build WebApplication1.sln
dotnet run --project WebApplication1            # dev, čita appsettings.Development.json
dotnet test                                     # NUnit testovi
```

Swagger je uključen u Development/Staging (i u prod na `/swagger`).

## Arhitektura — strogi slojevi

```
Controller  →  Service  →  Repository  →  TruckContext (EF Core)
(tanak)        (biznis)     (tanak,        (DbSet + query filteri)
                            samo pristup)
```

- **Kontroleri su tanki** — bez biznis logike; samo mapiranje request→service i HTTP odgovor.
- **Biznis logika živi u servisima.** Repozitorijumi ne smeju da upijaju poslovna pravila.
- Svaki domen ima par `Repository/<X>Repository` + `Services/<X>Services` sa `I<X>Service`/`I<X>Repository` interfejsima.
- DI registracija je centralizovana u `Configuration/ApiConfig.cs` (`builder.AddApiConfiguration()`), scoped lifetime.
- Mapiranje: **Mapster** (`Utils/Mapping/MappingConfig.cs`), `IgnoreNullValues(true)`.
- PDF: **QuestPDF** (`Services/QuestPdfServices`, Community licenca).

## Multi-tenancy (NE razbij)

- Entiteti implementiraju `ITenantEntity` (`DataAccess/Models/ITenantEntity.cs`).
- `TruckContext.OnModelCreating` postavlja globalni `HasQueryFilter` na svaki tenant entitet →
  automatski `WHERE TenantId == CurrentTenantId`. **Ne dodaji ručni tenant filter u repoima.**
- `SaveChanges`/`SaveChangesAsync` → `ApplyTenantId()`: na `Added` upisuje `TenantId`, na `Modified` zaključava ga (`IsModified = false`).
- Tenant izvor: `Utils/Tenant/HttpTenantProvider` čita `tenant_id` claim (fallback 1).
- Ako pišeš upit koji svesno prelazi tenant granicu (retko), koristi `IgnoreQueryFilters()` — i dobro razmisli.

## Auth / RBAC

- JWT u **HTTP-only cookie-jima** (`accessToken` 4h, `refreshToken` 7 dana). Token se čita u `Program.cs` `OnMessageReceived` iz cookie-ja.
- `/api/auth/refresh-token` namerno zaobilazi auth events (challenge/fail se gutaju).
- Claims: `Name`, `NameIdentifier` (UserId), `Role`, `tenant_id`.
- Svaki controller ima class-level `[Authorize(Roles=...)]`; osetljivi endpointi imaju method-level override.
- **Vozač** pristup: `Services/NalogVozacAccessServices/NalogVozacAccessService` — `ApplyVozacFilter`, `CanAccessNalogAsync`, `CanAccessVoziloAsync`. Puni opis: `../.cursor/rules/docs/autorizacija-vozac-rbac.md`.

## API konvencije

- Update endpointi → `204 NoContent` gde je već standardizovano; drži konzistentno.
- Validacija na service nivou, predvidljivi HTTP odgovori.
- **Ne redizajniraj oblik postojećih endpointa** osim ako je eksplicitno traženo.
- Greške: `Middleware/GlobalExceptionHandler` + `AddProblemDetails()`. Custom izuzeci u `Utils/Exceptions`.
- Lokalizacija poruka preko `Accept-Language` (frontend šalje `sr-Latn`), `Resources/`.

## Baza & migracije

- `DataAccess/TruckContext.cs` (DbSet-ovi, indeksi, query filteri, relacije).
- Modeli: `DataAccess/Models/`.
- **Migracije se rade ručno** kao numerisane SQL skripte u `../docs/Scripts/` (`01_…`→`15_…`). Nema EF Migrations. Kad menjaš šemu, dodaj sledeću numerisanu skriptu.

## Fajl storage

`Services/FileStorage` — Azure Blob (`Azure.Storage.Blobs`). Lokalni upload folder `uploads/nalog-dokumenti` (dev).
Queue: `Services/QueuePublisherServices` (`Azure.Storage.Queues`) — smer ka Azure Function obradi dokumenata.

## Konvencije koda

- Domenska imena su na **srpskom** (Nalog, Tura, Krug, Prevoznik, Gorivo…). Prati postojeću nomenklaturu, ne prevodi.
- DTO-ovi u `Utils/DTOs/`. Kad dodaješ polje, ažuriraj model + DTO + Mapster mapping.
- Logovanje zahteva: `Middleware/RequestLoggingMiddleware` + `Services/LogServices` (`Log` entitet).
