using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Identity;

namespace EpreuveDeBloc.ViewModels;
public partial class AdminMenuViewModel : ObservableObject
{
    private readonly UserManager<IdentityUser> _userManager;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    // Injection du UserManager configuré dans MauiProgram.cs
    public AdminMenuViewModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [RelayCommand]
    public async Task ConnexionAsync()
    {
        var user = await _userManager.FindByNameAsync(Username);

        if (user != null)
        {
            //  vérifie le mot de passe
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, Password);

            if (isPasswordValid)
            {
                //  On récupère la liste de ses rôles
                var roles = await _userManager.GetRolesAsync(user);

                // IMPORTANT : Forcer le changement de vue sur le Thread Principal
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Redirection selon le rôle
                    if (roles.Contains("Admin"))
                    {
                        // On redirige vers l'interface Admin
                        await Shell.Current.GoToAsync("AdminPanel");
                    }
                    else
                    {
                        // On redirige vers l'interface Salarié classique
                        await Shell.Current.GoToAsync("MainPage"); 
                    }
                });
                return;
            }
        }

        // Optionnel : Afficher un message d'erreur si on arrive ici (mauvais login/mdp)
        // await Shell.Current.DisplayAlert("Erreur", "Identifiants incorrects.", "OK");
    }
}
