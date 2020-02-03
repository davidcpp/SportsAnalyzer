using System;
using System.Data.Entity;
using SportsAnalyzer.Models;

namespace SportsAnalyzer.DAL
{
  public class XmlSoccerAPI_DBContext : DbContext, IXmlSoccerAPI_DBContext
  {
    public DbSet<FootballTeam> FootballTeams { get; set; }
    public DbSet<TeamLeagueStanding> LeagueTable { get; set; }
    public DbSet<FootballMatch> LeagueMatches { get; set; }

    public XmlSoccerAPI_DBContext() : base("XmlSoccerAPI_DBContext")
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      Database.SetInitializer<XmlSoccerAPI_DBContext>(null);
      base.OnModelCreating(modelBuilder);
    }
  }

  public interface IXmlSoccerAPI_DBContext : IDisposable
  {
    DbSet<FootballTeam> FootballTeams { get; }
    DbSet<TeamLeagueStanding> LeagueTable { get; }
    DbSet<FootballMatch> LeagueMatches { get; }
    int SaveChanges();
  }
}