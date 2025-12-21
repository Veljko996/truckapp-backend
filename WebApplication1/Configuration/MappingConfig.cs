using WebApplication1.Utils.DTOs.NalogDTO;

namespace WebApplication1.Utils.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        #region Ture
        TypeAdapterConfig<Tura, TuraReadDto>
             .NewConfig()
             .IgnoreNullValues(true)
             .Map(dest => dest.PrevoznikNaziv,
                  src => src.Prevoznik != null ? src.Prevoznik.Naziv : null)
             .Map(dest => dest.VoziloNaziv,
                  src => src.Vozilo != null ? src.Vozilo.Naziv : null)
             .Map(dest => dest.KlijentNaziv,
                  src => src.Klijent != null ? src.Klijent.NazivFirme : null)
             .Map(dest => dest.VrstaNadogradnjeNaziv,
                  src => src.VrstaNadogradnje != null ? src.VrstaNadogradnje.Naziv : null);

        TypeAdapterConfig<CreateTuraDto, Tura>
          .NewConfig()
          .Ignore(dest => dest.RedniBroj)
          .Ignore(dest => dest.TuraId)
          .Ignore(dest => dest.Prevoznik)
          .Ignore(dest => dest.Vozilo)
          .Ignore(dest => dest.Klijent)
          .Ignore(dest => dest.KreiranPutniNalog);


        TypeAdapterConfig<UpdateTuraDto, Tura>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.PrevoznikId)
            .Ignore(dest => dest.VoziloId)
            .Ignore(dest => dest.KlijentId)
            .Ignore(dest => dest.UlaznaCena)
            .Ignore(dest => dest.IzlaznaCena)
            .Ignore(dest => dest.Valuta)
            .Ignore(dest => dest.StatusTure);

        TypeAdapterConfig<UpdateTureBusinessDto, Tura>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.StatusTure);

        TypeAdapterConfig<UpdateTuraStatusDto, Tura>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<UpdateTuraNotesDto, Tura>
            .NewConfig()
            .IgnoreNullValues(true);

        #endregion Ture

        #region NasaVozila

        TypeAdapterConfig<NasaVozila, NasaVozilaReadDto>
            .NewConfig()
            .Map(dest => dest.Vinjete, src => src.Vinjete);

        TypeAdapterConfig<NasaVozilaCreateDto, NasaVozila>
            .NewConfig()
            .Ignore(dest => dest.VoziloId);

        TypeAdapterConfig<NasaVozilaUpdateDto, NasaVozila>
            .NewConfig()
            .IgnoreNullValues(true);


        #endregion NasaVozila

        #region Vinjete
       
        TypeAdapterConfig<Vinjeta, VinjetaReadDto>
            .NewConfig()
            .Map(dest => dest.NazivVozila, src => src.Vozilo != null ? src.Vozilo.Naziv : null);

        TypeAdapterConfig<VinjetaCreateDTO, Vinjeta>
            .NewConfig()
            .Ignore(dest => dest.VinjetaId);

        TypeAdapterConfig<VinjetaUpdateDto, Vinjeta>
            .NewConfig()
            .IgnoreNullValues(true);

        #endregion Vinjete

        #region Prevoznici

        TypeAdapterConfig<Prevoznik, PrevoznikDto>
            .NewConfig();

        TypeAdapterConfig<CreatePrevoznikDto, Prevoznik>
            .NewConfig()
            .Ignore(dest => dest.PrevoznikId);

        TypeAdapterConfig<UpdatePrevoznikDto, Prevoznik>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.PrevoznikId);

        #endregion Prevoznici

        #region VrstaNadogradnje

        TypeAdapterConfig<VrstaNadogradnje, VrstaNadogradnjeReadDto>
            .NewConfig();

        #endregion VrstaNadogradnje

        #region Nalog

        TypeAdapterConfig<Nalog, NalogReadDto>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.PrevoznikNaziv,
                 src => src.Prevoznik != null ? src.Prevoznik.Naziv : null);

        TypeAdapterConfig<CreateNalogDto, Nalog>
            .NewConfig()
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        TypeAdapterConfig<AssignPrevoznikDto, Nalog>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.TuraId)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        TypeAdapterConfig<UpdateBusinessFieldsDto, Nalog>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.TuraId)
            .Ignore(dest => dest.PrevoznikId)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        TypeAdapterConfig<UpdateNotesDto, Nalog>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.TuraId)
            .Ignore(dest => dest.PrevoznikId)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        TypeAdapterConfig<UpdateStatusDto, Nalog>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.TuraId)
            .Ignore(dest => dest.PrevoznikId)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        TypeAdapterConfig<MarkIstovarenDto, Nalog>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.NalogId)
            .Ignore(dest => dest.NalogBroj)
            .Ignore(dest => dest.TuraId)
            .Ignore(dest => dest.PrevoznikId)
            .Ignore(dest => dest.Prevoznik)
            .Ignore(dest => dest.Tura);

        #endregion Nalog
    }
}
