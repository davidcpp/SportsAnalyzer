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
      Date = match.Date;
      Round = match.Round;
      AwayTeam = match.AwayTeam;
      AwayGoals = match.AwayGoals;
      AwayGoalDetails = string.Join(";", match.AwayGoalDetails);
      HomeTeam = match.HomeTeam;
      HomeGoals = match.HomeGoals;
      HomeGoalDetails = string.Join(";", match.HomeGoalDetails);
    }

    public bool IsEqualToXmlMatch(XMLSoccerCOM.Match match)
    {
      if (Date != match.Date)
        return false;
      if (AwayTeam != match.AwayTeam)
        return false;
      if (AwayGoals != match.AwayGoals)
        return false;
      if (AwayGoalDetails.Equals(match.AwayGoalDetails.ToString()))
        return false;
      if (HomeTeam != match.HomeTeam)
        return false;
      if (HomeGoals != match.HomeGoals)
        return false;
      if (HomeGoalDetails.Equals(match.HomeGoalDetails.ToString()))
        return false;

      return true;
    }

  }
}