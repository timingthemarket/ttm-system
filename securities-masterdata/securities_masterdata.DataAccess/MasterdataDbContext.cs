using Microsoft.EntityFrameworkCore;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Entities.Composite;
using Index = securities_masterdata.DataAccess.Entities.Index;

namespace securities_masterdata.DataAccess;

public class MasterdataDbContext : DbContext
{
    public MasterdataDbContext(DbContextOptions<MasterdataDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SecurityPrice> SecuritiesPrices { get; set; }
    public virtual DbSet<Security> Securities { get; set; }
    public virtual DbSet<Market> Markets { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }
    public virtual DbSet<Indicator> Indicators { get; set; }
    
    public virtual DbSet<Index> Indexes { get; set; }
    public virtual DbSet<IndexValue> IndexValues { get; set; }
    public virtual DbSet<IndexSecurity> IndexSecurities { get; set; }
    
    // Composite 
    public virtual DbSet<AverageVolume> AverageVolume { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Security>(entity =>
        {
            entity.ToTable("securities");
            entity.HasKey(k => k.SecurityId);

            entity.HasMany(e => e.SecuritiesPrices)
                .WithOne(e => e.Security)
                .HasForeignKey(e => e.SecurityId);
            entity.HasOne(e => e.Market)
                .WithOne(e => e.Security)
                .HasForeignKey<Security>(e => e.MarketId);
            entity.HasOne(e => e.Currency)
                .WithOne(e => e.Security)
                .HasForeignKey<Security>(e => e.CurrencyId);
        });

        modelBuilder.Entity<SecurityPrice>(entity =>
        {
            entity.ToTable("securities_prices");
            entity.HasKey(k => new {k.SecurityId, k.Date});
        });

        modelBuilder.Entity<Market>(entity =>
        {
            entity.ToTable("markets");
            entity.HasKey(k => k.MarketId);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");
            entity.HasKey(k => k.CurrencyId);

            entity.HasMany(e => e.CurrencyRates)
                .WithOne(e => e.Currency)
                .HasForeignKey(e => e.CurrencyIdFrom);
        });
        
        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.ToTable("currency_rates");
            entity.HasKey(k => new { k.CurrencyIdFrom, k.CurrencyIdTo, k.Date });
        });
        
        modelBuilder.Entity<Indicator>(entity =>
        {
            entity.ToTable("indicators");
            entity.HasKey(k => new { k.IndicatorId, k.Date, k.SecurityId });
        });
        
        modelBuilder.Entity<Index>(entity =>
        {
            entity.ToTable("indexes");
            entity.HasKey(k => k.IndexId);

            entity.HasMany(e => e.IndexSecurities)
                .WithOne(e => e.Index)
                .HasForeignKey(e => e.IndexId);
            entity.HasMany(e => e.IndexValues)
                .WithOne(e => e.Index)
                .HasForeignKey(e => e.IndexId);
        });
        
        modelBuilder.Entity<IndexValue>(entity =>
        {
            entity.ToTable("index_values");
            entity.HasKey(k => new { k.IndexId, k.Date });
        });
        
        modelBuilder.Entity<IndexSecurity>(entity =>
        {
            entity.ToTable("index_securities");
            entity.HasKey(k => new { k.IndexId, k.SecurityId });

            entity.HasOne(e => e.Security)
                .WithOne(e => e.IndexSecurity)
                .HasForeignKey<IndexSecurity>(e => e.SecurityId);
        });

        modelBuilder.Entity<AverageVolume>().HasNoKey();
    }
}