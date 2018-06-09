﻿using SportsAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SportsAnalyzer
{
  using IXmlSR = SportsAnalyzer.IXmlSoccerRequester;
  using XmlSocDB = SportsAnalyzer.DAL.XmlSoccerAPI_DBContext;

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


    public const int DefaultSeasonYear = 2017;
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

    public static void ClearDBSet(DbSet dbList)
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

    public static void RefreshMatchesData(string leagueName,
      int seasonYear,
      IXmlSR xmlSocReq,
      XmlSocDB xmlSocDB)
    {
      LastUpdateTime = DateTime.UtcNow;
      MatchesLastUpdateTime = LastUpdateTime;

      var xmlLeagueMatches = xmlSocReq.
        GetHistoricMatchesByLeagueAndSeason(leagueName, seasonYear);

      ClearDBSet(xmlSocDB.LeagueMatches);

      xmlSocDB.LeagueMatches.AddRange(xmlLeagueMatches.ConvertToMatchList());
      xmlSocDB.SaveChanges();
    }

    public static Statistics CalcStats(string leagueName,
      int seasonYear,
      string startRound,
      string endRound,
      XmlSocDB xmlSocDB)
    {
      var statistics = new Statistics(seasonYear, leagueName);
      statistics.SetMatches(xmlSocDB.LeagueMatches.ToList());
      statistics.SetRoundsRange(startRound, endRound);
      statistics.CalculateAll();
      statistics.CreateRoundsSelectList();
      return statistics;
    }

    public static Statistics CalcStatsForRounds(Statistics model, XmlSocDB xmlSocDB)
    {
      var statistics = new Statistics(model.SeasonYear, model.LeagueName);
      statistics.SetMatches(xmlSocDB.LeagueMatches.ToList());
      statistics.SetRounds(model.RoundsNumbersInts);
      statistics.CalculateAll();
      statistics.CreateRoundsSelectList();
      return statistics;
    }
  }
}