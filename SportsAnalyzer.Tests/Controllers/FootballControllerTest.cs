using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsAnalyzer.Controllers;
using System.Web.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
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
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);
      var xmlTestList = CreateTestTeamList(0);

      MockSequence sequence = new MockSequence();
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestList);

      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestList);
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestList);
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, defaultSeasonYear))
        .Returns(xmlTestList);

      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, seasonYearExample))
        .Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act

      // calling controller action without parameters
      ViewResult viewResult = footballController.Teams() as ViewResult;

      // passing one parameter to controller action
      viewResult = footballController.Teams(defaultLeagueShortName) as ViewResult;
      viewResult = footballController.Teams(defaultLeagueId) as ViewResult;
      viewResult = footballController.Teams(leagueIdExample) as ViewResult;

      // passing two parameter to controller action;
      viewResult = footballController.Teams(leagueIdExample, seasonYearExample) as ViewResult;

      // Probably calls with null arguments are unneccessary - arguments of Controller action has default values

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


    // Question is that whether these following methods are practically needed
    // - assuming that XmlSoccer.Requester doesn't provide Data with null objects or null fields

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTeams_TeamWithNullFields_EntityValidationException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(1, null);

      XmlSoccerReq_Mock.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(defaultLeague) as ViewResult;

      // Assert
      XmlSoccerReq_Mock.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(NullReferenceException))]
    public void ShowTeams_ListWithNullObjects_NullReferenceException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(3);
      xmlTestList[1] = null;

      XmlSoccerReq_Mock.Setup(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(defaultLeague) as ViewResult;

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

      MockSequence sequence = new MockSequence();
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestLeagueTable);

      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestLeagueTable);
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear))
        .Returns(xmlTestLeagueTable);
      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetLeagueStandingsBySeason(leagueIdExample, defaultSeasonYear))
        .Returns(xmlTestLeagueTable);

      XmlSoccerReq_Mock.InSequence(sequence)
        .Setup(x => x.GetLeagueStandingsBySeason(leagueIdExample, seasonYearExample))
        .Returns(xmlTestLeagueTable);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act

      // calling controller action without parameters
      ViewResult viewResult = footballController.Table() as ViewResult;

      // passing one parameter to controller action
      viewResult = footballController.Table(defaultLeagueShortName) as ViewResult;
      viewResult = footballController.Table(defaultLeagueId) as ViewResult;
      viewResult = footballController.Table(leagueIdExample) as ViewResult;

      // passing two parameter to controller action
      viewResult = footballController.Table(leagueIdExample, seasonYearExample) as ViewResult;

      // Probably calls with null arguments are unneccessary - arguments of Controller action has default values

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

    List<XMLSoccerCOM.Team> CreateTestTeamList(int size, string testString = "abcd")
    {
      List<XMLSoccerCOM.Team> xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }
      return xmlList;
    }

    List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(int size, string testString = "abcd", int testInt = 0)
    {
      List<XMLSoccerCOM.TeamLeagueStanding> xmlList = new List<XMLSoccerCOM.TeamLeagueStanding>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeamLeagueStanding(i, testInt, testString));
      }
      return xmlList;
    }

    XMLSoccerCOM.Team CreateTestTeam(int team_Id, string testString)
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

    XMLSoccerCOM.TeamLeagueStanding CreateTestTeamLeagueStanding(int team_Id, int testInt, string testString)
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


  }
}
