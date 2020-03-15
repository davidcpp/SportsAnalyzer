namespace SportsAnalyzer.Controllers
{
  using System.Linq;
  using System.Web.Http;
  using SportsAnalyzer.DAL;
  using SportsAnalyzer.Models;
  using static SportsAnalyzer.Common;

  public class StatsController : ApiController
  {
    private readonly XmlSoccerAPI_DBContext dbContext = new XmlSoccerAPI_DBContext();
    private readonly IXmlSoccerRequester xmlSoccerRequester;

    public StatsController()
    {
      xmlSoccerRequester = new XmlSoccerRequester();
    }

    public StatsController(IXmlSoccerRequester xmlSoccerRequester)
    {
      this.xmlSoccerRequester = xmlSoccerRequester;
    }

    [HttpPost]
    public IHttpActionResult GoalsIntervals(StatsRequestModel statsRequest)
    {
      CheckMatchesData(statsRequest);
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(dbContext.LeagueMatches.ToList());
      stats.SetRounds(statsRequest.Rounds.ToList());
      stats.CalculateGoalsInIntervals();

      return Ok(stats.GoalsInIntervalsPercent);
    }

    [HttpPost]
    public IHttpActionResult MatchGoals(StatsRequestModel statsRequest)
    {
      CheckMatchesData(statsRequest);
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(dbContext.LeagueMatches.ToList());
      stats.SetRounds(statsRequest.Rounds.ToList());
      stats.CalculateMatchGoals();

      return Ok(stats.MatchGoalsPct);
    }

    [HttpPost]
    public IHttpActionResult RoundPoints(StatsRequestModel statsRequest)
    {
      CheckMatchesData(statsRequest);
      var stats = new Statistics(statsRequest.SeasonYear,
        statsRequest.LeagueName,
        statsRequest.TeamName);
      stats.SetMatches(dbContext.LeagueMatches.ToList());
      stats.SetRoundsRange("first", "last");
      stats.CalculateRoundPoints();

      return Ok(stats.RoundPoints);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        dbContext.Dispose();
      }

      base.Dispose(disposing);
    }

    private void CheckMatchesData(StatsRequestModel statsRequest)
    {
      if (IsDataOutOfDate(MatchesLastUpdateTime))
      {
        UpdateMatchesData(statsRequest.LeagueName,
          statsRequest.SeasonYear,
          xmlSoccerRequester,
          dbContext);
        MatchesDataUpdated = true;
      }
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