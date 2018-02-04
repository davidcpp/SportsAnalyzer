using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace SportsAnalyzer.Models
{
  public static class TeamLeagueStandingConverter
  {
    public static List<TeamLeagueStanding> ConvertToLeagueStandingList(this List<XMLSoccerCOM.TeamLeagueStanding> xmlList)
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
    /* Fields */

    public const int MaxTeamNameLength = 80;

    public int Id { get; set; }

    [Required]
    [Display(Name = "Id Klubu")]
    public int Team_Id { get; set; }

    [Required]
    [Display(Name = "Klub")]
    [StringLength(MaxTeamNameLength, MinimumLength = 2)]
    public string Team { get; set; }

    [Required]
    [Display(Name = "Mecze")]
    public int Played { get; set; }

    [Required]
    [Display(Name = "Punkty")]
    public int Points { get; set; }

    [Required]
    [Display(Name = "Wygrane")]
    public int Won { get; set; }

    [Required]
    [Display(Name = "Remisy")]
    public int Draw { get; set; }

    [Required]
    [Display(Name = "Przegrane")]
    public int Lost { get; set; }

    [Required]
    [Display(Name = "Bramki zdobyte")]
    public int Goals_For { get; set; }

    [Required]
    [Display(Name = "Bramki stracone")]
    public int Goals_Against { get; set; }

    [Display(Name = "Bilans bramek")]
    public int Goal_Difference { get; set; }

    //// Additional field present in the class XMLSoccerCOM.TeamLeagueStanding - here unneeded,
    //// possibly will be necessary for expanded view creating

    public int RedCards { get; set; }
    public int YellowCards { get; set; }
    public int NumberOfShots { get; set; }
    public int PlayedAway { get; set; }
    public int PlayedAtHome { get; set; }

    //public int? Group_Id { get; set; }
    //public string Group { get; set; }


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