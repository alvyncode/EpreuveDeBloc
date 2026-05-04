using Microsoft.EntityFrameworkCore;
using EpreuveDeBloc.Models;

namespace EpreuveDeBloc.Datas.Repositories;

public class SalarieRepository
{
    private AppDbContext _context;
    public SalarieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Salarie>> GetAll()
    {
        return await _context.Salaries
        .Include(s => s.Site)
        .Include(s => s.Service)
        .ToListAsync();     
    }
    public List<Site> GetSites()
    {
        return _context.Sites.ToList();
    }
    public List<Service> GetServices()
    {
        return _context.Services.ToList();
    }
    public async Task<List<Salarie>> RechercheAsync(Salarie criteres)
    {
        var requete = _context.Salaries.AsQueryable();
        var query = _context.Salaries
                            .Include(s => s.Site)
                            .Include(s => s.Service)
                            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteres.Nom))
        {
            query = query.Where(s => s.Nom.Contains(criteres.Nom) || s.Prenom.Contains(criteres.Prenom));
        }

        if (criteres.SiteId > 0)
        {
            query = query.Where(s => s.SiteId == criteres.SiteId);
        }

        if (criteres.ServiceId > 0)
        {
            query = query.Where(s => s.ServiceId == criteres.ServiceId);
        }

        return await query.ToListAsync();
    }
    public async Task AjouterSalarieAsync(Salarie nouveauSalarie)
    {
        if (nouveauSalarie == null)
            throw new ArgumentNullException(nameof(nouveauSalarie));

        await _context.Salaries.AddAsync(nouveauSalarie);
        await _context.SaveChangesAsync();
    }
    public async Task<int> ObtenirOuCreerSiteAsync(string nomSite)
    {
    
        var siteExistant = await _context.Sites
            .FirstOrDefaultAsync(s => s.Nom.ToLower() == nomSite.ToLower());

        if (siteExistant != null)
        {
            return siteExistant.Id; 
        }


        var nouveauSite = new Site { Nom = nomSite };
        await _context.Sites.AddAsync(nouveauSite);
        await _context.SaveChangesAsync();
        
        return nouveauSite.Id;
    }

    public async Task<int> ObtenirOuCreerServiceAsync(string nomService)
    {
        var serviceExistant = await _context.Services
            .FirstOrDefaultAsync(s => s.Nom.ToLower() == nomService.ToLower());

        if (serviceExistant != null) return serviceExistant.Id;

        var nouveauService = new Service { Nom = nomService };
        await _context.Services.AddAsync(nouveauService);
        await _context.SaveChangesAsync();
        
        return nouveauService.Id;
    }
}
