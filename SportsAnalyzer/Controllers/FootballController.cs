namespace SportsAnalyzer.Controllers
{
  using System.Linq;
  using System.Web.Mvc;
  using SportsAnalyzer.DAL;
  using SportsAnalyzer.Models;
  using static SportsAnalyzer.Common;

  public class FootballController : Controller
  {
    private readonly IXmlSoccerAPI_DBContext dbContext;
    private readonly IXmlSoccerRequester xmlSoccerRequester;

    /* Constructors */

    public FootballController()
    {
      xmlSoccerRequester = new XmlSoccerRequester();
      dbContext = new XmlSoccerAPI_DBContext();
    }

    public FootballController(IXmlSoccerRequester xmlSoccerRequester, IXmlSoccerAPI_DBContext dbContext = null)
    {
      this.xmlSoccerRequester = xmlSoccerRequester;
      this.dbContext = dbContext ?? new XmlSoccerAPI_DBContext();
    }

    /* Methods */

    // GET: Football
    public ActionResult Index()
    {
      ViewBag.Title = "Scottish Premier League main page.";

      return View(dbContext.FootballTeams.ToList());
    }

    // GET: Football/Stats/{startRound}/{endRound}/{league}/{seasonYear}
    public ActionResult Stats(
      string startRound = "1",
      string endRound = "last",
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
      {
        league = DefaultLeagueFullName;
      }

      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        UpdateMatchesData(league, seasonYear, xmlSoccerRequester, dbContext);
      }

      var stats = new Statistics(seasonYear, league);
      stats.CalcStats(dbContext, startRound, endRound);
      stats.CreateTeamsSelectList();
      stats.CreateRoundsSelectList();
      return View(stats);
    }

    // GET: Football/Table/{league}/{seasonYear}
    public ActionResult Table(string league = DefaultLeagueFullName, int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
      {
        league = DefaultLeagueFullName;
      }

      if (IsDataOutOfDate(TableLastUpdateTime))
      {
        UpdateTableData(league, seasonYear, xmlSoccerRequester, dbContext);
      }

      ViewBag.EmptyList = "League Table is empty";
      if (dbContext.LeagueTable.Count() == 0)
      {
        ViewBag.Message = ViewBag.EmptyList;
      }

      return View(dbContext.LeagueTable.ToList());
    }

    // GET: Football/Teams/{league}/{seasonYear}
    public ActionResult Teams(string league = DefaultLeagueFullName, int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
      {
        league = DefaultLeagueFullName;
      }

      if (IsDataOutOfDate(TeamsLastUpdateTime))
      {
        UpdateTeamsData(league, seasonYear, xmlSoccerRequester, dbContext);
      }

      ViewBag.EmptyList = "List of teams is empty";
      if (dbContext.FootballTeams.Count() == 0)
      {
        ViewBag.Message = ViewBag.EmptyList;
      }

      return View(dbContext.FootballTeams.ToList());
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        dbContext.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}