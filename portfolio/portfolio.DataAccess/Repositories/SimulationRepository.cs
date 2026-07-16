using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.DataAccess.Models.Views;

namespace portfolio.DataAccess.Repositories;

public class SimulationRepository : ISimulationRepository
{
    public void SaveSimulation(Simulation result)
    {
        using var context = new PortfolioDbContext();
        context.Simulations.Add(result);
        context.SaveChanges();
    }
    
    public void UpdateSimulation(Simulation result)
    {
        using var context = new PortfolioDbContext();
        context.Simulations.Update(result);
        context.SaveChanges();
    }
    
    public void SaveSimulationPeriod(SimulationPeriod period)
    {
        using var context = new PortfolioDbContext();
        context.SimulationPeriod.Add(period);
        context.SaveChanges();
    }

    public async Task<Simulation?> GetSimulation(Guid id)
    {
        using var context = new PortfolioDbContext();
        var simulation = await context.Simulations
            .Include(s => s.Periods)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (simulation == null)
        {
            return null;
        }

        var portfolioIds = simulation.Periods.Select(p => p.PortfolioId).ToHashSet();
        var portfolios = await context.Portfolios
            .Include(p => p.PortfolioValues)
            .Include(p => p.PortfolioIndicators)
            .Where(p => portfolioIds.Contains(p.Id))
            .ToListAsync();

        foreach (var period in simulation.Periods)
        {
            period.Portfolio = portfolios.Single(p => p.Id == period.PortfolioId);
        }
        
        return simulation;
    }

    public async Task<Session> SaveSession(DateOnly date)
    {
        using var context = new PortfolioDbContext();
        var session = new Session
        {
            SessionDate = date,
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }
    
    public async Task<List<string>> GetPortfolioHashesFromSessionDate(DateOnly date)
    {
        using var context = new PortfolioDbContext();
        var session =  await context.Sessions
            .FirstOrDefaultAsync(s => s.SessionDate == date);
        
        if (session == null)
            return new List<string>();
        
        return await context.Simulations
            .Where(s => s.SessionId == session.Id)
            .SelectMany(s => s.Periods)
            .Select(s => s.Portfolio.Hash)
            .ToListAsync();
    }


    public async Task<Session?> GetLatestSession()
    {
        using var context = new PortfolioDbContext();
        return await context.Sessions.OrderByDescending(s => s.SessionDate).FirstOrDefaultAsync();
    }

    public async Task<Session?> GetSessionByDate(DateOnly date)
    {
        using var context = new PortfolioDbContext();
        return await context.Sessions.FirstOrDefaultAsync(s => s.SessionDate == date);
    }

    public async Task<List<SessionCountView>> GetAllSessionsWithCounts()
    {
        using var context = new PortfolioDbContext();

        return await context.Simulations
            .AsNoTracking()
            .Join(context.Sessions,
                s => s.SessionId,
                ss => ss.Id,
                (s, ss) => new { Simulation = s, Session = ss })
            .GroupBy(x => new { x.Session.Id, x.Session.SessionDate })
            .Select(g => new SessionCountView
            {
                Id = g.Key.Id,
                SessionDate = g.Key.SessionDate,
                SimulationCount = g.Count()
            })
            .OrderBy(s => s.SessionDate)
            .ToListAsync();
    }

    public List<Simulation> GetSimulations(int limit)
    {
        using var context = new PortfolioDbContext();
        return context.Simulations.Include(s => s.Periods)
            .ThenInclude(p => p.Portfolio)
            .ThenInclude(p => p.PortfolioValues)
            .OrderByDescending(s => s.Registered)
            .Take(limit)
            .ToList();
    }

    public async Task<List<Simulation>> GetSimulationsFromDate(DateTime date)
    {
        using var context = new PortfolioDbContext();
        return await context.Simulations
            .AsNoTracking()
            .OrderByDescending(s => s.Registered)
            .Where(s => s.Completed > date)
            .ToListAsync();
    }

    public async Task<SimulationView> GetLatestBestSimulation()
    {
        var qry = $"""
                   select s.*, p.securities_date from simulation s
                        inner join simulation_period sp on sp.simulation_id = s.id
                        inner join portfolio p on sp.portfolio_id = p.id
                   order by p.securities_date desc, s.percentage_change desc, s.init_money
                   limit 1
                   """;
        
        using var context = new PortfolioDbContext();
        return await context.SimulationView.FromSqlRaw(qry).FirstAsync();
    }
    
    public async Task<SimulationView?> GetBestSimulationByDate(DateOnly date)
    {
        var qry = $"""
                   select s.*, p.securities_date from simulation s
                        inner join simulation_period sp on sp.simulation_id = s.id
                        inner join portfolio p on sp.portfolio_id = p.id
                   where p.securities_date < '{date}'
                   order by p.securities_date desc, s.percentage_change desc, s.init_money
                   limit 1
                   """;
        
        using var context = new PortfolioDbContext();
        return await context.SimulationView.FromSqlRaw(qry).FirstOrDefaultAsync();
    }
}