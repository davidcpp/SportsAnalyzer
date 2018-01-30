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

namespace SportsAnalyzer.Controllers
{
  public interface IXmlSoccerRequester
  {
    List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear);
    List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear);
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
  }

  public class FootballController : Controller
  {
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    public const string DefaultLeagueFullName = "Scottish Premier League";
    public const int DefaultSeasonYear = 2017;
    public const string DefaultLeagueShortName = "SPL";
    public const string DefaultLeagueId = "3";

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
    // mode - mode of table display: 0-normal, 1-expanded
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

    //public ActionResult Team(int seasonYear = defaultSeasonYear, string teamName = "")
    //{
    //}
  }
}