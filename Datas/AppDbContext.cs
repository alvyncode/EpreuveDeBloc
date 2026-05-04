using EpreuveDeBloc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EpreuveDeBloc.Datas;
public class AppDbContext: IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = "server=localhost;user=root;password=;database=annuaire_db";
            optionsBuilder.UseMySql(
                connectionString, 
                ServerVersion.AutoDetect(connectionString)
            );
        }
    }
    public DbSet<Salarie> Salaries { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Site> Sites { get; set; }

}
