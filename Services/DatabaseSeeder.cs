using System.Text.Json;
using EpreuveDeBloc.Datas;
using EpreuveDeBloc.Models;
namespace EpreuveDeBloc.Services;
public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;

    public DatabaseSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedDataAsync()
    {
        // Charge ube seule fois
        if (_dbContext.Salaries.Any())
            return;

        // Service et Site car non fournit par l'Api
        var sites = new List<Site>
        {
            new Site { Nom = "Paris" },
            new Site { Nom = "Strasbourg" },
            new Site { Nom = "Lyon" }
        };

        var services = new List<Service>
        {
            new Service { Nom = "Comptabilité" },
            new Service { Nom = "Production" },
            new Service { Nom = "Accueil" }
        };
        _dbContext.Sites.AddRange(sites);
        _dbContext.Services.AddRange(services);
        await _dbContext.SaveChangesAsync();

        using var client = new HttpClient();
        string url = "https://randomuser.me/api/?results=400&nat=fr";
        
        string jsonResponse = await client.GetStringAsync(url);
        
        // JSON en objets C#
        var apiData = JsonSerializer.Deserialize<ApiResponse>(jsonResponse);

        if (apiData?.Results == null) return;

        var random = new Random();
        var nouveauxSalaries = new List<Salarie>();

        foreach (var user in apiData.Results)
        {
            // Random sites
            var randomSite = sites[random.Next(sites.Count)];
            var randomService = services[random.Next(services.Count)];

            var salarie = new Salarie
            {
                Nom = user.Name.Last,
                Prenom = user.Name.First,
                Email = user.Email,
                NumeroDeTelephoneFixe = user.Phone,
                NumeroDeTelephonePortable = user.Cell,

                SiteId = randomSite.Id,
                ServiceId = randomService.Id
            };

            nouveauxSalaries.Add(salarie);
        }
        _dbContext.Salaries.AddRange(nouveauxSalaries);
        await _dbContext.SaveChangesAsync();
    }
}