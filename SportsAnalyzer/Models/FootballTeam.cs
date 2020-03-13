namespace SportsAnalyzer.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public static class FootballTeamConverter
  {
    public static List<FootballTeam> ConvertToTeamList(this List<XMLSoccerCOM.Team> xmlList)
    {
      var list = new List<FootballTeam>();
      foreach (var xmlTeam in xmlList)
      {
        if (xmlTeam != null)
          list.Add(new FootballTeam(xmlTeam));
      }
      return list;
    }
  }

  public class FootballTeam
  {
    /* Fields */

    public const int MaxTeamNameLength = 80;
    public const int MaxCountryNameLength = 60;

    /* Constructors */

    public FootballTeam() { }

    public FootballTeam(XMLSoccerCOM.Team team)
    {
      ConvertFromXmlSoccerTeam(team);
    }

    /* Properties */

    public int Id { get; set; }

    [Required]
    [Display(Name = "Team Id")]
    public int Team_Id { get; set; }

    [Required]
    [Display(Name = "Team Name")]
    [StringLength(MaxTeamNameLength, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Country")]
    [StringLength(MaxCountryNameLength, MinimumLength = 2)]
    public string Country { get; set; }

    [Display(Name = "Stadium")]
    public string Stadium { get; set; }

    [Display(Name = "Website")]
    public string HomePageURL { get; set; }

    [Display(Name = "Wiki link")]
    public string WIKILink { get; set; }

    /* Methods */

    public void ConvertFromXmlSoccerTeam(XMLSoccerCOM.Team team)
    {
      if (team == null)
        return;

      Team_Id = team.Team_Id;
      Name = team.Name;
      Country = team.Country;
      Stadium = team.Stadium;
      HomePageURL = team.HomePageURL;
      WIKILink = team.WIKILink;
    }

    public bool IsEqualToXmlTeam(XMLSoccerCOM.Team team)
    {
      if (team == null)
        return false;

      if (Team_Id != team.Team_Id)
        return false;
      if (Name != team.Name)
        return false;
      if (Country != team.Country)
        return false;

      return true;
    }
  }
}