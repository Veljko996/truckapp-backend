namespace WebApplication1.Services.QuestPdfServices;

/// <summary>Fiksni tekstovi za Fetico PDF (MTS struktura, Fetico brend).</summary>
internal static class FeticoNalogPdfText
{
    public const string ValutaPlacanja =
        "RSD: 60 dana od dana prijema originalne overene dokumentacije (CMR, EX, T1) – dinarski po srednjem kursu NBS na dan istovara. " +
        "EUR: originalna faktura, overen CMR-tovarni list, EX, T1 dokument – na dan istovara.";

    public const string NacinPlacanja = "Plaćanje po srednjem kursu NBS-a (RSD) / po uslovima iz fakture (EUR).";

    public const string NapomenaFinansije =
        "Prilikom fakturisanja pozvati se na broj datog naloga. Faktura se u određenim slučajevima radi bez PDV-a (uskladiti sa ponudom).";

    public static readonly string[] UsloviSr =
    {
        "Direktan kontakt sa našim poslovnim partnerima nije dozvoljen.",
        "Zaštita imena poslovnih partnera mora biti zagarantovana.",
        "Pretovar i dotovari su strogo zabranjeni, osim ako nije drugačije pismeno naznačeno.",
        "Kašnjenje na utovar ili istovar, nezgode ili kvarovi moraju biti odmah prijavljeni pismenim putem.",
        "Vozilo mora imati CMR osiguranje.",
        "Vozač je dužan da prisustvuje utovaru i istovaru i da sva odstupanja od naloga odmah prijavi.",
        "Primalac naloga je dužan da sa pošiljkom koja mu je poverena postupa sa pažnjom – da je pošiljka potpuno zaštićena od oštećenja, prljavštine i vremenskih uslova, kao i da je u odgovarajućoj meri zaštićena od neovlašćenog pristupa.",
        "U slučaju da se na istovaru utvrdi bilo kakav nedostatak kod pošiljke (manjak, oštećenje), primalac naloga je dužan da nalagodavcu nadoknadi štetu u punom iznosu.",
        "Plaćanje transporta nije moguće bez originalnog, pečatiranog CMR-a bez ikakvih primedbi od strane primaoca.",
        "Vreme za utovar i izvozno carinjenje je 24 časa, a za uvozno carinjenje i istovar 48 časova, bez naknade.",
        "Vozač je odgovoran za osovinsko opterećenje i za pravilan raspored tereta na vozilu.",
        "Vozač mora strogo da vodi računa o dokumentaciji i svaki EX/T-T5/TC11 mora biti overen jer je to uslov za naplatu vozarine.",
        "Ukoliko primalac naloga želi da angažuje drugog prevoznika za transport pošiljke koja je predmet ovog naloga, mora odmah da obavesti nalagodavca. U slučaju da dobije dozvolu od nalagodavca, primalac naloga mora ovaj nalog overiti svojim pečatom i vratiti ga nalagodavcu. Takođe, primalac naloga zadržava obavezu ispunjavanja svih gore navedenih uslova.",
        "U slučaju otkazivanja naloga za rad na dan utovara robe ili na dan pre utovara robe od strane prevoznika, prevoznik je dužan da nalagodavcu nadoknadi razliku između svih troškova izvršenja prevoza drugim vozilom ili angažovanjem drugog prevoznika i naknade za prevoz koja je trebalo da bude isplaćena prevozniku prema nalogu za rad.",
        "U situaciji kada nalog za terminski utovar/istovar nije ispoštovan od strane primaoca naloga, primalac će platiti nalagodavcu ugovornu kaznu u visini od 150 EUR za svakih narednih 24 časa kašnjenja, u RSD protivvrednosti po srednjem kursu NBS na dan plaćanja, kao i nadoknaditi nalagodavcu sve troškove, ugovorne kazne, penale itd. zbog kašnjenja na utovar/istovar, u skladu sa politikom koju nalagodavac Fetico Milk & Logistic ima za konkretan utovar.",
        "U slučaju da bilo koji od navedenih uslova nije zadovoljen, ne možemo Vam garantovati isplatu vozarine ili nekog dela iste."
    };

    public const string UputstvoFaktureSr =
        "Neophodno je da nam, nakon istovara, pošaljete skeniranu fakturu za transport i CMR na mail. " +
        "Faktura mora imati sve Vaše podatke (tačan naziv, adresu, telefon, PIB i matični broj; za ino partnera IBAN i SWIFT). " +
        "Mora glasiti na: Aleksandar Jovanović PR proizvodnja ostalih prehrambenih proizvoda i usluge prevoza Fetico Milk & Logistic – Matije Gupca 107, 26232 Starčevo, Srbija, PIB 109267361. " +
        "Mora imati mesto i datum izdavanja računa i datum prometa usluga (promet kao datum istovara sa CMR-a). Valuta na fakturi obavezna. " +
        "Napomena o poreskom oslobađanju obavezna za rezidente SR (čl. 24/12 Zakona o PDV-u – u skladu sa vrstom posla). " +
        "Neispravna faktura neće biti knjižena dok se ne ispravi; pomera se i rok plaćanja.";

    public const string FooterPoslednjaNapomena =
        "Obavezno nas kontaktirajte ako niste dobili sve strane naloga; u suprotnom smatraćemo da ste ih primili.";
}
