﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;

namespace SportsAnalyzer.Controllers
{
  public class StatsController : ApiController
  {
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    private const int defaultSeasonYear = FootballController.DefaultSeasonYear;
    private const int requestsBreakMinutes = FootballController.requestsBreakMinutes;
    private const int requestsBreakSeconds = FootballController.requestsBreakSeconds;

    private const string defaultLeagueFullName = FootballController.DefaultLeagueFullName;
    private const string defaultLeagueShortName = FootballController.DefaultLeagueShortName;
    private const string defaultLeagueId = FootballController.DefaultLeagueId;

    private static DateTime lastUpdateTime = FootballController.lastUpdateTime;
    private static DateTime matchesLastUpdateTime = FootballController.matchesLastUpdateTime;

    private static List<XMLSoccerCOM.Match> xmlLeagueMatches = FootballController.xmlLeagueMatches;


    public StatsController()
    {
      _xmlSoccerRequester = new XmlSoccerRequester();
    }

    public StatsController(IXmlSoccerRequester xmlSoccerRequester)
    {
      _xmlSoccerRequester = xmlSoccerRequester;
    }

    [HttpGet]
    public IHttpActionResult GetDataset(
      string teamName,
      string startRound = "1",
      string endRound = "last",
      string league = defaultLeagueFullName,
      int seasonYear = defaultSeasonYear)
    {
      if (league == defaultLeagueShortName || league == defaultLeagueId)
        league = defaultLeagueFullName;

      if ((matchesLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - matchesLastUpdateTime).TotalMinutes > requestsBreakMinutes)
        && (DateTime.UtcNow - lastUpdateTime).TotalSeconds > requestsBreakSeconds)
      {
        // TODO: Extract the method from the current condition code
        lastUpdateTime = DateTime.UtcNow;
        matchesLastUpdateTime = lastUpdateTime;

        xmlLeagueMatches = _xmlSoccerRequester
          .GetHistoricMatchesByLeagueAndSeason(league, seasonYear);
      }

      Statistics statistics = new Statistics(seasonYear, league, teamName);
      statistics.SetMatches(xmlLeagueMatches);
      statistics.SetRoundsRange(startRound, endRound);
      statistics.CalculateAll();

      return Ok(statistics.GoalsInIntervalsPercent);
    }
  }
}
