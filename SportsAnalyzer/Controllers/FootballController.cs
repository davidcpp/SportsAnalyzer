using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;
using System.Net;
using System.Diagnostics;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Threading;

namespace SportsAnalyzer.Controllers
{
  public interface IXmlSoccerRequester
  {
    List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear);
    List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear);
    List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(string league, int seasonStartYear);
  }

  public class XmlSoccerRequester : IXmlSoccerRequester
  {
    private const string apiKey = "AZRBAQTJUNSUUELVRATIYETSXZJREDNJQVMHENMHJOAVVAZKRC";
    private XMLSoccerCOM.Requester _xmlSoccerRequester = new XMLSoccerCOM.Requester(apiKey);

    public List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear)
    {
      Debug.Write("GetAllTeamsByLeagueAndSeason()\n");
      return _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonStartYear);
    }
    public List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear)
    {
      Debug.Write("GetLeagueStandingsBySeason()\n");
      return _xmlSoccerRequester.GetLeagueStandingsBySeason(league, seasonStartYear);
    }
    public List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(string league, int seasonStartYear)
    {
      Debug.Write("GetHistoricMatchesByLeagueAndSeason()\n");
      return _xmlSoccerRequester.GetHistoricMatchesByLeagueAndSeason(league, seasonStartYear);
    }
  }

  public class FootballController : Controller
  {
    /* Constant Fields*/

    public const string DefaultLeagueFullName = "Scottish Premier League";
    public const int DefaultSeasonYear = 2017;
    public const string DefaultLeagueShortName = "SPL";
    public const string DefaultLeagueId = "3";
    public const int DefaultRoundsNumber = 33;

    private const int requestsBreakMinutes = 5;
    /* Fields */

    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;
    private static List<XMLSoccerCOM.Match> xmlLeagueMatches;
    private static DateTime lastUpdateTime;

    /* Constructors */

    public FootballController()
    {
      Trace.Write("FootballController()\n");
      _xmlSoccerRequester = new XmlSoccerRequester();
    }

    public FootballController(IXmlSoccerRequester xmlSoccerRequester)
    {
      Trace.Write("FootballController(IXmlSoccerRequester)\n");
      _xmlSoccerRequester = xmlSoccerRequester;
    }

    /* Methods */

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }

    // GET: Football
    public ActionResult Index()
    {
      ViewBag.Title = "Scottish Premier League main page.";

      return View(db.FootballTeams.ToList());
    }

    void ClearDBSet(DbSet dbList)
    {
      foreach (var dbItem in dbList)
      {
        dbList.Remove(dbItem);
      }
    }

    // GET: Football/Teams/{league}/{seasonYear}
    public ActionResult Teams(string league = DefaultLeagueFullName, int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;
      var XmlTeams = _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonYear);

      ClearDBSet(db.FootballTeams);

      db.FootballTeams.AddRange(XmlTeams.ConvertToTeamList());
      db.SaveChanges();

      ViewBag.EmptyList = "List of teams is empty";
      if (db.FootballTeams.Count() == 0)
        ViewBag.Message = ViewBag.EmptyList;

      return View(db.FootballTeams.ToList());
    }

    // GET: Football/Table/{league}/{seasonYear}
    public ActionResult Table(string league = DefaultLeagueFullName, int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      var XmlLeagueStandings = _xmlSoccerRequester.GetLeagueStandingsBySeason(league, seasonYear);

      ClearDBSet(db.LeagueTable);

      db.LeagueTable.AddRange(XmlLeagueStandings.ConvertToLeagueStandingList());
      db.SaveChanges();

      ViewBag.EmptyList = "League Table is empty";
      if (db.LeagueTable.Count() == 0)
        ViewBag.Message = ViewBag.EmptyList;

      return View(db.LeagueTable.ToList());
    }

    // GET: Football/Stats/{startRound}/{endRound}/{teamName}/{league}/{seasonYear}
    public ActionResult Stats(
      string startRound = "1",
      string endRound = "last",
      string teamName = "",
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (String.IsNullOrEmpty(teamName))
      {
        if (lastUpdateTime == DateTime.MinValue ||
          (lastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
        {
          xmlLeagueMatches = _xmlSoccerRequester.GetHistoricMatchesByLeagueAndSeason(league, seasonYear);
          lastUpdateTime = DateTime.UtcNow;
        }

        Statistics statistics = new Statistics(DefaultSeasonYear, DefaultLeagueFullName);
        statistics.SetMatches(xmlLeagueMatches);
        statistics.SetRoundsRange(startRound, endRound);
        statistics.CalculateAll();

        statistics.CreateRoundsSelectList();
        return View(statistics);
      }
      return View();
    }

    // Action for Multiselect list form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Stats([Bind(Include = "LeagueName, SeasonYear, RoundsNumbersInts")] Statistics model)
    {
      if (lastUpdateTime == DateTime.MinValue ||
        (lastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
      {
        xmlLeagueMatches = _xmlSoccerRequester.
          GetHistoricMatchesByLeagueAndSeason(model.LeagueName, model.SeasonYear);
        lastUpdateTime = DateTime.UtcNow;
      }
      Statistics statistics = new Statistics(model.SeasonYear, model.LeagueName);
      statistics.SetMatches(xmlLeagueMatches);
      statistics.SetRounds(model.RoundsNumbersInts);
      statistics.CalculateAll();

      statistics.CreateRoundsSelectList();
      return View(statistics);
    }
  }
}