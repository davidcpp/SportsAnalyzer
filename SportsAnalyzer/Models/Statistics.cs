using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
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
    public List<Round> RoundsNumbers { get; set; } = new List<Round>();
    public List<int> RoundsNumbersInts { get; set; } = new List<int>();
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

    // Number of matches with a given number of goals
    // MatchGoalsPct[numberOfGoals] = numberOfMatches
    public IDictionary<int, double> MatchGoalsPct = new Dictionary<int, double>();
    // Number of points in the given round for the given team
    // RoundPoints[numberOfRound] = numberOfPoints
    public IDictionary<int, RoundResult> RoundPoints = new Dictionary<int, RoundResult>();

    // TablePositions[teamName][roundNumber] = position of the team in the table after roundNumber
    public static IDictionary<string, Dictionary<int, int>> TablePositions
      = new Dictionary<string, Dictionary<int, int>>();

    private static bool TableCalculated = false;

    /* Methods */

    public void CalculateAll()
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

      var regexGoalTime = new Regex("\\d{1,}");
      foreach (var match in SelectedMatches)
      {
        var homeGoalDetails = match.HomeGoalDetails.Split(new char[] { ';' });
        var awayGoalDetails = match.AwayGoalDetails.Split(new char[] { ';' });

        foreach (var detailedGoal in homeGoalDetails)
        {
          if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
          {
            // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
            GoalsInIntervals[(int)Ceiling(goalTime / MatchIntervalLength) - 1]++;
          }
        }

        foreach (var detailedGoal in awayGoalDetails)
        {
          if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
          {
            // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
            GoalsInIntervals[(int)Ceiling(goalTime / MatchIntervalLength) - 1]++;
          }
        }

        int matchGoals = match.HomeGoals ?? 0;
        matchGoals += match.AwayGoals ?? 0;
        if (!MatchGoalsPct.ContainsKey(matchGoals))
          MatchGoalsPct.Add(matchGoals, 0);
        MatchGoalsPct[matchGoals]++;
      }

      for (int i = 0; i < NumberOfMatchIntervals; i++)
      {
        GoalsInIntervalsPercent[i] = Round((GoalsInIntervals[i] / GoalsSum) * 100, 2);
      }

      int index = 0;
      for (var i = 0; i < MatchGoalsPct.Count; i++)
      {
        index = MatchGoalsPct.Keys.ElementAt(i);
        MatchGoalsPct[index] = Round((MatchGoalsPct[index] / MatchesNumber) * 100, 2);
      }
    }

    /// <summary> Calculate team positions in the table for all rounds </summary>
    public void CalculateTablePositions()
    {
      var teamsNames = new HashSet<string>(AllMatches.Select(match => match.HomeTeam));

      int matchRound, prevMatchRound = 0, matchesInRoundCounter = 0;
      int maxMatchesInRound = teamsNames.Count / 2;
      var teamLeagueStandings = new Dictionary<string, TeamLeagueStanding>();

      var orderedMatches = AllMatches.OrderBy(match => match.Round ?? 0);
      int lastRound = orderedMatches.LastOrDefault()?.Round ?? 0;

      InitAllTeamsStandings(teamLeagueStandings, teamsNames);

      foreach (var match in orderedMatches)
      {
        matchRound = match.Round ?? 0;

        if (matchRound == 0)
          continue;

        if (matchRound != prevMatchRound)
          matchesInRoundCounter = 0;

        matchesInRoundCounter++;
        prevMatchRound = matchRound;

        UpdateStandingsAfterMatch(teamLeagueStandings, match);

        // Calculating teams positions after whole round 
        // or after each subsequent match in last round (for duration of the round case)
        if (matchesInRoundCounter < maxMatchesInRound && matchRound != lastRound)
        {
          continue;
        }

        // Calculating teams positions in the league table

        var standingsInOrder = GetTeamsOrder(teamLeagueStandings);
        SetTeamTablePositions(standingsInOrder, matchRound);
      }
      TableCalculated = true;
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
      standings[match.HomeTeam].Goals_For += match.HomeGoals ?? 0;
      standings[match.HomeTeam].Goals_Against += match.AwayGoals ?? 0;
      standings[match.HomeTeam].Goal_Difference = standings[match.HomeTeam].Goals_For
        - standings[match.HomeTeam].Goals_Against;
      standings[match.HomeTeam].Points += match.HomeGoals > match.AwayGoals ?
        3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);

      standings[match.AwayTeam].Goals_For += match.AwayGoals ?? 0;
      standings[match.AwayTeam].Goals_Against += match.HomeGoals ?? 0;
      standings[match.AwayTeam].Goal_Difference = standings[match.AwayTeam].Goals_For
        - standings[match.AwayTeam].Goals_Against;
      standings[match.AwayTeam].Points += match.AwayGoals > match.HomeGoals ?
        3 : (match.HomeGoals == match.AwayGoals ? 1 : 0);
    }

    /// <summary> Calculate order of teams in the league table after the given round</summary>
    private static List<TeamLeagueStanding> GetTeamsOrder(
      Dictionary<string, TeamLeagueStanding> standings)
    {
      var result = standings.Values
        .OrderBy(standing => standing.Points)
        .ThenBy(standing => standing.Goal_Difference)
        .ThenBy(standing => standing.Goals_For).ToList();

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

      if (!TableCalculated)
        CalculateTablePositions();

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

        var roundResult = new RoundResult
        {
          Points = roundPoints,
          TablePosition = TablePositions[TeamName][matchRound],
          Opponent = opponent,
          OpposingTeams = match.HomeTeam + " - " + match.AwayTeam,
          MatchResult = match.HomeGoals + ":" + match.AwayGoals
        };

        if (!RoundPoints.ContainsKey(matchRound))
          RoundPoints.Add(matchRound, roundResult);

        prevMatchRound = matchRound;
      }
    }

    public void CreateTeamsSelectList()
    {
      var teamNames = new HashSet<string>(
        AllMatches.Select(match => match.HomeTeam)).ToList();
      teamNames.Sort();

      for (int i = 0; i < teamNames.Count; i++)
      {
        var teamItem = new SelectListItem
        {
          Value = (i + 1).ToString(),
          Text = teamNames[i]
        };
        TeamItems.Add(teamItem);
      }

      TeamsSelectList = new MultiSelectList(TeamItems.OrderBy(item => item.Text),
        "Value",
        "Text");
    }

    public void CreateRoundsSelectList()
    {
      Dictionary<int, bool> roundsDictionary = new Dictionary<int, bool>();

      int prevMatchRound = 0;
      int currentMatchRound = AllMatches.Count > 0 ? (AllMatches[0].Round ?? 0) : 0;
      var startDate = AllMatches.Count > 0 ?
        (AllMatches[0].Date ?? DateTime.MinValue) : DateTime.MinValue;

      for (int i = 1; i < AllMatches.Count; i++)
      {
        prevMatchRound = currentMatchRound;
        currentMatchRound = AllMatches[i].Round ?? 1;

        if (currentMatchRound == prevMatchRound || roundsDictionary.ContainsKey(prevMatchRound))
          continue;

        RoundsNumbers.Add(new Round
        {
          Number = prevMatchRound,
          StartDate = startDate,
          EndDate = AllMatches[i - 1].Date ?? DateTime.MinValue
        });

        startDate = AllMatches[i].Date ?? DateTime.MinValue;
        roundsDictionary[prevMatchRound] = true;
      }

      RoundsNumbers.Add(new Round
      {
        Number = currentMatchRound,
        StartDate = startDate,
        EndDate = AllMatches.Count > 0 ?
          (AllMatches.Last().Date ?? DateTime.MinValue) : (DateTime.MinValue)
      });

      RoundsSelectList = new MultiSelectList(
        RoundsNumbers.ToList().OrderBy(x => x.Number),
        "Number",
        "Number",
        selectedValues: RoundsNumbersInts);
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
      LeagueRoundsNumber = AllMatches.Select(x => x.Round).Max() ?? LeagueRoundsNumber;
    }

    private void SetSelectedMatches()
    {
      AllMatches.Reverse();

      SelectedMatches = AllMatches.Select(x => x)
        .Where(x => RoundsNumbersInts.Contains(x.Round ?? 1)).ToList();

      if (TeamName != DefaultTeamName)
      {
        SelectedMatches = SelectedMatches.Select(x => x)
          .Where((x) => x.HomeTeam == TeamName || x.AwayTeam == TeamName).ToList();
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

    public class Round
    {
      [Required]
      public int Number { get; set; }

      public DateTime StartDate { get; set; }
      public DateTime EndDate { get; set; }
    }

    public class RoundResult
    {
      public int Points { get; set; }
      public int TablePosition { get; set; }
      public string Opponent { get; set; }
      public string OpposingTeams { get; set; }
      public string MatchResult { get; set; }
    };
  }
}