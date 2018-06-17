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

      if ((MatchesLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - MatchesLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
      {
        // TODO: Extract the method from the current condition code
        LastUpdateTime = DateTime.UtcNow;
        MatchesLastUpdateTime = LastUpdateTime;

        var xmlLeagueMatches = _xmlSoccerRequester
          .GetHistoricMatchesByLeagueAndSeason(stats.LeagueName, stats.SeasonYear);

        ClearDBSet(db.LeagueMatches);

        db.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
        db.SaveChanges();
      }
      stats.SetMatches(db.LeagueMatches.ToList());
      stats.SetRounds(statsRequest.Rounds.ToList());
      stats.CalculateAll();

      return Ok(stats.GoalsInIntervalsPercent);
    }

    public class StatsRequestModel
    {
      public int[] Rounds { get; set; }
      public string TeamName { get; set; }
    }
  }
}