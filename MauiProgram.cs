using EpreuveDeBloc.Datas;
using EpreuveDeBloc.Datas.Repositories;
using EpreuveDeBloc.Services;
using EpreuveDeBloc.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
		builder.Services.AddSingleton<MainPageViewModel>();
		builder.Services.AddSingleton<AdminPanelViewModel>();
		builder.Services.AddTransient<SalarieRepository>();
        builder.Services.AddTransient<MainPage>();
		

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
        }
    }


}
