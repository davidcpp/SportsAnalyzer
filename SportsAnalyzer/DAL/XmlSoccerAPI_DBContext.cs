using System.Data.Entity;
using SportsAnalyzer.Models;

namespace SportsAnalyzer.DAL
{
  public class XmlSoccerAPI_DBContext : DbContext
  {
    public XmlSoccerAPI_DBContext() : base("XmlSoccerAPI_DBContext")
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      Database.SetInitializer<XmlSoccerAPI_DBContext>(null);
      base.OnModelCreating(modelBuilder);
    }

    public DbSet<FootballTeam> FootballTeams { get; set; }
    public DbSet<TeamLeagueStanding> LeagueTable { get; set; }
  }
}