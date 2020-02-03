using System;
using System.Collections.Generic;
using System.Web.Http;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;
using System.Linq;
using static SportsAnalyzer.Common;

namespace SportsAnalyzer.Controllers
{
  public class StatsController : ApiController
  {
    private readonly XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    public StatsController()
    {
      _xmlSoccerRequester = new XmlSoccerRequester();
    }

    public StatsController(IXmlSoccerRequester xmlSoccerRequester)
    {
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

    [HttpPost]
    public IHttpActionResult GoalsIntervals(StatsRequestModel statsRequest)
    {
      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        RefreshMatchesData(statsRequest.LeagueName,
          statsRequest.SeasonYear,
          _xmlSoccerRequester,
          db);
      }
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(db.LeagueMatches.ToList());
      stats.SetRounds(statsRequest.Rounds.ToList());
      stats.CalculateGoalsInIntervals();

      return Ok(stats.GoalsInIntervalsPercent);
    }

    [HttpPost]
    public IHttpActionResult MatchGoals(StatsRequestModel statsRequest)
    {
      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        RefreshMatchesData(statsRequest.LeagueName,
          statsRequest.SeasonYear,
          _xmlSoccerRequester,
          db);
      }
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(db.LeagueMatches.ToList());
      stats.SetRounds(statsRequest.Rounds.ToList());
      stats.CalculateMatchGoals();

      return Ok(stats.MatchGoalsPct);
    }

    [HttpPost]
    public IHttpActionResult RoundPoints(StatsRequestModel statsRequest)
    {
      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        RefreshMatchesData(statsRequest.LeagueName,
          statsRequest.SeasonYear,
          _xmlSoccerRequester,
          db);
      }
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(db.LeagueMatches.ToList());
      stats.SetRoundsRange("first", "last");
      stats.CalculateRoundPoints();

      return Ok(stats.RoundPoints);
    }

    public class StatsRequestModel
    {
      public int[] Rounds { get; set; }
      public string TeamName { get; set; }
      public string LeagueName { get; set; }
      public int SeasonYear { get; set; }
    }
  }
}