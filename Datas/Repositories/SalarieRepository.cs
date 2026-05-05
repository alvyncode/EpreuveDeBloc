using Microsoft.EntityFrameworkCore;
using EpreuveDeBloc.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Data;

namespace EpreuveDeBloc.Datas.Repositories;

public class SalarieRepository
{
    private AppDbContext _context;
    public SalarieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ObservableCollection<Salarie>> GetAll()
    {
        var salariesList = await _context.Salaries
        .Include(s => s.Site)
        .Include(s => s.Service)
        .ToListAsync();     
        return new ObservableCollection<Salarie>(salariesList);    
    }
    public List<Site> GetSites()
    {
        return _context.Sites.ToList();
    }
    public List<Service> GetServices()
    {
        return _context.Services.ToList();
    }
    public async Task<ObservableCollection<Salarie>> RechercheAsync(Salarie criteres)
    {
        var liste = new ObservableCollection<Salarie>();

        //  Récupération de la connexion SQL brute (ADO.NET) depuis Entity Framework
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();

        //Requette SQL
        var sql = new StringBuilder(@"
            SELECT 
                s.Id, s.Nom, s.Prenom, s.NumeroDeTelephoneFixe, s.NumeroDeTelephonePortable, s.Email, s.SiteId, s.ServiceId,
                si.Id AS SiteIdJoin, si.Nom AS SiteNom,
                se.Id AS ServiceIdJoin, se.Nom AS ServiceNom
            FROM Salaries s
            INNER JOIN Sites si ON s.SiteId = si.Id
            INNER JOIN Services se ON s.ServiceId = se.Id
            WHERE 1=1 ");

        // Ajout dynamique des conditions et paramètres 100% ADO.NET
        if (!string.IsNullOrWhiteSpace(criteres.Nom))
        {
            sql.Append(" AND (s.Nom LIKE @nom OR s.Prenom LIKE @prenom)");
            
            var paramNom = command.CreateParameter();
            paramNom.ParameterName = "@nom";
            paramNom.Value = $"%{criteres.Nom}%";
            command.Parameters.Add(paramNom);

            var paramPrenom = command.CreateParameter();
            paramPrenom.ParameterName = "@prenom";
            paramPrenom.Value = $"%{criteres.Prenom}%"; 
            command.Parameters.Add(paramPrenom);
        }

        if (criteres.SiteId > 0)
        {
            sql.Append(" AND s.SiteId = @siteId");
            var paramSite = command.CreateParameter();
            paramSite.ParameterName = "@siteId";
            paramSite.Value = criteres.SiteId;
            command.Parameters.Add(paramSite);
        }

        if (criteres.ServiceId > 0)
        {
            sql.Append(" AND s.ServiceId = @serviceId");
            var paramService = command.CreateParameter();
            paramService.ParameterName = "@serviceId";
            paramService.Value = criteres.ServiceId;
            command.Parameters.Add(paramService);
        }

        command.CommandText = sql.ToString();

        // Exécution de la requête et lecture des résultats (DataReader)
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            //  Mappage manuel : on extrait les données cellule par cellule pour recréer les objets
            var salarie = new Salarie
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                // Utilisation de ternaires pour gérer les valeurs NULL potentielles de la DB
                Nom = reader.IsDBNull(reader.GetOrdinal("Nom")) ? null : reader.GetString(reader.GetOrdinal("Nom")),
                Prenom = reader.IsDBNull(reader.GetOrdinal("Prenom")) ? null : reader.GetString(reader.GetOrdinal("Prenom")),
                NumeroDeTelephoneFixe = reader.IsDBNull(reader.GetOrdinal("NumeroDeTelephoneFixe")) ? null : reader.GetString(reader.GetOrdinal("NumeroDeTelephoneFixe")),
                NumeroDeTelephonePortable = reader.IsDBNull(reader.GetOrdinal("NumeroDeTelephonePortable")) ? null : reader.GetString(reader.GetOrdinal("NumeroDeTelephonePortable")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                SiteId = reader.GetInt32(reader.GetOrdinal("SiteId")),
                ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),

                // Mappage des relations (LE include qui m'aurait pris 5s à évrire)
                Site = new Site
                {
                    Id = reader.GetInt32(reader.GetOrdinal("SiteIdJoin")),
                    Nom = reader.IsDBNull(reader.GetOrdinal("SiteNom")) ? null : reader.GetString(reader.GetOrdinal("SiteNom"))
                },
                Service = new Service
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ServiceIdJoin")),
                    Nom = reader.IsDBNull(reader.GetOrdinal("ServiceNom")) ? null : reader.GetString(reader.GetOrdinal("ServiceNom"))
                }
            };

            liste.Add(salarie);
        }

        return liste;
    }
    public async Task AjouterOuModifierSalarieAsync(Salarie salarie)
    {
        // 1. Récupération et ouverture de la connexion ADO.NET
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        // 2. Création des paramètres partagés (utiles pour l'INSERT et l'UPDATE)
        // On gère les valeurs nulles (C# null devient DBNull.Value en SQL)
        var paramNom = command.CreateParameter();
        paramNom.ParameterName = "@nom";
        paramNom.Value = (object)salarie.Nom ?? DBNull.Value;
        command.Parameters.Add(paramNom);

        var paramPrenom = command.CreateParameter();
        paramPrenom.ParameterName = "@prenom";
        paramPrenom.Value = (object)salarie.Prenom ?? DBNull.Value;
        command.Parameters.Add(paramPrenom);
        
        var paramNumeroDeTelephoneFixe = command.CreateParameter();
        paramNumeroDeTelephoneFixe.ParameterName = "@numerodetelephonefixe";
        paramNumeroDeTelephoneFixe.Value = (object)salarie.NumeroDeTelephoneFixe ?? DBNull.Value;
        command.Parameters.Add(paramNumeroDeTelephoneFixe);

        var paramNumeroDeTelephonePortable= command.CreateParameter();
        paramNumeroDeTelephonePortable.ParameterName = "@numerodetelephoneportable";
        paramNumeroDeTelephonePortable.Value = (object)salarie.NumeroDeTelephonePortable ?? DBNull.Value;
        command.Parameters.Add(paramNumeroDeTelephonePortable);

        var paramEmail = command.CreateParameter();
        paramEmail.ParameterName = "@email";
        paramEmail.Value = (object)salarie.Email ?? DBNull.Value;
        command.Parameters.Add(paramEmail);

        var paramSiteId = command.CreateParameter();
        paramSiteId.ParameterName = "@siteId";
        paramSiteId.Value = salarie.SiteId;
        command.Parameters.Add(paramSiteId);

        var paramServiceId = command.CreateParameter();
        paramServiceId.ParameterName = "@serviceId";
        paramServiceId.Value = salarie.ServiceId;
        command.Parameters.Add(paramServiceId);

        // 3. Logique de séparation INSERT / UPDATE
        if (salarie.Id == 0)
        {
            // --- CAS : CRÉATION (INSERT) ---
            // On insère, puis on demande à la base de données de nous renvoyer l'ID fraîchement généré
            command.CommandText = @"
                INSERT INTO Salaries (Nom, Prenom,NumeroDeTelephoneFixe,NumeroDeTelephonePortable,Email, SiteId, ServiceId) 
                VALUES (@nom, @prenom ,@numerodetelephonefixe,@numerodetelephoneportable ,@email, @siteId, @serviceId);
                SELECT CAST(SCOPE_IDENTITY() as int);"; // Note : SCOPE_IDENTITY() est spécifique à SQL Server

            // ExecuteScalarAsync exécute la requête et retourne la première colonne de la première ligne (notre nouvel ID)
            var newId = await command.ExecuteScalarAsync();
            
            if (newId != null && newId != DBNull.Value)
            {
                salarie.Id = (int)newId; // On met à jour l'objet C# comme le fait Entity Framework
            }
        }
        else
        {
            // --- CAS : MODIFICATION (UPDATE) ---
            // Il nous faut un paramètre supplémentaire pour la clause WHERE
            var paramId = command.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = salarie.Id;
            command.Parameters.Add(paramId);

            command.CommandText = @"
                UPDATE Salaries 
                SET Nom = @nom, 
                    Prenom = @prenom,
                    NumeroDeTelephoneFixe = @numerodetelephonefixe
                    NumeroDeTelephonePortable = @numerodetelephoneportable
                    Email = @email
                    SiteId = @siteId, 
                    ServiceId = @serviceId
                WHERE Id = @id;";

            // ExecuteNonQueryAsync exécute la requête sans rien retourner (juste le nombre de lignes affectées)
            await command.ExecuteNonQueryAsync();
        }
    }
    // public async Task AjouterOuModifierSalarieAsync(Salarie salarie)

    // {

    //     if (salarie.Id == 0)

    //     {

    //         await _context.Salaries.AddAsync(salarie);

    //     }

    //     else

    //     {

    //         _context.Salaries.Update(salarie);

    //     }

    //     await _context.SaveChangesAsync();

    // }
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
    public async Task<bool> DeleteAsync(int id) 
    {
        try
        {
            // 1. Récupération et ouverture de la connexion ADO.NET
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            // requête SQL de suppression
            command.CommandText = "DELETE FROM Salaries WHERE Id = @id;";

            // 3Securisation contre les inject
            var paramId = command.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = id;
            command.Parameters.Add(paramId);

            //exécution de la requête
            // ExecuteNonQueryAsync retourne le nombre de lignes affectées par la requête
            int lignesSupprimees = await command.ExecuteNonQueryAsync();

            // Si 0 ligne, cela veut dire que l'ID n'existait pas (false)
            return lignesSupprimees > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"💥 ERREUR SUPPRESSION SQL : {ex.Message}");
            return false;
        }
    }
}
