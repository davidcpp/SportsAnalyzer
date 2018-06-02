﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;
using System.Data.Entity;

namespace SportsAnalyzer.Controllers
{
  public interface IXmlSoccerRequester
  {
    List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(
      string league, int seasonStartYear);

    List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(
      string league, int seasonStartYear);

    List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(
      string league, int seasonStartYear);
  }

  public class XmlSoccerRequester : IXmlSoccerRequester
  {
    private const string apiKey = "AZRBAQTJUNSUUELVRATIYETSXZJREDNJQVMHENMHJOAVVAZKRC";
    private XMLSoccerCOM.Requester _xmlSoccerRequester = new XMLSoccerCOM.Requester(apiKey);

    public List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(
      string league, int seasonStartYear)
    {
      return _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonStartYear);
    }

    public List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(
      string league, int seasonStartYear)
    {
      return _xmlSoccerRequester.GetLeagueStandingsBySeason(league, seasonStartYear);
    }

    public List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(
      string league, int seasonStartYear)
    {
      return _xmlSoccerRequester
        .GetHistoricMatchesByLeagueAndSeason(league, seasonStartYear);
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

    public static DateTime lastUpdateTime;
    public static DateTime tableLastUpdateTime;
    public static DateTime teamsLastUpdateTime;

    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    private static List<XMLSoccerCOM.Match> xmlLeagueMatches;
    private static List<XMLSoccerCOM.TeamLeagueStanding> xmlLeagueStandings;
    private static List<XMLSoccerCOM.Team> xmlTeams;

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

    void ClearDBSet(DbSet dbList)
    {
      foreach (var dbItem in dbList)
      {
        dbList.Remove(dbItem);
      }
    }

    // GET: Football/Teams/{league}/{seasonYear}
    public ActionResult Teams(
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if (teamsLastUpdateTime == DateTime.MinValue
        || (teamsLastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
      {
        xmlTeams = _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonYear);
        ClearDBSet(db.FootballTeams);

        db.FootballTeams.AddRange(xmlTeams.ConvertToTeamList());
        db.SaveChanges();

        teamsLastUpdateTime = DateTime.UtcNow;
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

      if (tableLastUpdateTime == DateTime.MinValue
        || (tableLastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
      {
        xmlLeagueStandings = _xmlSoccerRequester
          .GetLeagueStandingsBySeason(league, seasonYear);

        ClearDBSet(db.LeagueTable);

        db.LeagueTable.AddRange(xmlLeagueStandings.ConvertToLeagueStandingList());
        db.SaveChanges();

        tableLastUpdateTime = DateTime.UtcNow;
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
        if (lastUpdateTime == DateTime.MinValue
          || (lastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
        {
          xmlLeagueMatches = _xmlSoccerRequester
            .GetHistoricMatchesByLeagueAndSeason(league, seasonYear);

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
    public ActionResult Stats(
      [Bind(Include = "LeagueName, SeasonYear, RoundsNumbersInts")] Statistics model)
    {
      if (lastUpdateTime == DateTime.MinValue
        || (lastUpdateTime - DateTime.UtcNow).TotalMinutes > requestsBreakMinutes)
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