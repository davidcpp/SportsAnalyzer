using System;
using System.Collections.Generic;

namespace SportsAnalyzer.Models
{
  public static class FootballMatchConverter
  {
    public static List<FootballMatch> ConvertToMatchList(this List<XMLSoccerCOM.Match> xmlList)
    {
      var list = new List<FootballMatch>();
      foreach (var xmlMatch in xmlList)
      {
        if (xmlMatch != null)
          list.Add(new FootballMatch(xmlMatch));
      }
      return list;
    }
  }

  public class FootballMatch
  {
    public int Id { get; set; }

    public DateTime? Date { get; set; }
    public string AwayTeam { get; set; }
    public int? AwayGoals { get; set; }
    public string AwayGoalDetails { get; set; }
    public int? Round { get; set; }
    public string League { get; set; }
    public string HomeTeam { get; set; }
    public int? HomeGoals { get; set; }
    public string HomeGoalDetails { get; set; }

    /* Constructors */

    public FootballMatch() { }

    public FootballMatch(XMLSoccerCOM.Match match)
    {
      ConvertFromXmlSoccerMatch(match);
    }

    /* Methods */

    public void ConvertFromXmlSoccerMatch(XMLSoccerCOM.Match match)
    {
      if (match == null)
        return;

      Round = match.Round;
      AwayTeam = match.AwayTeam;
      AwayGoals = match.AwayGoals;
      AwayGoalDetails = match.AwayGoalDetails != null ?
        string.Join(";", match.AwayGoalDetails) : string.Empty;
      HomeTeam = match.HomeTeam;
      HomeGoals = match.HomeGoals;
      HomeGoalDetails = match.HomeGoalDetails != null ?
        string.Join(";", match.HomeGoalDetails) : string.Empty;
    }

    public bool IsEqualToXmlMatch(XMLSoccerCOM.Match match)
    {
      if (match == null)
        return false;

      if (Date != match.Date)
        return false;
      if (AwayTeam != match.AwayTeam)
        return false;
      if (AwayGoals != match.AwayGoals)
        return false;
      if (match.AwayGoalDetails == null)
      {
        if (AwayGoalDetails?.Length != 0 && AwayGoalDetails != null)
          return false;
      }
      else if (AwayGoalDetails != match.AwayGoalDetails.ToString())
      {
        return false;
      }
      if (HomeTeam != match.HomeTeam)
        return false;
      if (HomeGoals != match.HomeGoals)
        return false;
      if (match.HomeGoalDetails == null)
      {
        if (HomeGoalDetails?.Length != 0 && HomeGoalDetails != null)
          return false;
      }
      else if (HomeGoalDetails != match.HomeGoalDetails.ToString())
      {
        return false;
      }
      return true;
    }

  }
}