using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace SportsAnalyzer.Models
{
  public static class FootballTeamConverter
  {
    public static List<FootballTeam> ConvertToTeamList(this List<XMLSoccerCOM.Team> XmlList)
    {
      List<FootballTeam> list = new List<FootballTeam>();
      foreach (var xmlTeam in XmlList)
      {
        list.Add(new FootballTeam(xmlTeam));
      }
      return list;
    }
  }

  public class FootballTeam
  {
    /* Fields */

    public int Id { get; set; }

    [Required]
    [Display(Name = "Id drużyny")]
    public int Team_Id { get; set; }

    [Required]
    [Display(Name = "Nazwa Drużyny")]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; set; }

    //[Required]
    [Display(Name = "Kraj pochodzenia")]
    [StringLength(60, MinimumLength = 2)]
    public string Country { get; set; }

    [Display(Name = "Stadion")]
    public string Stadium { get; set; }

    [Display(Name = "Strona WWW")]
    public string HomePageURL { get; set; }

    [Display(Name = "Wiki link")]
    public string WIKILink { get; set; }

    /* Constructors */

    public FootballTeam() { }

    public FootballTeam(XMLSoccerCOM.Team team)
    {
      ConvertFromXmlSoccerTeam(team);
    }

    /* Methods */

    public void ConvertFromXmlSoccerTeam(XMLSoccerCOM.Team team)
    {
      Team_Id = team.Team_Id;
      Name = team.Name;
      Country = team.Country;
      Stadium = team.Stadium;
      HomePageURL = team.HomePageURL;
      WIKILink = team.WIKILink;
    }

    public bool IsEqualToXmlTeam(XMLSoccerCOM.Team team)
    {
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