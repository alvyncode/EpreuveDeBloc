using EpreuveDeBloc.Datas;
using EpreuveDeBloc.Datas.Repositories;
using EpreuveDeBloc.Services;
using EpreuveDeBloc.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EpreuveDeBloc.Views;

namespace EpreuveDeBloc;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
				#if DEBUG
					builder.Logging.AddDebug();
				#endif
			// CHaine de connexion permettant la mise en place d'un singleton
		string connectionString = "Server=127.0.0.1;Port=3306;Database=annuaire_db;Uid=root;Pwd=";
        builder.Services.AddDbContext<AppDbContext>(options =>options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
		builder.Services.AddSingleton<MainPageViewModel>();
		builder.Services.AddTransient<AdminPanelViewModel>();
		builder.Services.AddTransient<AdminMenuViewModel>();
		builder.Services.AddTransient<SalarieRepository>();
        builder.Services.AddTransient<AdminPanel>();
        builder.Services.AddTransient<AdminMenu>();
        builder.Services.AddTransient<MainPage>();
		builder.Services.AddIdentityCore<IdentityUser>(options => 
		{
			// Tu peux configurer tes règles ici
			options.Password.RequireDigit = true;
			options.Password.RequiredLength = 6;
			options.SignIn.RequireConfirmedAccount = false;
		}).AddRoles<IdentityRole>()
		// lie Identity à Entity Framework (et donc à ton MySQL via Pomelo)
		.AddEntityFrameworkStores<AppDbContext>();

        var app = builder.Build();
        InitialiserBaseDeDonnees(app);

        return app;
		
    }
    private static void InitialiserBaseDeDonnees(MauiApp app)
    {
		// Scope = espace de travail temp. Pour eviter de recreer une bd à chanque lancement
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
			
			var seeder = new DatabaseSeeder(db);
        	Task.Run(async () => await seeder.SeedDataAsync()).Wait();
		var services = scope.ServiceProvider;

			try
			{
				var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

				Task.Run(async () =>
				{
					if (!await roleManager.RoleExistsAsync("Admin"))
					{
						await roleManager.CreateAsync(new IdentityRole("Admin"));
					}
					var adminEmail = "admin@annuaire.com";
					var adminUser = await userManager.FindByEmailAsync(adminEmail);

					if (adminUser == null)
					{
						// Création de l'utilisateur
						adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
						var result = await userManager.CreateAsync(adminUser, "Admin123!"); //  Mot de passe par défaut

						// Rôle "Admin"
						if (result.Succeeded)
						{
							await userManager.AddToRoleAsync(adminUser, "Admin");
							
						}
					}
				}).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation : {ex.Message}");
			}	
        }
    }


}