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

    [HttpPost]
    public IHttpActionResult GetDataset(StatsRequestModel statsRequest)
    {
      Statistics stats = new Statistics(teamName: statsRequest.TeamName);

      if (stats.LeagueName == DefaultLeagueShortName || stats.LeagueName == DefaultLeagueId)
        stats.LeagueName = DefaultLeagueFullName;

      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        RefreshMatchesData(stats.LeagueName, stats.SeasonYear, _xmlSoccerRequester, db);
      }
      CalcStatsForRounds(stats, db, statsRequest.Rounds.ToList());

      return Ok(stats.GoalsInIntervalsPercent);
    }

    public class StatsRequestModel
    {
      public int[] Rounds { get; set; }
      public string TeamName { get; set; }
    }
  }
}