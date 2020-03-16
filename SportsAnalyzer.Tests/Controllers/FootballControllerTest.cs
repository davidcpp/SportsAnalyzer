namespace SportsAnalyzer.Tests.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Data.Entity;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Web.Mvc;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Moq;
  using SportsAnalyzer.Controllers;
  using SportsAnalyzer.DAL;
  using SportsAnalyzer.Models;
  using static AuxiliaryMethods;
  using static SportsAnalyzer.Common;

  public static class AuxiliaryMethods
  {
    /* Fields */
    public static readonly int NumberOfTeams = 12;
    public static readonly int SeasonYearExample = 2001;
    public static readonly string LeagueIdExample = "league";

    public static readonly (string, int)[] CallMockArguments = new[]
    {
      (DefaultLeagueFullName, DefaultSeasonYear),
      (DefaultLeagueFullName, DefaultSeasonYear),
      (DefaultLeagueFullName, DefaultSeasonYear),
      (DefaultLeagueFullName, DefaultSeasonYear),
      (LeagueIdExample, DefaultSeasonYear),
      (LeagueIdExample, SeasonYearExample)
    };

    public static readonly List<(string, int?)> CallActionArguments = new List<(string, int?)>
    {
      (null, null),
      (DefaultLeagueShortName, null),
      (DefaultLeagueFullName, DefaultSeasonYear),
      (DefaultLeagueId, null),
      (LeagueIdExample, null),
      (LeagueIdExample, SeasonYearExample),
    };

    private const string StandardString = "abcd";

    /* Methods */

    public static IEnumerable<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.TeamLeagueStanding>>>>
      CreateTableRequestsExpressions((string, int)[] callMockArguments)
    {
      foreach (var (league, seasonYear) in callMockArguments)
      {
        yield return x => x.GetLeagueStandingsBySeason(league, seasonYear);
      }
    }

    public static IEnumerable<Expression<Func<IXmlSoccerRequester, List<XMLSoccerCOM.Team>>>>
      CreateTeamsRequestsExpressions((string, int)[] callMockArguments)
    {
      foreach (var (league, seasonYear) in callMockArguments)
      {
        yield return x => x.GetAllTeamsByLeagueAndSeason(league, seasonYear);
      }
    }

    public static XMLSoccerCOM.Team CreateTestTeam(int team_Id, string testString)
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

    public static List<XMLSoccerCOM.Team> CreateTestTeamList(
      int size,
      string testString = StandardString)
    {
      var xmlList = new List<XMLSoccerCOM.Team>();
      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeam(i, testString));
      }

      return xmlList;
    }

    public static XMLSoccerCOM.TeamLeagueStanding CreateTestTeamLeagueStanding(
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

    public static List<XMLSoccerCOM.TeamLeagueStanding> CreateTestLeagueTable(
      int size,
      string testString = StandardString,
      int testInt = 0)
    {
      var xmlList = new List<XMLSoccerCOM.TeamLeagueStanding>();

      for (int i = 1; i <= size; i++)
      {
        xmlList.Add(CreateTestTeamLeagueStanding(i, testInt, testString));
      }

      return xmlList;
    }

    public static void CallControlerActionMuliply(
      FootballControllerTest.FootballControllerAction footballControllerAction,
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
    }
  }

  public static class MockExtensions
  {
    public static void SetupSequenceCalls<T>(
      this Mock<IXmlSoccerRequester> mockXmlReq,
      List<T> xmlData,
      List<Expression<Func<IXmlSoccerRequester, List<T>>>> callMockExpressions) where T : class
    {
      var sequence = new MockSequence();

      foreach (var expression in callMockExpressions)
      {
        mockXmlReq.InSequence(sequence)
          .Setup(expression)
          .Returns(xmlData);
      }
    }
  }

  [TestClass]
  public class FootballControllerTest
  {
    public FootballControllerTest()
    {
      // These assignments are executed before each unit test
      TableLastUpdateTime = DateTime.MinValue;
      TeamsLastUpdateTime = DateTime.MinValue;
      LastUpdateTime = DateTime.MinValue;
    }

    /* Delegates */

    public delegate ActionResult FootballControllerAction(
      string league = DefaultLeagueFullName,
      int seasonYear = DefaultSeasonYear);

    /* Methods */

    [TestMethod]
    public void ShowTable_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestLeagueTable = CreateTestLeagueTable(0);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

      var modelLeagueTable = viewResult.Model as List<TeamLeagueStanding>;

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )
      Assert.IsNotNull(modelLeagueTable);
      Assert.AreEqual(0, apiTestLeagueTable.Count);
      Assert.AreEqual(apiTestLeagueTable.Count, modelLeagueTable.Count);
    }

    [TestMethod]
    public void ShowTable_RepeatedControllerActionCall_NoRepeatedApiRequest()
    {
      // Arrange
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var apiTestLeagueTable = CreateTestLeagueTable(0);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Table() as ViewResult;
      viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);
    }

    [TestMethod]
    public void ShowTable_TestTable_EqualTables()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestLeagueTable = CreateTestLeagueTable(NumberOfTeams);

      mockXmlReq.Setup(x => x.GetLeagueStandingsBySeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestLeagueTable);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Table() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);

      var modelLeagueTable = viewResult.Model as List<TeamLeagueStanding>;

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects?)
      Assert.IsNotNull(modelLeagueTable);
      Assert.AreEqual(NumberOfTeams, apiTestLeagueTable.Count);
      Assert.AreEqual(apiTestLeagueTable.Count, modelLeagueTable.Count);

      for (int i = 0; i < modelLeagueTable.Count; i++)
      {
        Assert.IsTrue(modelLeagueTable[i].IsEqualToXmlTeamStanding(apiTestLeagueTable[i]));
      }
    }

    [TestMethod]
    public void ShowTable_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestLeagueTable = CreateTestLeagueTable(0);

      var callMockExpressions = CreateTableRequestsExpressions(CallMockArguments).ToList();
      var mockXmlReq = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);
      mockXmlReq.SetupSequenceCalls(apiTestLeagueTable, callMockExpressions);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      CallControlerActionMuliply(footballController.Table, CallActionArguments);

      // Assert
      Assert.AreEqual(callMockExpressions.Count, testDBContext.SavedChanges);

      mockXmlReq.Verify(
        x => x.GetLeagueStandingsBySeason(It.IsAny<string>(), It.IsAny<int>()),
        Times.Exactly(callMockExpressions.Count));
      mockXmlReq.Verify(
        x => x.GetLeagueStandingsBySeason(DefaultLeagueFullName, DefaultSeasonYear),
        Times.Exactly(callMockExpressions.Count - 2));
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(LeagueIdExample, DefaultSeasonYear), Times.Once);
      mockXmlReq.Verify(x => x.GetLeagueStandingsBySeason(LeagueIdExample, SeasonYearExample), Times.Once);
    }

    [TestMethod]
    public void ShowTeams_EmptyXmlList_ViewMessage()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();

      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestTeamList = CreateTestTeamList(0);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);
      Assert.AreEqual(viewResult.ViewBag.EmptyList, viewResult.ViewBag.Message);

      var modelTeamList = viewResult.Model as List<FootballTeam>;

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )
      Assert.IsNotNull(modelTeamList);
      Assert.AreEqual(0, apiTestTeamList.Count);
      Assert.AreEqual(apiTestTeamList.Count, modelTeamList.Count);
    }

    [TestMethod]
    public void ShowTeams_RepeatedControllerActionCall_NoRepeatedApiRequest()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestTeamList = CreateTestTeamList(0);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Teams() as ViewResult;
      viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);
    }

    [TestMethod]
    public void ShowTeams_TestList_EqualLists()
    {
      // Arrange
      var mockXmlReq = new Mock<IXmlSoccerRequester>();
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestTeamList = CreateTestTeamList(NumberOfTeams);

      mockXmlReq.Setup(x => x.GetAllTeamsByLeagueAndSeason(
        It.IsAny<string>(), It.IsAny<int>()))
        .Returns(apiTestTeamList);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      var viewResult = footballController.Teams() as ViewResult;

      // Assert
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()), Times.Once());

      Assert.AreEqual(1, testDBContext.SavedChanges);

      var modelTeamList = viewResult.Model as List<FootballTeam>;

      // Checking if list forwarded as a model (viewResult.Model) to View
      // corresponds to test list (Does this list have the same values of their objects? )
      Assert.IsNotNull(modelTeamList);
      Assert.AreEqual(NumberOfTeams, apiTestTeamList.Count);
      Assert.AreEqual(apiTestTeamList.Count, modelTeamList.Count);

      for (int i = 0; i < modelTeamList.Count; i++)
      {
        Assert.IsTrue(modelTeamList[i].IsEqualToXmlTeam(apiTestTeamList[i]));
      }
    }

    [TestMethod]
    public void ShowTeams_VariousControllerArgs_ProperArgumentsCall()
    {
      // Arrange
      var testDBContext = new TestXmlSoccerAPI_DBContext();
      var apiTestTeamList = CreateTestTeamList(0);

      var callMockExpressions = CreateTeamsRequestsExpressions(CallMockArguments).ToList();
      var mockXmlReq = new Mock<IXmlSoccerRequester>(MockBehavior.Strict);
      mockXmlReq.SetupSequenceCalls(apiTestTeamList, callMockExpressions);

      var footballController = new FootballController(mockXmlReq.Object, testDBContext);

      // Act
      CallControlerActionMuliply(footballController.Teams, CallActionArguments);

      // Assert
      Assert.AreEqual(callMockExpressions.Count, testDBContext.SavedChanges);

      mockXmlReq.Verify(
        x => x.GetAllTeamsByLeagueAndSeason(It.IsAny<string>(), It.IsAny<int>()),
        Times.Exactly(callMockExpressions.Count));
      mockXmlReq.Verify(
        x => x.GetAllTeamsByLeagueAndSeason(DefaultLeagueFullName, DefaultSeasonYear),
        Times.Exactly(callMockExpressions.Count - 2));
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(LeagueIdExample, DefaultSeasonYear), Times.Once);
      mockXmlReq.Verify(x => x.GetAllTeamsByLeagueAndSeason(LeagueIdExample, SeasonYearExample), Times.Once);
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

    public void Dispose()
    {
    }
  }

  public partial class TestDbSet<T> : DbSet<T>
    where T : class
  {
    private readonly ObservableCollection<T> data;
    private readonly IQueryable query;

    public TestDbSet()
    {
      data = new ObservableCollection<T>();
      query = data.AsQueryable();
    }

    public override ObservableCollection<T> Local
    {
      get { return new ObservableCollection<T>(data); }
    }

    public override T Add(T item)
    {
      data.Add(item);
      return item;
    }

    public override IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
      foreach (var item in entities)
      {
        data.Add(item);
      }

      return data;
    }

    public override T Remove(T item)
    {
      data.Remove(item);
      return item;
    }

    public override T Attach(T item)
    {
      data.Add(item);
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
  }

  public partial class TestDbSet<T> : IQueryable, IEnumerable<T>
    where T : class
  {
    Type IQueryable.ElementType
    {
      get { return query.ElementType; }
    }

    System.Linq.Expressions.Expression IQueryable.Expression
    {
      get { return query.Expression; }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return query.Provider; }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return data.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return data.GetEnumerator();
    }
  }
}
