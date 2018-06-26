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
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

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
      Statistics stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.CalcStatsForRounds(db, statsRequest.Rounds.ToList());

      return Ok(stats.GoalsInIntervalsPercent);
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