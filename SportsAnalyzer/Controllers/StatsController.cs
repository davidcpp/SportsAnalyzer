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

    [HttpGet]
    public IHttpActionResult GetDataset(
      string teamName,
      string startRound = "1",
      string endRound = "last",
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear)
    {
      if (league == DefaultLeagueShortName || league == DefaultLeagueId)
        league = DefaultLeagueFullName;

      if ((MatchesLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - MatchesLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds)
      {
        // TODO: Extract the method from the current condition code
        LastUpdateTime = DateTime.UtcNow;
        MatchesLastUpdateTime = LastUpdateTime;

        var xmlLeagueMatches = _xmlSoccerRequester
          .GetHistoricMatchesByLeagueAndSeason(league, seasonYear);

        ClearDBSet(db.LeagueMatches);

        db.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
        db.SaveChanges();
      }

      Statistics statistics = new Statistics(seasonYear, league, teamName);
      statistics.SetMatches(db.LeagueMatches.ToList());
      statistics.SetRoundsRange(startRound, endRound);
      statistics.CalculateAll();

      return Ok(statistics.GoalsInIntervalsPercent);
    }
  }
}
