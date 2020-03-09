using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsAnalyzer.Controllers;
using System.Web.Mvc;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using SportsAnalyzer.Models;
using System.Data.Entity.Validation;
using static SportsAnalyzer.Common;
using SportsAnalyzer.DAL;

namespace SportsAnalyzer.Tests.Controllers
{
  [TestClass]
  public class FootballControllerTest
  {
    /* Fields */

    private const int numberOfTeams = 12;
    private const string shortString = "a";
    private const string standardString = "abcd";

    private const int defaultSeasonYear = DefaultSeasonYear;
    private const int seasonYearExample = 2001;

    private const string defaultLeagueShortName = DefaultLeagueShortName;
    private const string defaultLeague = DefaultLeagueFullName;
    private const string defaultLeagueId = DefaultLeagueId;
    private const string leagueIdExample = "league";

    /* Delegates */

    private delegate ActionResult FootballControllerAction(
      string league = defaultLeague,
      int seasonYear = defaultSeasonYear);

    /* Methods */

    [TestMethod]
    public void ShowTable_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestLeagueTable = CreateTestLeagueTable(0);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TableLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      var viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testXmlSoccerContext.SavedChanges);
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )

      var dbList = viewResult.Model as List<TeamLeagueStanding>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(0, xmlTestLeagueTable.Count);
      Assert.AreEqual(xmlTestLeagueTable.Count, dbList.Count);
    }

    [TestMethod]
    public void ShowTable_RecentlyUpdatedTable_NoRequestInvocation()
    {
      // Arrange
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TableLastUpdateTime = DateTime.UtcNow;
      LastUpdateTime = DateTime.UtcNow;

      // Act
      var viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Never());

      Assert.AreEqual(0, testXmlSoccerContext.SavedChanges);
    }

    // Not completely proper unit test - necessity of using database (external dependency)
    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTable_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var xmlTestLeagueTable = CreateTestLeagueTable(1, shortString);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object);
      TableLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      footballController.Table();
    }

    [TestMethod]
    public void ShowTable_TestTable_EqualTables()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestLeagueTable = CreateTestLeagueTable(numberOfTeams);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TableLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      var viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testXmlSoccerContext.SavedChanges);

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects?)

      var dbList = viewResult.Model as List<TeamLeagueStanding>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(numberOfTeams, xmlTestLeagueTable.Count);
      Assert.AreEqual(xmlTestLeagueTable.Count, dbList.Count);
      for (int i = 0; i < dbList.Count; i++)
      {
        Assert.IsTrue(dbList[i].IsEqualToXmlTeamStanding(xmlTestLeagueTable[i]));
      }
    }

    [TestMethod]
    public void ShowTable_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestLeagueTable = CreateTestLeagueTable(0);

      var callMockExpressions =
        new List<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.TeamLeagueStanding>>>>
      {
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(leagueIdExample, defaultSeasonYear),
        x => x.GetLeagueStandingsBySeason(leagueIdExample, seasonYearExample),
      };

      var mockXmlReq = SetSequenceOfMockCalls(xmlTestLeagueTable, callMockExpressions);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);

      var listOfCallArgs = new List<(string, int?)>
      {
        (null, null),
        (defaultLeagueShortName, null),
        (defaultLeague, defaultSeasonYear),
        (defaultLeagueId, null),
        (leagueIdExample, null),
        (leagueIdExample, seasonYearExample),
      };

      // Act
      CallControlerActionMuliply(footballController.Table, listOfCallArgs);

      // Assert
      Assert.AreEqual(callMockExpressions.Count, testXmlSoccerContext.SavedChanges);

      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()),
        Times.Exactly(callMockExpressions.Count));
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(defaultLeague, defaultSeasonYear),
        Times.Exactly(callMockExpressions.Count - 2));
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(leagueIdExample, defaultSeasonYear), Times.Once);
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(leagueIdExample, seasonYearExample), Times.Once);
    }

    [TestMethod]
    public void ShowTeams_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestTeamList = CreateTestTeamList(0);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TeamsLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      var viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testXmlSoccerContext.SavedChanges);
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )

      var dbList = viewResult.Model as List<FootballTeam>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(0, xmlTestTeamList.Count);
      Assert.AreEqual(xmlTestTeamList.Count, dbList.Count);
    }

    [TestMethod]
    public void ShowTeams_RecentlyUpdatedList_NoRequestInvocation()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TeamsLastUpdateTime = DateTime.UtcNow;
      LastUpdateTime = DateTime.UtcNow;

      // Act
      var viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Never());

      Assert.AreEqual(0, testXmlSoccerContext.SavedChanges);
    }

    // Not completely proper unit test - necessity of using database (external dependency)
    [TestMethod]
    [ExpectedException(exceptionType: typeof(DbEntityValidationException))]
    public void ShowTeams_StringLengthOutOfRange_EntityValidationException()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var xmlTestTeamList = CreateTestTeamList(1, shortString);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object);
      TeamsLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      footballController.Teams();
    }

    [TestMethod]
    public void ShowTeams_TestList_EqualLists()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestTeamList = CreateTestTeamList(numberOfTeams);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(xmlTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);
      TeamsLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;

      // Act
      var viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testXmlSoccerContext.SavedChanges);

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )

      var dbList = viewResult.Model as List<FootballTeam>;

      Assert.IsNotNull(dbList);
      Assert.AreEqual(numberOfTeams, xmlTestTeamList.Count);
      Assert.AreEqual(xmlTestTeamList.Count, dbList.Count);

      for (int i = 0; i < dbList.Count; i++)
      {
        Assert.IsTrue(dbList[i].IsEqualToXmlTeam(xmlTestTeamList[i]));
      }
    }

    [TestMethod]
    public void ShowTeams_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var testXmlSoccerContext = new TestXmlSoccerAPI_DBContext();
      var xmlTestTeamList = CreateTestTeamList(0);

      var callMockExpressions =
        new List<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.Team>>>>
      {
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, defaultSeasonYear),
        x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, seasonYearExample),
      };

      var mockXmlReq = SetSequenceOfMockCalls(xmlTestTeamList, callMockExpressions);

      var footballController = new FootballController(mockXmlReq.Object, testXmlSoccerContext);

      var listOfCallArgs = new List<(string, int?)>
      {
        (null, null),
        (defaultLeagueShortName, null),
        (defaultLeague, defaultSeasonYear),
        (defaultLeagueId, null),
        (leagueIdExample, null),
        (leagueIdExample, seasonYearExample),
      };

      // Act
      CallControlerActionMuliply(footballController.Teams, listOfCallArgs);

      // Assert
      Assert.AreEqual(callMockExpressions.Count, testXmlSoccerContext.SavedChanges);

      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()),
        Times.Exactly(callMockExpressions.Count));
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(defaultLeague, defaultSeasonYear),
        Times.Exactly(callMockExpressions.Count - 2));
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, defaultSeasonYear), Times.Once);
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(leagueIdExample, seasonYearExample), Times.Once);
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
        HomePageURL = testString,
      };
    }

    private List<XMLSoccerCOM.Team> CreateTestTeamList(
      int size,
      string testString = standardString)
    {
      var xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }
      return xmlList;
    }

    private XMLSoccerCOM.TeamLeagueStanding CreateTestTeamLeagueStanding(
      int team_Id,
      int testInt,
      string testString)
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
        Goal_Difference = testInt,
      };
    }

    private List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(
      int size,
      string testString = standardString,
      int testInt = 0)
    {
      var xmlList = new List<XMLSoccerCOM.TeamLeagueStanding>();

      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeamLeagueStanding(i, testInt, testString));
      }
      return xmlList;
    }

    private Mock<IXmlSoccerRequester> SetSequenceOfMockCalls<T>(
      T xmlData,
      List<Expression<Func<IXmlSoccerRequester, T>>> CallMockExpressions) where T : class
    {
      var mockXmlReq = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);

      var sequence = new MockSequence();

      foreach (var expression in CallMockExpressions)
      {
        mockXmlReq.InSequence(sequence)
          .Setup(expression)
          .Returns(xmlData);
      }

      return mockXmlReq;
    }

    private void CallControlerActionMuliply(
      FootballControllerAction footballControllerAction,
      List<(string league, int? seasonYear)> listOfCallArgs)
    {
      foreach (var (league, seasonYear) in listOfCallArgs)
      {
        // Reset update time variables to call IXmlSoccerRequester methods
        // each time footballControllerAction is called
        TableLastUpdateTime = DateTime.MinValue;
        TeamsLastUpdateTime = DateTime.MinValue;
        LastUpdateTime = DateTime.MinValue;

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
      // Probably calls with null arguments are unnecessary 
      // - arguments of Controller action has default values
    }
  }

  public class TestXmlSoccerAPI_DBContext : IXmlSoccerAPI_DBContext
  {
    public TestXmlSoccerAPI_DBContext()
    {
      FootballTeams = new TestDbSet<FootballTeam>();
      LeagueTable = new TestDbSet<TeamLeagueStanding>();
      LeagueMatches = new TestDbSet<FootballMatch>();
      SavedChanges = 0;
    }

    public DbSet<FootballTeam> FootballTeams { get; set; }
    public DbSet<TeamLeagueStanding> LeagueTable { get; set; }
    public DbSet<FootballMatch> LeagueMatches { get; set; }
    public int SavedChanges { get; private set; }

    public int SaveChanges()
    {
      SavedChanges++;
      return 0;
    }

    public void Dispose() { }
  }

  public class TestDbSet<T> : DbSet<T>, IQueryable, IEnumerable<T>
    where T : class
  {
    private readonly ObservableCollection<T> _data;
    private readonly IQueryable _query;

    public TestDbSet()
    {
      _data = new ObservableCollection<T>();
      _query = _data.AsQueryable();
    }

    public override T Add(T item)
    {
      _data.Add(item);
      return item;
    }

    public override IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
      foreach (var item in entities)
      {
        _data.Add(item);
      }
      return _data;
    }

    public override T Remove(T item)
    {
      _data.Remove(item);
      return item;
    }

    public override T Attach(T item)
    {
      _data.Add(item);
      return item;
    }

    public override T Create()
    {
      return Activator.CreateInstance<T>();
    }

    public override TDerivedEntity Create<TDerivedEntity>()
    {
      return Activator.CreateInstance<TDerivedEntity>();
    }

    public override ObservableCollection<T> Local
    {
      get { return new ObservableCollection<T>(_data); }
    }

    Type IQueryable.ElementType
    {
      get { return _query.ElementType; }
    }

    System.Linq.Expressions.Expression IQueryable.Expression
    {
      get { return _query.Expression; }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return _query.Provider; }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _data.GetEnumerator();
    }
  }
}
