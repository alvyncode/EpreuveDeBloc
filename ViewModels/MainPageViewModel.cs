using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EpreuveDeBloc.Datas.Repositories;
using EpreuveDeBloc.Models;
namespace EpreuveDeBloc.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly SalarieRepository _salaryRepo;

    [ObservableProperty]
    private List<Salarie> _users = new();

    #region Propriétés de chargement


    [ObservableProperty]
    private List<Site> _villes;

    [ObservableProperty]
    private List<Service> _services;

    #endregion

    #region Propriété de recherche

    [ObservableProperty]
    private string _nomRecherche;
    
    [ObservableProperty]
    private Site _siteRecherche;    

    [ObservableProperty]
    private Service _serviceRecherche;
    
    #endregion
    public MainPageViewModel(SalarieRepository salaryRepo)
    {
        _salaryRepo = salaryRepo;

        _services = _salaryRepo.GetServices();
        var nullService = new Service();
        _services.Add(nullService);
        
        _villes = _salaryRepo.GetSites();
        var nullSite = new Site();
        _villes.Add(nullSite);
        
        Task.Run(async () => await LoadUsersAsync(Users));
    }

    public async Task LoadUsersAsync(List<Salarie> users)
    {
        try
        {
            var salariesList = await _salaryRepo.GetAll();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                users.Clear();
                foreach (var salarie in salariesList)
                {
                    users.Add(salarie);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de chargement : {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task RechercheDeSalarieAsync()
    {
        var salarieRecherche = new Salarie() 
        { 
            Nom = _nomRecherche,
            Prenom = _nomRecherche,
            SiteId = _siteRecherche?.Id ?? 0, 
            ServiceId = _serviceRecherche?.Id ?? 0 
        };
        Users = await _salaryRepo.RechercheAsync(salarieRecherche);
    }
    [RelayCommand]
    public async Task AllerVersNouvelleVueAsync()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
{
    try 
    {
        await Shell.Current.GoToAsync("AdminMenu");
    }
    catch (Exception ex) 
    {
        // Mets un point d'arrêt (breakpoint) ici pour lire ex.Message !
        System.Diagnostics.Debug.WriteLine($"ERREUR : {ex.Message}");
    }
});
    }
}