using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportsAnalyzer.Models
{
  public static class TeamLeagueStandingConverter
  {
    public static List<TeamLeagueStanding> ConvertToLeagueStandingList(
      this List<XMLSoccerCOM.TeamLeagueStanding> xmlList)
    {
      var list = new List<TeamLeagueStanding>();
      foreach (var xmlTeamLeagueStanding in xmlList)
      {
        list.Add(new TeamLeagueStanding(xmlTeamLeagueStanding));
      }
      return list;
    }
  }

  public class TeamLeagueStanding
  {
    /* Constant fields */

    public const int MaxTeamNameLength = 80;

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
    public int Goals_For { get; set; }

    [Required]
    [Display(Name = "Goals Against")]
    public int Goals_Against { get; set; }

    [Display(Name = "Goals Difference")]
    public int Goal_Difference { get; set; }

    /* Constructors */

    public TeamLeagueStanding() { }

    public TeamLeagueStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
      ConvertFromXmlTeamStanding(teamStanding);
    }

    /* Methods */

    public void ConvertFromXmlTeamStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
      Team_Id = teamStanding.Team_Id;

      Team = teamStanding.Team;
      Played = teamStanding.Played;
      Points = teamStanding.Points;

      Won = teamStanding.Won;
      Draw = teamStanding.Lost;
      Lost = teamStanding.Lost;

      Goals_For = teamStanding.Goals_For;
      Goals_Against = teamStanding.Goals_Against;
      Goal_Difference = teamStanding.Goal_Difference;
    }

    public bool IsEqualToXmlTeamStanding(XMLSoccerCOM.TeamLeagueStanding teamStanding)
    {
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
      if (Goals_For != teamStanding.Goals_For)
        return false;
      if (Goals_Against != teamStanding.Goals_Against)
        return false;
      if (Goal_Difference != teamStanding.Goal_Difference)
        return false;

      return true;
    }
  }
}