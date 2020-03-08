using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using static System.Math;
using static SportsAnalyzer.Common;

namespace SportsAnalyzer.Models
{
  public class Statistics
  {
    /* Fields */

    private double[] goalsInIntervals = null;
    private double[] goalsInIntervalsPercent = null;
    private double[] timeIntervalsLimits = null;

    /* Constructors */

    public Statistics()
    {
    }

    public Statistics(int seasonYear = DefaultSeasonYear,
                    string leagueName = DefaultLeagueName,
                    string teamName = DefaultTeamName,
                    double numberOfMatchIntervals = DefaultNumberOfMatchIntervals)
    {
      SeasonYear = seasonYear;
      NumberOfMatchIntervals = numberOfMatchIntervals;
      MatchIntervalLength = MatchTime / NumberOfMatchIntervals;
      LeagueName = leagueName;
      TeamName = teamName;
    }

    /* Properties */

    public List<FootballMatch> AllMatches { get; private set; }
    public List<FootballMatch> SelectedMatches { get; private set; }
    public List<int> RoundsNumbersInts { get; set; } = new List<int>();
    public List<SelectListItem> RoundItems { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> TeamItems { get; set; } = new List<SelectListItem>();

    [Display(Name = "Chose rounds")]
    public MultiSelectList RoundsSelectList { get; private set; }

    [Display(Name = "Chose teams")]
    public MultiSelectList TeamsSelectList { get; private set; }

    public int SeasonYear { get; set; }
    public int LeagueRoundsNumber { get; private set; } = DefaultRoundsNumber;
    public string StartRound { get; private set; }
    public string EndRound { get; private set; }

    public double NumberOfMatchIntervals { get; }
    public double MatchIntervalLength { get; }
    public double MatchTime { get; } = DefaultMatchTime;

    [Display(Name = "League name")]
    public string LeagueName { get; set; }

    [Display(Name = "Team name")]
    public string TeamName { get; }

    [Display(Name = "Avg. number of goals")]
    public double GoalsAvg { get; private set; }

    [Display(Name = "Avg. number of host/guest goals")]
    public double GoalsAvgHome { get; private set; }

    public double GoalsAvgAway { get; private set; }

    [Display(Name = "Goals sum")]
    public double GoalsSum { get; private set; }

    [Display(Name = "Number of matches")]
    public double MatchesNumber { get; private set; }

    public double[] GoalsInIntervals
    {
      get
      {
        return goalsInIntervals ??
          (goalsInIntervals = new double[(int)NumberOfMatchIntervals]);
      }
      set { goalsInIntervals = value; }
    }

    public double[] GoalsInIntervalsPercent
    {
      get
      {
        return goalsInIntervalsPercent ??
          (goalsInIntervalsPercent = new double[(int)NumberOfMatchIntervals]);
      }
      set { goalsInIntervalsPercent = value; }
    }

    public double[] TimeIntervalsLimits
    {
      get
      {
        if (timeIntervalsLimits == null)
        {
          timeIntervalsLimits = new double[(int)NumberOfMatchIntervals + 1];
          for (int i = 0; i <= NumberOfMatchIntervals; i++)
          {
            timeIntervalsLimits[i] = (MatchTime / NumberOfMatchIntervals) * i;
          }
        }
        return timeIntervalsLimits;
      }
      set { timeIntervalsLimits = value; }
    }

    // Number of matches (%) with a given number of goals
    // MatchGoalsPct[numberOfGoals] = numberOfMatches
    public double[] MatchGoalsPct;

    // Number of points in the given round for the given team
    // RoundPoints[numberOfRound] = numberOfPoints
    public IDictionary<int, RoundResult> RoundPoints = new Dictionary<int, RoundResult>();

    // TablePositions[teamName][roundNumber] = position of the team in the table after roundNumber
    public static IDictionary<string, Dictionary<int, int>> TablePositions
      = new Dictionary<string, Dictionary<int, int>>();

    private static Task CalcTablePositions;
    private readonly object _lockObject = new object();

    /* Methods */

    public void CalculateBasicStats()
    {
      if (SelectedMatches == null || SelectedMatches.Count == 0)
      {
        return;
      }

      MatchesNumber = SelectedMatches.Count;
      GoalsSum = SelectedMatches.Sum((match) => match.HomeGoals ?? 0)
        + SelectedMatches.Sum((match) => match.AwayGoals ?? 0);
      GoalsAvg = Round(GoalsSum / MatchesNumber, 2);
      GoalsAvgHome = Round(SelectedMatches.Average((match) => match.HomeGoals ?? 0), 2);
      GoalsAvgAway = Round(SelectedMatches.Average((match) => match.AwayGoals ?? 0), 2);
    }

    public void CalculateGoalsInIntervals()
    {
      if (SelectedMatches == null || SelectedMatches.Count == 0)
      {
        return;
      }

      var regexGoalTime = new Regex("\\d{1,}");
      double scoredGoalsSum = 0;
      foreach (var match in SelectedMatches)
      {
        var homeGoalDetails = match.HomeGoalDetails.Split(new char[] { ';' });
        var awayGoalDetails = match.AwayGoalDetails.Split(new char[] { ';' });

        if (TeamName == match.HomeTeam || TeamName == DefaultTeamName)
        {
          scoredGoalsSum += match.HomeGoals ?? 0;

          foreach (var detailedGoal in homeGoalDetails)
          {
            if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
            {
              // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
              GoalsInIntervals[(int)Ceiling(goalTime / MatchIntervalLength) - 1]++;
            }
          }
        }

        if (TeamName == match.AwayTeam || TeamName == DefaultTeamName)
        {
          scoredGoalsSum += match.AwayGoals ?? 0;

          foreach (var detailedGoal in awayGoalDetails)
          {
            if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
            {
              // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
              GoalsInIntervals[(int)Ceiling(goalTime / MatchIntervalLength) - 1]++;
            }
          }
        }
      }

      for (int i = 0; i < NumberOfMatchIntervals; i++)
      {
        GoalsInIntervalsPercent[i] = Round((GoalsInIntervals[i] / scoredGoalsSum) * 100, 2);
      }
    }

    public void CalculateMatchGoals()
    {
      if (SelectedMatches == null || SelectedMatches.Count == 0)
      {
        return;
      }

      var matchGoals = new Dictionary<int, double>();
      int numberOfGoals = 0;
      double numberOfMatches = 0;

      foreach (var match in SelectedMatches)
      {
        numberOfGoals = match.HomeGoals ?? 0;
        numberOfGoals += match.AwayGoals ?? 0;
        if (!matchGoals.ContainsKey(numberOfGoals))
          matchGoals.Add(numberOfGoals, 0);
        matchGoals[numberOfGoals]++;
      }

      int maxNumberOfGoals = matchGoals.Max(x => x.Key);
      MatchGoalsPct = new double[maxNumberOfGoals + 1];

      foreach (var item in matchGoals)
      {
        numberOfGoals = item.Key;
        numberOfMatches = item.Value;
        MatchGoalsPct[numberOfGoals] = Round((numberOfMatches / SelectedMatches.Count) * 100, 2);
      }
    }

    /// <summary> Calculate team positions in the table for all rounds </summary>
    public void CalculateTablePositions()
    {
      var teamsNames = new HashSet<string>(AllMatches.Select(match => match.HomeTeam));

      int matchRound;
      var teamLeagueStandings = new Dictionary<string, TeamLeagueStanding>();
      var orderedMatches = AllMatches.OrderBy(match => match.Round ?? 0);

      InitAllTeamsStandings(teamLeagueStandings, teamsNames);

      for (int i = 0; i < orderedMatches.Count(); i++)
      {
        var match = orderedMatches.ElementAt(i);
        matchRound = match.Round ?? 0;

        if (matchRound == 0)
          continue;

        UpdateStandingsAfterMatch(teamLeagueStandings, match);

        // Calculating teams positions after all played matches in a given round - even incomplete
        // - Check if there are still any matches in a given round
        var nextMatch = orderedMatches.ElementAtOrDefault(i + 1);
        if (nextMatch?.Round != match.Round)
        {
          // Calculating teams positions in the league table
          var standingsInOrder = GetTeamsOrder(teamLeagueStandings);
          SetTeamTablePositions(standingsInOrder, matchRound);
        }
      }
    }

    /// <summary> Init standings objects for teamsNames </summary>
    private void InitAllTeamsStandings(Dictionary<string, TeamLeagueStanding> standings,
       HashSet<string> teamsNames)
    {
      foreach (var teamName in teamsNames)
      {
        if (!standings.ContainsKey(teamName))
        {
          standings[teamName] = new TeamLeagueStanding
          {
            Team = teamName
          };
        }
      }
    }

    /// <summary> Set goals and points for teams after match between them </summary>
    private void UpdateStandingsAfterMatch(Dictionary<string, TeamLeagueStanding> standings,
      FootballMatch match)
    {
      standings[match.HomeTeam].GoalsFor += match.HomeGoals ?? 0;
      standings[match.HomeTeam].GoalsAgainst += match.AwayGoals ?? 0;
      standings[match.HomeTeam].GoalsDifference = standings[match.HomeTeam].GoalsFor
        - standings[match.HomeTeam].GoalsAgainst;
      standings[match.HomeTeam].Points += match.HomeGoals > match.AwayGoals ?
        3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);

      standings[match.AwayTeam].GoalsFor += match.AwayGoals ?? 0;
      standings[match.AwayTeam].GoalsAgainst += match.HomeGoals ?? 0;
      standings[match.AwayTeam].GoalsDifference = standings[match.AwayTeam].GoalsFor
        - standings[match.AwayTeam].GoalsAgainst;
      standings[match.AwayTeam].Points += match.AwayGoals > match.HomeGoals ?
        3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);
    }

    /// <summary> Calculate order of teams in the league table after the given round</summary>
    private static List<TeamLeagueStanding> GetTeamsOrder(
      Dictionary<string, TeamLeagueStanding> standings)
    {
      var result = standings.Values
        .OrderBy(standing => standing.Points)
        .ThenBy(standing => standing.GoalsDifference)
        .ThenBy(standing => standing.GoalsFor).ToList();

      result.Reverse();
      return result;
    }

    /// <summary> Set positions of teams in the league table after the given round</summary>
    private static void SetTeamTablePositions(IEnumerable<TeamLeagueStanding> standingsInOrder,
      int matchRound)
    {
      for (int i = 0; i < standingsInOrder.Count(); i++)
      {
        var teamName = standingsInOrder.ElementAt(i).Team;

        if (!TablePositions.ContainsKey(teamName))
          TablePositions[teamName] = new Dictionary<int, int>();

        // Position int the table of the given team after matchRound
        TablePositions[teamName][matchRound] = i + 1;
      }
    }

    public void CalculateRoundPoints()
    {
      if (TeamName == DefaultTeamName)
        return;

      lock (_lockObject)
      {
        if (MatchesDataUpdated)
        {
          MatchesDataUpdated = false;
          CalcTablePositions = Task.Run(() => CalculateTablePositions());
        }
      }
      CalcTablePositions?.Wait();

      int roundPoints = 0;
      int matchRound = 0, prevMatchRound = 0;
      string opponent;

      foreach (var match in SelectedMatches.OrderBy(match => match.Round ?? 0))
      {
        matchRound = match.Round ?? 0;
        if (prevMatchRound == matchRound || matchRound == 0)
          continue;

        if (match.HomeTeam == TeamName)
        {
          roundPoints += match.HomeGoals > match.AwayGoals ? 3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);
          opponent = match.AwayTeam;
        }
        else
        {
          roundPoints += match.HomeGoals < match.AwayGoals ? 3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);
          opponent = match.HomeTeam;
        }

        var matchDate = match.Date.Value.ToUniversalTime()
          .ToString("ddd, dd.MM.yyyy HH:mm", CultureInfo.CreateSpecificCulture("en-gb"));

        var roundResult = new RoundResult
        {
          Points = roundPoints,
          TablePosition = TablePositions[TeamName][matchRound],
          Opponent = opponent,
          OpposingTeams = match.HomeTeam + " - " + match.AwayTeam,
          MatchResult = match.HomeGoals + ":" + match.AwayGoals,
          MatchDate = matchDate,
        };

        if (!RoundPoints.ContainsKey(matchRound))
          RoundPoints.Add(matchRound, roundResult);

        prevMatchRound = matchRound;
      }
    }

    public void CreateTeamsSelectList()
    {
      var teamNames = new HashSet<string>(AllMatches.Select(match => match.HomeTeam)).ToList();
      teamNames.Sort();

      for (int i = 0; i < teamNames.Count; i++)
      {
        var teamItem = new SelectListItem
        {
          Value = (i + 1).ToString(),
          Text = teamNames[i],
        };
        TeamItems.Add(teamItem);
      }

      TeamsSelectList = new MultiSelectList(TeamItems, "Value", "Text");
    }

    public void CreateRoundsSelectList()
    {
      var teamRounds = new HashSet<int>(AllMatches.Select(match => match.Round ?? 1)).ToList();
      teamRounds.Sort();

      for (int i = 0; i < teamRounds.Count; i++)
      {
        var roundItem = new SelectListItem
        {
          Value = teamRounds[i].ToString(),
          Text = teamRounds[i].ToString(),
        };
        RoundItems.Add(roundItem);
      }

      RoundsSelectList = new MultiSelectList(RoundItems, "Value", "Text", RoundsNumbersInts);
    }

    public void SetRoundsRange(string startRound, string endRound)
    {
      StartRound = startRound;
      EndRound = endRound;
      GenerateUserSelectedRounds();
      SetSelectedMatches();
    }

    public void SetRounds(IEnumerable<int> rounds)
    {
      RoundsNumbersInts = rounds?.ToList() ?? new List<int>();
      SetSelectedMatches();
    }

    public void SetMatches(IEnumerable<FootballMatch> matches)
    {
      AllMatches = matches?.ToList() ?? new List<FootballMatch>();
      LeagueRoundsNumber = AllMatches.Max(x => x.Round) ?? LeagueRoundsNumber;
    }

    private void SetSelectedMatches()
    {
      AllMatches.Reverse();

      SelectedMatches = AllMatches
        .Where(x => RoundsNumbersInts.Contains(x.Round ?? 1)).ToList();

      if (TeamName != DefaultTeamName)
      {
        SelectedMatches = SelectedMatches
          .Where(x => x.HomeTeam == TeamName || x.AwayTeam == TeamName).ToList();
      }
    }

    private void GenerateUserSelectedRounds()
    {
      if (!int.TryParse(StartRound, out int startRoundInt))
        startRoundInt = 1;

      if (!int.TryParse(EndRound, out int endRoundInt))
        endRoundInt = LeagueRoundsNumber;

      if (startRoundInt > 0 && endRoundInt >= startRoundInt)
      {
        RoundsNumbersInts = Enumerable
          .Range(startRoundInt, endRoundInt - startRoundInt + 1)
          .ToList();
      }
    }

    public class RoundResult
    {
      public int Points { get; set; }
      public int TablePosition { get; set; }
      public string Opponent { get; set; }
      public string OpposingTeams { get; set; }
      public string MatchResult { get; set; }
      public string MatchDate { get; set; }
    };
  }
}