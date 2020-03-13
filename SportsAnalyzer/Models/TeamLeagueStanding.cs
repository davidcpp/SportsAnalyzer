namespace SportsAnalyzer.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public static class TeamLeagueStandingConverter
  {
    public static List<TeamLeagueStanding> ConvertToLeagueStandingList(
      this List<XMLSoccerCOM.TeamLeagueStanding> xmlList)
    {
      var list = new List<TeamLeagueStanding>();
      foreach (var xmlTeamLeagueStanding in xmlList)
      {
        if (xmlTeamLeagueStanding != null)
          list.Add(new TeamLeagueStanding(xmlTeamLeagueStanding));
      }
      return list;
    }
  }

  public class TeamLeagueStanding
  {
    /* Constant fields */

    public const int MaxTeamNameLength = 80;

    /* Constructors */

    public TeamLeagueStanding() { }

    public TeamLeagueStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
      ConvertFromXmlTeamStanding(teamStanding);
    }

    /* Properties */

    public int Id { get; set; }

    [Required]
    [Display(Name = "Team Id")]
    public int Team_Id { get; set; }

    [Required]
    [Display(Name = "Team Name")]
    [StringLength(MaxTeamNameLength, MinimumLength = 2)]
    public string Team { get; set; }

    [Required]
    [Display(Name = "Played")]
    public int Played { get; set; }

    [Required]
    [Display(Name = "Points")]
    public int Points { get; set; }

    [Required]
    [Display(Name = "Won")]
    public int Won { get; set; }

    [Required]
    [Display(Name = "Drawn")]
    public int Draw { get; set; }

    [Required]
    [Display(Name = "Lost")]
    public int Lost { get; set; }

    [Required]
    [Display(Name = "Goals For")]
    public int GoalsFor { get; set; }

    [Required]
    [Display(Name = "Goals Against")]
    public int GoalsAgainst { get; set; }

    [Display(Name = "Goals Difference")]
    public int GoalsDifference { get; set; }

    /* Methods */

    public void ConvertFromXmlTeamStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
      if (teamStanding == null)
        return;

      Team_Id = teamStanding.Team_Id;

      Team = teamStanding.Team;
      Played = teamStanding.Played;
      Points = teamStanding.Points;

      Won = teamStanding.Won;
      Draw = teamStanding.Lost;
      Lost = teamStanding.Lost;

      GoalsFor = teamStanding.Goals_For;
      GoalsAgainst = teamStanding.Goals_Against;
      GoalsDifference = teamStanding.Goal_Difference;
    }

    public bool IsEqualToXmlTeamStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
      if (teamStanding == null)
        return false;

      if (Team_Id != teamStanding.Team_Id)
        return false;
      if (Team != teamStanding.Team)
        return false;
      if (Played != teamStanding.Played)
        return false;
      if (Points != teamStanding.Points)
        return false;
      if (Won != teamStanding.Won)
        return false;
      if (Draw != teamStanding.Draw)
        return false;
      if (Lost != teamStanding.Lost)
        return false;
      if (GoalsFor != teamStanding.Goals_For)
        return false;
      if (GoalsAgainst != teamStanding.Goals_Against)
        return false;
      if (GoalsDifference != teamStanding.Goal_Difference)
        return false;

      return true;
    }
  }
}