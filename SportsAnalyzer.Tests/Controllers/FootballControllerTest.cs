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
    const string standardString = "abcd";

    const int defaultSeasonYear = FootballController.DefaultSeasonYear;
    const int seasonYearExample = 2001;

    const string defaultLeagueShortName = FootballController.DefaultLeagueShortName;
    const string defaultLeague = FootballController.DefaultLeagueFullName;
    const string defaultLeagueId = FootballController.DefaultLeagueId;
    const string leagueIdExample = "league";

    /* Delegates */ 

    delegate ActionResult FootballControllerAction(string league = defaultLeague, int seasonYear = defaultSeasonYear);

    /* Methods */

    [TestMethod]
    public void ShowTeams_TestList_EqualLists()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestTeamList(numberOfTeams);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      ViewResult viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      // Checking if list forwarded as a model (viewResult.Model) to View
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
    public void ShowTeams_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(0);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      ViewResult viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTeams_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(1, shortString);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      footballController.Teams();

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
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
      CallControlerActionMuliply(footballController.Teams, listOfCallArgs);

      // Assert
    }

    [TestMethod]
    public void ShowTable_TestTable_EqualTables()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestLeagueTable(numberOfTeams);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      ViewResult viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      // Checking if list forwarded as a model (viewResult.Model) to View
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
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestLeagueTable(0);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      ViewResult viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTable_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestLeagueTable(1, shortString);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(mockXmlReq.Object);

      // Act
      footballController.Table();

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    public void ShowTable_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
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
      CallControlerActionMuliply(footballController.Table, listOfCallArgs);

      // Assert
    }

    /* Auxiliary methods */

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

    private List<XMLSoccerCOM.Team> CreateTestTeamList(int size, string testString = standardString)
    {
      List<XMLSoccerCOM.Team> xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }
      return xmlList;
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

    private List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(int size, string testString = standardString, int testInt = 0)
    {
      List<XMLSoccerCOM.TeamLeagueStanding> xmlList = new List<XMLSoccerCOM.TeamLeagueStanding>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeamLeagueStanding(i, testInt, testString));
      }
      return xmlList;
    }

    private Mock<IXmlSoccerRequester> SetSequenceOfMockCalls<T>(T xmlData, List<Expression<Func<IXmlSoccerRequester, T>>> CallMockExpressions)
    {
      var mockXmlReq = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);

      MockSequence sequence = new MockSequence();

      foreach (var expression in CallMockExpressions)
      {
        mockXmlReq.InSequence(sequence)
          .Setup(expression)
          .Returns(xmlData);
      }

      return mockXmlReq;
    }

    private void CallControlerActionMuliply(FootballControllerAction footballControllerAction, 
                                           List<(string league, int? seasonYear)> listOfCallArgs)
    {
      foreach (var (league, seasonYear) in listOfCallArgs)
      {
        if (league == null && seasonYear == null)
        {
          // calling controller action without parameters
          footballControllerAction();
        }
        else if (seasonYear == null)
        {
          // calling controller action with 1 parameter
          footballControllerAction(league);
        }
        else
        {
          // calling controller action with 2 parameters
          footballControllerAction(league, seasonYear.Value);
        }
      }
      // Probably calls with null arguments are unnecessary - arguments of Controller action has default values
    }


  }
}
