using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;
using static SportsAnalyzer.Common;

namespace SportsAnalyzer.Controllers
{
  public class FootballController : Controller
  {
    private IXmlSoccerAPI_DBContext db;

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    /* Constructors */

    public FootballController()
    {
      _xmlSoccerRequester = new XmlSoccerRequester();
      db = new XmlSoccerAPI_DBContext();
    }

    public FootballController(IXmlSoccerRequester xmlSoccerRequester,
      IXmlSoccerAPI_DBContext dbContext = null)
    {
      _xmlSoccerRequester = xmlSoccerRequester;
      db = dbContext ?? new XmlSoccerAPI_DBContext();
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

    // GET: Football/Teams/{league}/{seasonYear}
    public ActionResult Teams(
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if (IsDataOutOfDate(TeamsLastUpdateTime))
      {
        RefreshTeamsData(league, seasonYear, _xmlSoccerRequester, db);
      }

      ViewBag.EmptyList = "List of teams is empty";
      if (db.FootballTeams.Count() == 0)
        ViewBag.Message = ViewBag.EmptyList;

      return View(db.FootballTeams.ToList());
    }

    // GET: Football/Table/{league}/{seasonYear}
    public ActionResult Table(
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if (IsDataOutOfDate(TableLastUpdateTime))
      {
        RefreshTableData(league, seasonYear, _xmlSoccerRequester, db);
      }

      ViewBag.EmptyList = "League Table is empty";
      if (db.LeagueTable.Count() == 0)
        ViewBag.Message = ViewBag.EmptyList;

      return View(db.LeagueTable.ToList());
    }

    // GET: Football/Stats/{startRound}/{endRound}/{league}/{seasonYear}
    public ActionResult Stats(
      string startRound = "1",
      string endRound = "last",
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        RefreshMatchesData(league, seasonYear, _xmlSoccerRequester, db);
      }
      var stats = new Statistics(seasonYear, league);
      stats.CalcStats(db, startRound, endRound);
      stats.CreateTeamsSelectList();
      stats.CreateRoundsSelectList();
      return View(stats);
    }
  }
}