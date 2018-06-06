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

    public string AwayTeam { get; set; }
    public int? AwayGoals { get; set; }
    public string[] AwayGoalDetails { get; set; }
    public int? Round { get; set; }
    public string League { get; set; }
    public string HomeTeam { get; set; }
    public int? HomeGoals { get; set; }
    public string[] HomeGoalDetails { get; set; }

    /* Constructors */

    public FootballMatch() { }

    public FootballMatch(XMLSoccerCOM.Match match)
    {
      ConvertFromXmlSoccerMatch(match);
    }

    /* Methods */

    public void ConvertFromXmlSoccerMatch(XMLSoccerCOM.Match match)
    {
      AwayTeam = match.AwayTeam;
      AwayGoals = match.AwayGoals;
      AwayGoalDetails = match.AwayGoalDetails;
      HomeTeam = match.HomeTeam;
      HomeGoals = match.HomeGoals;
      HomeGoalDetails = match.HomeGoalDetails;
    }

    public bool IsEqualToXmlMatch(XMLSoccerCOM.Match team)
    {
      if (AwayTeam != team.AwayTeam)
        return false;
      if (AwayGoals != team.AwayGoals)
        return false;
      if (AwayGoalDetails != team.AwayGoalDetails)
        return false;
      if (HomeTeam != team.HomeTeam)
        return false;
      if (HomeGoals != team.HomeGoals)
        return false;
      if (HomeGoalDetails != team.HomeGoalDetails)
        return false;

      return true;
    }

  }
}