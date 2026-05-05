using System.Collections.Generic;
using System.Threading.Tasks;
using EpreuveDeBloc.Models;
using Microsoft.EntityFrameworkCore;

namespace EpreuveDeBloc.Datas.Repositories;
public class ServiceRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // CREATE en SQL pur
    public async Task AddServiceAsync(Service service)
    {
        // ExecuteSqlInterpolatedAsync convertit automatiquement les variables en paramètres SQL sécurisés
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Services (Nom) VALUES ({service.Nom})");
    }

    // READ en SQL pur
    public async Task<List<Service>> GetAllServicesAsync()
    {
        // Assure-toi d'avoir un DbSet<ServiceItem> Services { get; set; } dans ton AppDbContext
        return await _dbContext.Services
            .FromSqlRaw("SELECT * FROM Services")
            .ToListAsync();
    }

    // UPDATE en SQL pur
    public async Task UpdateServiceAsync(Service service)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Services SET Nom = {service.Nom}");
    }

    // DELETE en SQL pur
    public async Task DeleteServiceAsync(int id)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM Services WHERE Id = {id}");
    }
}
