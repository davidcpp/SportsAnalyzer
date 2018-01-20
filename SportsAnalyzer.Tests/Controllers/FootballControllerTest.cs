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

    [TestMethod]
    public void ShowTeams_TestList_EqualLists()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestTeamList(numberOfTeams);

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
      // - sprawdzenie czy lista, która została przekazana jako model (viewResult.Model) do widoku 
      // jest zgodna z listą testową  (czy posiada te same elementy co lista testowa)
      List<FootballTeam> dbList = viewResult.Model as List<FootballTeam>;

      // sprawdzenie zgodności rozmiarów i elementów listy testowej i uzyskanej z niej listy, która jest modelem dla widoku
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
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(0);

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTeams_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(1, shortString);

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      ViewResult viewResult = null;

      // Act
      viewResult = footballController.Teams(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
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

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
    }

    [TestMethod]
    [ExpectedException(exceptionType: typeof(NullReferenceException))]
    public void ShowTeams_ListWithNullObjects_NullReferenceException()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();

      var xmlTestList = CreateTestTeamList(3);
      xmlTestList[1] = null;

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Teams(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
    }

    [TestMethod]
    public void ShowTable_TestTable_EqualTables()
    {
      // Arrange
      var XmlSoccerReq_Mock = new Mock<IXmlSoccerRequester>();
      var xmlTestList = CreateTestLeagueTable(numberOfTeams);

      XmlSoccerReq_Mock.Setup(xmlReq => xmlReq.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>())).
        Returns(xmlTestList);

      FootballController footballController = new FootballController(XmlSoccerReq_Mock.Object);

      // Act
      ViewResult viewResult = footballController.Table(FootballController.defaultSeasonYear) as ViewResult;

      // Assert
      // - sprawdzenie czy lista, która została przekazana jako model (viewResult.Model) do widoku 
      // jest zgodna z listą testową  (czy posiada te same elementy co lista testowa)
      List<TeamLeagueStanding> dbList = viewResult.Model as List<TeamLeagueStanding>;

      // sprawdzenie zgodności rozmiarów i elementów listy testowej i uzyskanej z niej listy, która jest modelem dla widoku
      Assert.IsNotNull(dbList);
      Assert.AreEqual(xmlTestList.Count, numberOfTeams);
      Assert.AreEqual(dbList.Count, xmlTestList.Count);
      for (int i = 0; i < dbList.Count; i++)
      {
        Assert.IsTrue(dbList[i].IsEqualToXmlTeamStanding(xmlTestList[i]));
      }

    }

    List<XMLSoccerCOM.Team> CreateTestTeamList(int size, string testString = "abcd")
    {
      List<XMLSoccerCOM.Team> xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }
      return xmlList;
    }

    List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(int size, int testInt = 0, string testString = "abcd")
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
