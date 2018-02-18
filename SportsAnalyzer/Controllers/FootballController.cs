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
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    public const string DefaultLeagueFullName = "Scottish Premier League";
    public const int DefaultSeasonYear = 2017;
    public const string DefaultLeagueShortName = "SPL";
    public const string DefaultLeagueId = "3";
    public const int DefaultNumberOfSeasonPhases = 3;
    public const int DefaultNumberOfTeams = 12;

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

    public ActionResult Stats(
      string startRound = "1",
      string endRound = "last",
      string teamName = "",
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (String.IsNullOrEmpty(teamName))
      {
        // TODO: Enter a min. 15 seconds break between two requests - it's required by the provider
        // However, Exception hasn't occur so far
        List<XMLSoccerCOM.Match> xmlLeagueMatches =
          _xmlSoccerRequester.GetHistoricMatchesByLeagueAndSeason(league, seasonYear);

        List<XMLSoccerCOM.Team> xmlTeams =
          _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonYear);

        xmlLeagueMatches = GetXmlMatchesByRoundsRange(startRound, endRound, xmlLeagueMatches, xmlTeams.Count);

        Statistics statistics = new Statistics(DefaultLeagueFullName);
        statistics.CalculateAll(xmlLeagueMatches);

        return View(new List<Statistics> { statistics });
      }
      return View();

    }
    // move this function to Statistics class as its private or protected method and
    // add method to calculate statistics from several matches
    private static List<XMLSoccerCOM.Match> GetXmlMatchesByRoundsRange(string startRound, 
                                                                      string endRound, 
                                                                      List<XMLSoccerCOM.Match> xmlLeagueMatches, 
                                                                      int numberOfTeams)
    {
      if (!int.TryParse(startRound, out int startRoundInt))
        startRoundInt = 1;

      var startRoundMatches =
        xmlLeagueMatches.Where(m => m.Round == startRoundInt)
                        .Select((x, index) => index);

      int startRoundFirstMatchIndex = 0;
      if (startRoundMatches != null && startRoundMatches.Any())
        startRoundFirstMatchIndex = startRoundMatches.ElementAt(0);

      // TODO: Is it good to use Lazy generic type to do evaluation of the following expression?
      if (!int.TryParse(endRound, out int endRoundInt))
        endRoundInt = (numberOfTeams - 1) * DefaultNumberOfSeasonPhases;

      var endRoundMatches =
        xmlLeagueMatches.Where(m => m.Round == endRoundInt)
                        .Select((x, index) => index);

      int endRoundLastMatchIndex = xmlLeagueMatches.Count - 1;
      if (endRoundMatches != null && endRoundMatches.Any())
        endRoundLastMatchIndex = endRoundMatches.ToArray().Last();

      xmlLeagueMatches = xmlLeagueMatches.GetRange(startRoundFirstMatchIndex,
                                                  (endRoundLastMatchIndex - startRoundFirstMatchIndex) + 1);
      return xmlLeagueMatches;
    }
  }
}