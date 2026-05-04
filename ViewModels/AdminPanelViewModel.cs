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
    private List<Salarie> _users = new();
    public AdminPanelViewModel(SalarieRepository salaryRepo,MainPageViewModel mainPageViewModel)
    {
        _salaryRepo = salaryRepo;
        Task.Run(async () => await mainPageViewModel.LoadUsersAsync(Users));
    }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom) || 
            string.IsNullOrWhiteSpace(Prenom) || 
            string.IsNullOrWhiteSpace(_nomSite) || 
            string.IsNullOrWhiteSpace(_nomService))
        {
            await Shell.Current.DisplayAlert("Erreur", "Tous les champs sont obligatoires.", "OK");
            return;
        }

        int siteId = await _salaryRepo.ObtenirOuCreerSiteAsync(NomSite.Trim());
        int serviceId = await _salaryRepo.ObtenirOuCreerServiceAsync(NomService.Trim());

        var nouveauSalarie = new Salarie()
        {
            Nom = Nom.Trim(),
            Prenom = Prenom.Trim(),
            NumeroDeTelephoneFixe = NumeroDeTelephoneFixe.Trim(),
            NumeroDeTelephonePortable = NumeroDeTelephonePortable.Trim(),
            Email = Email.Trim(),
            SiteId = siteId,
            ServiceId = serviceId
        };

        await _salaryRepo.AjouterSalarieAsync(nouveauSalarie);
        
        await Shell.Current.DisplayAlert("Succès", "Le salarié a été enregistré !", "OK");
        await Shell.Current.GoToAsync("..");
    }
}