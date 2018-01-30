using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsAnalyzer.Controllers;
using System.Web.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq.Expressions;
using SportsAnalyzer.Models;
using System.Data.Entity.Validation;

namespace SportsAnalyzer.Tests.Controllers
{
  [TestClass]
  public class FootballControllerTest
  {
    /* Fields */


    const int numberOfTeams = 12;
    const string shortString = "a";
    string longString = new string('a', 101);
    const string standardString = "abcd";

    const int defaultSeasonYear = FootballController.defaultSeasonYear;
    const int seasonYearExample = 2001;

    const string defaultLeagueShortName = FootballController.defaultLeagueShortName;
    const string defaultLeague = FootballController.defaultLeagueFullName;
    const string defaultLeagueId = FootballController.defaultLeagueId;
    const string leagueIdExample = "league";


    [TestMethod]
    public void ShowTeams_TestList_EqualLists()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestTeamList(numberOfTeams);

      XmlSoccerReq_Mock.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      // - checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )

      List<FootballTeam> dbList = viewResult.Model as List<FootballTeam>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(xmlTestList.Count, numberOfTeams);
      Assert.AreEqual(dbList.Count, xmlTestList.Count);

      for (int i = 0; i < dbList.Count; i++)
      {
        Assert.IsTrue(dbList[i].IsEqualToXmlTeam(xmlTestList[i]));
      }
    }


    [TestMethod]
    public void ShowTeams_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var xmlTestList = CreateTestTeamList(0);

      var callMockExpressions = new List<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.Team>>>>
      {
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, seasonYearExample),
      };

      Mock<IXmlSoccerRequester> mockXmlReq = SetSequenceOfMockCalls<List<XMLSoccerCOM.Team>>
        (xmlTestList, callMockExpressions);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      var listOfCallArgs = new List<(string, int?)>
      {
        (null, null),
        (defaultLeagueShortName, null),
        (defaultLeagueId, null),
        (leagueIdExample, null),
        (leagueIdExample, seasonYearExample)
      };

      // Act
      CallControlerActionRepeat(footballController.Teams, listOfCallArgs);

      // Assert
    }

    [TestMethod]
    public void ShowTeams_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(0);

      XmlSoccerReq_Mock.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTeams_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(1, shortString);

      XmlSoccerReq_Mock.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      ViewResult viewResult = null;

      // Act
      viewResult = footballController.Teams(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    public void ShowTable_TestTable_EqualTables()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestLeagueTable(numberOfTeams);

      XmlSoccerReq_Mock.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Table(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      // - checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )
      List<TeamLeagueStanding> dbList = viewResult.Model as List<TeamLeagueStanding>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(xmlTestList.Count, numberOfTeams);
      Assert.AreEqual(dbList.Count, xmlTestList.Count);
      for (int i = 0; i < dbList.Count; i++)
      {
        Assert.IsTrue(dbList[i].IsEqualToXmlTeamStanding(xmlTestList[i]));
      }

    }

    [TestMethod]
    public void ShowTable_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestLeagueTable(0);

      XmlSoccerReq_Mock.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Table(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

    }

    [TestMethod]
    public void ShowTable_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);
      var xmlTestLeagueTable = CreateTestLeagueTable(0);

      var callMockExpressions = new List<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.TeamLeagueStanding>>>>
      {
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(leagueIdExample, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(leagueIdExample, seasonYearExample),
      };

      Mock<IXmlSoccerRequester> mockXmlReq = SetSequenceOfMockCalls<List<XMLSoccerCOM.TeamLeagueStanding>>
        (xmlTestLeagueTable, callMockExpressions);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      var listOfCallArgs = new List<(string, int?)>
      {
        (null, null),
        (defaultLeagueShortName, null),
        (defaultLeagueId, null),
        (leagueIdExample, null),
        (leagueIdExample, seasonYearExample)
      };

      // Act
      CallControlerActionRepeat(footballController.Table, listOfCallArgs);

      // Assert
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTable_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestLeagueTable(1, shortString);

      XmlSoccerReq_Mock.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Table(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    /* Auxiliary methods */

    private List<XMLSoccerCOM.Team> CreateTestTeamList(int size, string testString = "abcd")
    {
      List<XMLSoccerCOM.Team> xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }
      return xmlList;
    }

    private List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(int size, string testString = "abcd", int testInt = 0)
    {
      List<XMLSoccerCOM.TeamLeagueStanding> xmlList = new List<XMLSoccerCOM.TeamLeagueStanding>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeamLeagueStanding(i, testInt, testString));
      }
      return xmlList;
    }

    private XMLSoccerCOM.Team CreateTestTeam(int team_Id, string testString)
    {
      return new XMLSoccerCOM.Team
      {
        Team_Id = team_Id,
        Country = testString,
        Name = testString,
        Stadium = testString,
        WIKILink = testString,
        HomePageURL = testString
      };
    }

    private XMLSoccerCOM.TeamLeagueStanding CreateTestTeamLeagueStanding(int team_Id, int testInt, string testString)
    {
      return new XMLSoccerCOM.TeamLeagueStanding
      {
        Team_Id = team_Id,
        Team = testString,
        Played = testInt,
        Points = testInt,
        Won = testInt,
        Draw = testInt,
        Lost = testInt,
        Goals_For = testInt,
        Goals_Against = testInt,
        Goal_Difference = testInt
      };
    }

    private Mock<IXmlSoccerRequester> SetSequenceOfMockCalls<T>(T xmlData, List<Expression<Func<IXmlSoccerRequester, T>>> CallMockExpressions)
    {
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);

      MockSequence sequence = new MockSequence();

      foreach (var expression in CallMockExpressions)
      {
        XmlSoccerReq_Mock.InSequence(sequence)
          .Setup(expression)
          .Returns(xmlData);
      }

      return XmlSoccerReq_Mock;
    }

    delegate ActionResult FootballControllerAction(string league = defaultLeague, int seasonYear = defaultSeasonYear);

    private void CallControlerActionRepeat(FootballControllerAction footballControllerAction, 
                                           List<(string league, int? seasonYear)> listOfCallArgs)
    {
      foreach (var callArgs in listOfCallArgs)
      {
        if (callArgs.league == null && callArgs.seasonYear == null)
        {
          // calling controller action without parameters
          footballControllerAction();
        }
        else if (callArgs.seasonYear == null)
        {
          // calling controller action with 1 parameter
          footballControllerAction(callArgs.league);
        }
        else
        {
          // calling controller action with 2 parameters
          footballControllerAction(callArgs.league, callArgs.seasonYear.Value);
        }
      }
      // Probably calls with null arguments are unneccessary - arguments of Controller action has default values
    }


  }
}
