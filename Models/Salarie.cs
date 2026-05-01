using System;

namespace EpreuveDeBloc.Models;

public class Salarie
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string NumeroDeTelephoneFixe { get; set; } = string.Empty;
    public string NumeroDeTelephonePortable { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Site
    public int SiteId { get; set; }
    public Site? Site { get; set; }
    // Service
    public int ServiceId { get; set; }
    public Service? Service { get; set; }

}
