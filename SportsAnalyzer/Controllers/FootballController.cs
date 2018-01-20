using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SportsAnalyzer.Models;
using SportsAnalyzer.DAL;
using System.Net;
using System.Diagnostics;
using System.Data.Entity.Validation;

namespace SportsAnalyzer.Controllers
{
  public interface IXmlSoccerRequester
  {
    List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear);
    List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear);
  }

  public class XmlSoccerRequester : IXmlSoccerRequester
  {
    private const string apiKey = "AZRBAQTJUNSUUELVRATIYETSXZJREDNJQVMHENMHJOAVVAZKRC";
    private XMLSoccerCOM.Requester _xmlSoccerRequester = new XMLSoccerCOM.Requester(apiKey);

    //public XmlSoccerRequester()
    //{
    //  apiKey= "AZRBAQTJUNSUUELVRATIYETSXZJREDNJQVMHENMHJOAVVAZKRC";
    //  _xmlSoccerRequester = new XMLSoccerCOM.Requester(apiKey);
    //}
    public List<XMLSoccerCOM.Team> GetAllTeamsByLeagueAndSeason(string league, int seasonStartYear)
    {
      Trace.Write("GetAllTeamsByLeagueAndSeason");
      return _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(league, seasonStartYear);

    }
    public List<XMLSoccerCOM.TeamLeagueStanding> GetLeagueStandingsBySeason(string league, int seasonStartYear)
    {
      Trace.Write("GetLeagueStandingsBySeason");
      return _xmlSoccerRequester.GetLeagueStandingsBySeason(league, seasonStartYear);
    }
  }

  public class FootballController : Controller
  {
    private XmlSoccerAPI_DBContext db = new XmlSoccerAPI_DBContext();

    private readonly IXmlSoccerRequester _xmlSoccerRequester;

    public const string SPL_id = "3";
    public const int defaultSeasonYear = 2017;

    public FootballController()
    {
      Trace.Write("FootballController()");
      _xmlSoccerRequester = new XmlSoccerRequester();
    }

    public FootballController(IXmlSoccerRequester xmlSoccerRequester)
    {
      Trace.Write("FootballController(IXmlSoccerRequester)");
      _xmlSoccerRequester = xmlSoccerRequester;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }

    // GET: Football
    public ActionResult Index()
    {
      ViewBag.Title = "Scottish Premier League main page.";

      return View(db.FootballTeams.ToList());
    }

    // GET: Football/Teams/{league}/{seasonYear}/{teamName}
    public ActionResult Teams(int seasonYear = defaultSeasonYear, string teamName = "")
    {
      var XmlFootbalTeams = _xmlSoccerRequester.GetAllTeamsByLeagueAndSeason(SPL_id, seasonYear);

      foreach (var dbTeam in db.FootballTeams)
      {
        db.FootballTeams.Remove(dbTeam);
      }
      int count = db.FootballTeams.Count();

      db.FootballTeams.AddRange(XmlFootbalTeams.ConvertToTeamList());
      db.SaveChanges();

      ViewBag.EmptyList = "The list of teams is empty";
      if (db.FootballTeams.Count() == 0)
        ViewBag.Message = "The list of teams is empty";

      return View(db.FootballTeams.ToList());
    }

    // GET: Football/Table/{league}/{seasonYear}/{teamName}
    // mode - tryb wyświetlania tabeli: 0-normalny, 1-rozszerzony
    public ActionResult Table(int seasonYear = defaultSeasonYear, int mode = 0)
    {
      //return View(db.LeagueTable.ToList());
      return new ViewResult();
    }

    //public ActionResult Team(int seasonYear = defaultSeasonYear, string teamName = "")
    //{
    //}
  }
}