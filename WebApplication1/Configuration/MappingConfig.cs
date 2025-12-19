namespace WebApplication1.Utils.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        #region Ture
        TypeAdapterConfig<Tura, TuraReadDto>
            .NewConfig()
            .Map(dest => dest.PrevoznikNaziv,
                src => src.Prevoznik != null ? src.Prevoznik.Naziv : string.Empty)
            .Map(dest => dest.VoziloNaziv,
                src => src.Vozilo != null ? src.Vozilo.Naziv : null);

        TypeAdapterConfig<CreateTuraDto, Tura>
             .NewConfig()
             .Ignore(dest => dest.TuraId)
             .Ignore(dest => dest.PrevoznikId)
             .Ignore(dest => dest.VoziloId)
             .Ignore(dest => dest.Prevoznik)
             .Ignore(dest => dest.Vozilo)
             .Ignore(dest => dest.KlijentId)
             .Ignore(dest => dest.Klijent)
             .Ignore(dest =>dest.KreiranPutniNalog);


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
    }
}
