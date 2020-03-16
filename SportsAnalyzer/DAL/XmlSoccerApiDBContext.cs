namespace SportsAnalyzer.DAL
{
  using System;
  using System.Data.Entity;
  using SportsAnalyzer.Models;

  public interface IXmlSoccerApiDBContext : IDisposable
  {
    DbSet<FootballTeam> FootballTeams { get; }

    DbSet<TeamLeagueStanding> LeagueTable { get; }

    DbSet<FootballMatch> LeagueMatches { get; }

    int SaveChanges();
  }

  public class XmlSoccerApiDBContext : DbContext, IXmlSoccerApiDBContext
  {
    public XmlSoccerApiDBContext() : base("XmlSoccerApiDBContext")
    {
    }

    public DbSet<FootballTeam> FootballTeams { get; set; }

    public DbSet<TeamLeagueStanding> LeagueTable { get; set; }

    public DbSet<FootballMatch> LeagueMatches { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      Database.SetInitializer<XmlSoccerApiDBContext>(null);
      base.OnModelCreating(modelBuilder);
    }
  }
}