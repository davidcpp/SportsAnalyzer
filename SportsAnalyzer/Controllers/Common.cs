namespace SportsAnalyzer
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Infrastructure;
  using System.Linq;
  using SportsAnalyzer.Models;

  using IXmlSocDB = SportsAnalyzer.DAL.IXmlSoccerApiDBContext;
  using IXmlSR = SportsAnalyzer.IXmlSoccerRequester;

  public interface IXmlSoccerRequester
  {
    List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(
      string league, int seasonStartYear);

    List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(
      string league, int seasonStartYear);

    List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(
      string league, int seasonStartYear);
  }

  public static class Common
  {
    /* Constant Fields*/

    public const string DefaultLeagueName = "*";
    public const string DefaultLeagueShortName = "SPL";
    public const string DefaultLeagueFullName = "Scottish Premier League";
    public const string DefaultLeagueId = "3";
    public const string DefaultTeamName = "*";

    public const int DefaultSeasonYear = 2019;
    public const int DefaultRoundsNumber = 33;
    public const int UpdateDataBreakMinutes = 5;
    public const int RequestsBreakSeconds = 15;

    public const double DefaultMatchTime = 90.0;
    public const double DefaultNumberOfMatchIntervals = 6.0;

    /* Fields */

    public static DateTime LastUpdateTime;
    public static DateTime MatchesLastUpdateTime;
    public static DateTime TableLastUpdateTime;
    public static DateTime TeamsLastUpdateTime;

    public static bool MatchesDataUpdated = false;

    public static void CalcStats(this Statistics statistics, IXmlSocDB xmlSocDb, string startRound, string endRound)
    {
      statistics.SetMatches(xmlSocDb.LeagueMatches.ToList());
      statistics.SetRoundsRange(startRound, endRound);
      statistics.CalculateBasicStats();
      statistics.CalculateGoalsInIntervals();
      statistics.CalculateMatchGoals();
    }

    public static void ClearDbSet<T>(DbSet<T> set) where T : class
    {
      foreach (var item in set)
      {
        set.Remove(item);
      }
    }

    public static bool IsDataOutOfDate(DateTime dataLastUpdateTime)
    {
      return dataLastUpdateTime == DateTime.MinValue
        || ((DateTime.UtcNow - dataLastUpdateTime).TotalMinutes > UpdateDataBreakMinutes
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds);
    }

    public static void UpdateTableData(string league, int seasonYear, IXmlSR xmlSocReq, IXmlSocDB xmlSocDb)
    {
      LastUpdateTime = DateTime.UtcNow;
      TableLastUpdateTime = LastUpdateTime;

      var xmlLeagueStandings = xmlSocReq.GetLeagueStandingsBySeason(league, seasonYear);

      ClearDbSet(xmlSocDb.LeagueTable);
      xmlSocDb.LeagueTable.AddRange(xmlLeagueStandings.ConvertToLeagueStandingList());
      SaveChangesInDatabase(xmlSocDb);
    }

    public static void UpdateTeamsData(string league, int seasonYear, IXmlSR xmlSocReq, IXmlSocDB xmlSocDb)
    {
      LastUpdateTime = DateTime.UtcNow;
      TeamsLastUpdateTime = LastUpdateTime;

      var xmlTeams = xmlSocReq.GetAllTeamsByLeagueAndSeason(league, seasonYear);

      ClearDbSet(xmlSocDb.FootballTeams);
      xmlSocDb.FootballTeams.AddRange(xmlTeams.ConvertToTeamList());
      SaveChangesInDatabase(xmlSocDb);
    }

    public static void UpdateMatchesData(string leagueName, int seasonYear, IXmlSR xmlSocReq, IXmlSocDB xmlSocDb)
    {
      MatchesDataUpdated = true;
      LastUpdateTime = DateTime.UtcNow;
      MatchesLastUpdateTime = LastUpdateTime;

      var xmlLeagueMatches = xmlSocReq.GetHistoricMatchesByLeagueAndSeason(leagueName, seasonYear);

      ClearDbSet(xmlSocDb.LeagueMatches);
      xmlSocDb.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
      SaveChangesInDatabase(xmlSocDb);
    }

    private static void SaveChangesInDatabase(IXmlSocDB dbContext)
    {
      bool saveFailed;
      do
      {
        saveFailed = false;
        try
        {
          dbContext.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
          saveFailed = true;
          ex.Entries.Single().Reload();
        }
      }
      while (saveFailed);
    }
  }

  public class XmlSoccerRequester : IXmlSoccerRequester
  {
    private const string ApiKey = "AZRBAQTJUNSUUELVRATIYETSXZJREDNJQVMHENMHJOAVVAZKRC";
    private readonly XMLSoccerCOM.Requester xmlSoccerRequester = new XMLSoccerCOM.Requester(ApiKey);

    public List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear)
    {
      return xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonStartYear);
    }

    public List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear)
    {
      return xmlSoccerRequester.GetLeagueStandingsBySeason(league, seasonStartYear);
    }

    public List<XMLSoccerCOM.Match> GetHistoricMatchesByLeagueAndSeason(string league, int seasonStartYear)
    {
      return xmlSoccerRequester
        .GetHistoricMatchesByLeagueAndSeason(league, seasonStartYear);
    }
  }
}