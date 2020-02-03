using SportsAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace SportsAnalyzer
{
  using IXmlSR = SportsAnalyzer.IXmlSoccerRequester;
  using IXmlSocDB = SportsAnalyzer.DAL.IXmlSoccerAPI_DBContext;

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
    public const int RequestsBreakMinutes = 5;
    public const int RequestsBreakSeconds = 15;

    public const double DefaultMatchTime = 90.0;
    public const double DefaultNumberOfMatchIntervals = 6.0;

    /* Fields */

    public static DateTime LastUpdateTime;
    public static DateTime MatchesLastUpdateTime;
    public static DateTime TableLastUpdateTime;
    public static DateTime TeamsLastUpdateTime;

    public static void ClearDBSet<T>(DbSet<T> dbList) where T : class
    {
      foreach (var dbItem in dbList)
      {
        dbList.Remove(dbItem);
      }
    }

    public static bool IsDataOutOfDate(DateTime dataLastUpdateTime)
    {
      return (dataLastUpdateTime == DateTime.MinValue
        || (DateTime.UtcNow - dataLastUpdateTime).TotalMinutes > RequestsBreakMinutes)
        && (DateTime.UtcNow - LastUpdateTime).TotalSeconds > RequestsBreakSeconds;
    }

    public static void RefreshTeamsData(string league,
      int seasonYear,
      IXmlSR xmlSocReq,
      IXmlSocDB xmlSocDB)
    {
      LastUpdateTime = DateTime.UtcNow;
      TeamsLastUpdateTime = LastUpdateTime;

      var xmlTeams = xmlSocReq.GetAllTeamsByLeagueAndSeason(league, seasonYear);
      ClearDBSet(xmlSocDB.FootballTeams);

      xmlSocDB.FootballTeams.AddRange(xmlTeams.ConvertToTeamList());
      SaveChangesInDatabase(xmlSocDB);
    }

    public static void RefreshTableData(string league,
      int seasonYear,
      IXmlSR xmlSocReq,
      IXmlSocDB xmlSocDB)
    {
      LastUpdateTime = DateTime.UtcNow;
      TableLastUpdateTime = LastUpdateTime;

      var xmlLeagueStandings = xmlSocReq
        .GetLeagueStandingsBySeason(league, seasonYear);

      ClearDBSet(xmlSocDB.LeagueTable);

      xmlSocDB.LeagueTable.AddRange(xmlLeagueStandings.ConvertToLeagueStandingList());
      SaveChangesInDatabase(xmlSocDB);
    }

    public static void RefreshMatchesData(string leagueName,
      int seasonYear,
      IXmlSR xmlSocReq,
      IXmlSocDB xmlSocDB)
    {
      LastUpdateTime = DateTime.UtcNow;
      MatchesLastUpdateTime = LastUpdateTime;

      var xmlLeagueMatches = xmlSocReq.
        GetHistoricMatchesByLeagueAndSeason(leagueName, seasonYear);

      ClearDBSet(xmlSocDB.LeagueMatches);

      xmlSocDB.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());

      SaveChangesInDatabase(xmlSocDB);
    }

    private static void SaveChangesInDatabase(IXmlSocDB db)
    {
      bool saveFailed;
      do
      {
        saveFailed = false;
        try
        {
          db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
          saveFailed = true;
          ex.Entries.Single().Reload();
        }
      } while (saveFailed);
    }

    public static void CalcStats(this Statistics statistics,
      IXmlSocDB xmlSocDB,
      string startRound,
      string endRound)
    {
      statistics.SetMatches(xmlSocDB.LeagueMatches.ToList());
      statistics.SetRoundsRange(startRound, endRound);
      statistics.CalculateBasicStats();
      statistics.CalculateGoalsInIntervals();
    }
  }
}