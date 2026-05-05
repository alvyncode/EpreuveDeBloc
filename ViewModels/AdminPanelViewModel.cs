using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EpreuveDeBloc.Datas.Repositories;
using EpreuveDeBloc.Models;

namespace EpreuveDeBloc.ViewModels;

public partial class AdminPanelViewModel: ObservableObject
{
    private readonly SalarieRepository _salaryRepo;

    [ObservableProperty]
    private string _nom = string.Empty;

    [ObservableProperty]
    private string _prenom = string.Empty;
    
    [ObservableProperty]
    private string numeroDeTelephoneFixe;
    [ObservableProperty]
    private string numeroDeTelephonePortable;
    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string _nomSite = string.Empty;

    [ObservableProperty]
    private string _nomService = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Salarie> _users;
    // Mémorise l'ID du salarié qu'on modifie (null si on est en train d'en créer un nouveau)
    private int? _idSalarieEnEdition = null;

    // Titre dynamique pour le formulaire
    [ObservableProperty]
    private string titreFormulaire = "Nouveau Salarié";
    public AdminPanelViewModel(SalarieRepository salaryRepo,MainPageViewModel mainPageViewModel)
    {
        _salaryRepo = salaryRepo;
        _users = mainPageViewModel.Users;
        // Task.Run(async () => await mainPageViewModel.LoadUsersAsync(Users));
    }
    [RelayCommand]
    public void ModifierSalarie(Salarie salarieAEditer)
    {
        if (salarieAEditer == null) return;

        _idSalarieEnEdition = salarieAEditer.Id;

        titreFormulaire = $"Modifier : {salarieAEditer.Prenom} {salarieAEditer.Nom}";

        Nom = salarieAEditer.Nom;
        Prenom = salarieAEditer.Prenom;
        NumeroDeTelephoneFixe = salarieAEditer.NumeroDeTelephoneFixe;
        NumeroDeTelephonePortable = salarieAEditer.NumeroDeTelephonePortable;
        
    }

    [RelayCommand]
    public async Task SupprimerSalarieAsync(Salarie salarieASupprimer)
    {
        if (salarieASupprimer == null) return;

        bool isConfirmed = await Shell.Current.DisplayAlert("Confirmation", 
            $"Êtes-vous sûr de vouloir supprimer {salarieASupprimer.Prenom} {salarieASupprimer.Nom} ?", 
            "Oui", "Non");

        if (isConfirmed)
        {
            // 1. On attend bien la suppression en base de données
            await _salaryRepo.DeleteAsync(salarieASupprimer.Id);

            // 2. On force la mise à jour visuelle sur le bon thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // On cherche l'élément par son ID directement dans la liste du MainVM (ou ta liste observable liée à la vue)
                var salarieDansLaListe = Users.FirstOrDefault(s => s.Id == salarieASupprimer.Id);
                
                if (salarieDansLaListe != null)
                {
                    // On le retire de la VRAIE liste qui est affichée à l'écran
                    Users.Remove(salarieDansLaListe);
                }
            });
        }
    }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom) || 
            string.IsNullOrWhiteSpace(Prenom) || 
            string.IsNullOrWhiteSpace(NomSite) || 
            string.IsNullOrWhiteSpace(NomService))
        {
            await Shell.Current.DisplayAlert("Erreur", "Tous les champs sont obligatoires.", "OK");
            return;
        }
        int siteId = await _salaryRepo.ObtenirOuCreerSiteAsync(NomSite.Trim());
        int serviceId = await _salaryRepo.ObtenirOuCreerServiceAsync(NomService.Trim());

        var salarieASauvegarder = new Salarie()
        {
            Id = _idSalarieEnEdition ?? 0, 
            
            Nom = Nom.Trim(),
            Prenom = Prenom.Trim(),
            NumeroDeTelephoneFixe = NumeroDeTelephoneFixe?.Trim(),
            NumeroDeTelephonePortable = NumeroDeTelephonePortable?.Trim(),
            Email = Email?.Trim(),
            SiteId = siteId,
            ServiceId = serviceId
        };
        await _salaryRepo.AjouterOuModifierSalarieAsync(salarieASauvegarder);

        salarieASauvegarder.Site = new Site { Nom = NomSite.Trim() };
        salarieASauvegarder.Service = new Service { Nom = NomService.Trim() };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_idSalarieEnEdition.HasValue)
            {
                var ancienSalarie = Users.FirstOrDefault(s => s.Id == salarieASauvegarder.Id);
                
                if (ancienSalarie != null)
                {
                    var index = Users.IndexOf(ancienSalarie);
                    
                    Users.RemoveAt(index);
                    Users.Insert(0, salarieASauvegarder);
                }
            }
            else
            {
                Users.Insert(0, salarieASauvegarder);
            }
        });

        // 4. On nettoie l'interface
        string message = _idSalarieEnEdition.HasValue ? "Le salarié a été modifié !" : "Le salarié a été enregistré !";
        await Shell.Current.DisplayAlert("Succès", message, "OK");
    }
    [RelayCommand]
    public void Annuler()
    {
        Nom = string.Empty;
        Prenom = string.Empty;
        NomService = string.Empty;
        NomSite = string.Empty;
        Email = string.Empty;
        NumeroDeTelephoneFixe = string.Empty;
        NumeroDeTelephonePortable = string.Empty;
    }
}