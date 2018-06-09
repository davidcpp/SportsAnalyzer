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
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    /* Constructors */

    public FootballController()
    {
      _xmlSoccerRequester = new XmlSoccerRequester();
    }

    public FootballController(IXmlSoccerRequester xmlSoccerRequester)
    {
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

    // GET: Football/Teams/{league}/{seasonYear}
    public ActionResult Teams(
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if ((TeamsLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - TeamsLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
      {
        LastUpdateTime = DateTime.UtcNow;
        TeamsLastUpdateTime = LastUpdateTime;

        var xmlTeams = _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonYear);
        ClearDBSet(db.FootballTeams);

        db.FootballTeams.AddRange(xmlTeams.ConvertToTeamList());
        db.SaveChanges();
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

      if ((TableLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - TableLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
      {
        LastUpdateTime = DateTime.UtcNow;
        TableLastUpdateTime = LastUpdateTime;

        var xmlLeagueStandings = _xmlSoccerRequester
          .GetLeagueStandingsBySeason(league, seasonYear);

        ClearDBSet(db.LeagueTable);

        db.LeagueTable.AddRange(xmlLeagueStandings.ConvertToLeagueStandingList());
        db.SaveChanges();
      }

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
        if ((MatchesLastUpdateTime == DateTime.MinValue
          || (DateTime.UtcNow - MatchesLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
          && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
        {
          LastUpdateTime = DateTime.UtcNow;
          MatchesLastUpdateTime = LastUpdateTime;

          var xmlLeagueMatches = _xmlSoccerRequester
            .GetHistoricMatchesByLeagueAndSeason(league, seasonYear);

          ClearDBSet(db.LeagueMatches);

          db.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
          db.SaveChanges();
        }

        Statistics statistics = new Statistics(DefaultSeasonYear, DefaultLeagueFullName);
        statistics.SetMatches(db.LeagueMatches.ToList());
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
    public ActionResult Stats(
      [Bind(Include = "LeagueName, SeasonYear, RoundsNumbersInts")] Statistics model)
    {
      if ((MatchesLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - MatchesLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
      {
        LastUpdateTime = DateTime.UtcNow;
        MatchesLastUpdateTime = LastUpdateTime;

        var xmlLeagueMatches = _xmlSoccerRequester.
          GetHistoricMatchesByLeagueAndSeason(model.LeagueName, model.SeasonYear);

        ClearDBSet(db.LeagueMatches);

        db.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
        db.SaveChanges();
      }

      Statistics statistics = new Statistics(model.SeasonYear, model.LeagueName);
      statistics.SetMatches(db.LeagueMatches.ToList());
      statistics.SetRounds(model.RoundsNumbersInts);
      statistics.CalculateAll();

      statistics.CreateRoundsSelectList();
      return View(statistics);
    }
  }
}