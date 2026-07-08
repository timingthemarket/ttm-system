using portfolio.DataAccess.Models;
using portfolio.DataAccess.Models.Db;
using portfolio.DataAccess.Models.Views;

namespace portfolio.DataAccess.Interfaces;

public interface ISimulationRepository
{
    void SaveSimulation(Simulation result);
    void UpdateSimulation(Simulation result);
    void SaveSimulationPeriod(SimulationPeriod period);
    Task<Session> SaveSession(DateOnly date);
    Task<List<string>> GetPortfolioHashesFromSessionDate(DateOnly date);
    Task<List<SessionCountView>> GetAllSessionsWithCounts();
    Task<Session?> GetLatestSession();
    Task<Session?> GetSessionByDate(DateOnly date);
    List<Simulation> GetSimulations(int limit);
    Task<List<Simulation>> GetSimulationsFromDate(DateTime date);
    Task<Simulation?> GetSimulation(Guid id);
    Task<SimulationView> GetLatestBestSimulation();
    Task<SimulationView?> GetBestSimulationByDate(DateOnly date); 
}