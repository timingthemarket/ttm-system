using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess.Constants;
using portfolio.DataAccess.Models.Db;
using portfolio.DataAccess.Models.Views;

namespace portfolio.DataAccess;

public class PortfolioDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(Configuration.DbConString, options =>
        {
            options.CommandTimeout(30); // 30 seconds command timeout
        });


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SimulationPeriod>(entity =>
        {
            entity.ToTable("simulation_period");
            entity.HasKey(k => k.Id);
            
            entity.HasOne(e => e.Portfolio)
                .WithOne(e => e.SimulationPeriod)
                .HasForeignKey<Portfolio>(e => e.Id);
        });

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.ToTable("portfolio");
            entity.HasKey(k => k.Id);

            entity.HasOne(e => e.SimulationPeriod)
                .WithOne(e => e.Portfolio)
                .HasForeignKey<SimulationPeriod>(e => e.PortfolioId);
        });

        modelBuilder.Entity<PortfolioValue>(entity =>
        {
            entity.ToTable("portfolio_value");
            entity.HasKey(k => k.Id);

            entity.HasOne(e => e.Portfolio)
                .WithMany(e => e.PortfolioValues)
                .HasForeignKey(e => e.PortfolioId);
        });
        
        modelBuilder.Entity<PortfolioIndicator>(entity =>
        {
            entity.ToTable("portfolio_indicators");
            entity.HasKey(k => new { k.PortfolioId, k.Indicator });

            entity.HasOne(e => e.Portfolio)
                .WithMany(e => e.PortfolioIndicators)
                .HasForeignKey(e => e.PortfolioId);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("session");
            entity.HasKey(k => k.Id);
        });

        modelBuilder.Entity<Simulation>(entity =>
        {
            entity.ToTable("simulation");
            entity.HasKey(k => k.Id);

            entity.HasOne(e => e.Session)
                .WithMany(e => e.Simulations)
                .HasForeignKey(e => e.SessionId);
        });
        
        modelBuilder.Entity<PortfolioOutcomeView>(entity =>
        {
            entity.ToView("portfolio_outcome_view");
            entity.HasKey(k => k.PortfolioId);
        });
    }

    public virtual DbSet<Portfolio> Portfolios { get; set; }
    public virtual DbSet<PortfolioIndicator> PortfolioIndicators { get; set; }
    public virtual DbSet<PortfolioTrends> PortfolioTrends { get; set; }
    public virtual DbSet<PortfolioValue> PortfolioValues { get; set; }
    public virtual DbSet<Simulation> Simulations { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }
    public virtual DbSet<SimulationPeriod> SimulationPeriod { get; set; }
    // Views
    public virtual DbSet<SimulationView> SimulationView { get; set; }
    public virtual DbSet<PortfolioOutcomeView> PortfolioOutcomeView { get; set; }

}