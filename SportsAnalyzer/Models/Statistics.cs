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

    [Display(Name = "Chose rounds")]
    public MultiSelectList RoundsSelectList { get; private set; }

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

    /* Methods */

    public void CalculateAll()
    {
      if (SelectedMatches == null || SelectedMatches.Count == 0)
      {
        return;
      }

      MatchesNumber = SelectedMatches.Count;
      GoalsSum = SelectedMatches.Sum((match) => match.HomeGoals.Value + match.AwayGoals.Value);
      GoalsAvg = Round(GoalsSum / MatchesNumber, 2);
      GoalsAvgHome = Round(SelectedMatches.Average((match) => match.HomeGoals.Value), 2);
      GoalsAvgAway = Round(SelectedMatches.Average((match) => match.AwayGoals.Value), 2);

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
      }

      for (int i = 0; i < NumberOfMatchIntervals; i++)
      {
        GoalsInIntervalsPercent[i] = Round((GoalsInIntervals[i] / GoalsSum) * 100, 2);
      }
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
      RoundsNumbersInts = rounds.ToList();
      SetSelectedMatches();
    }

    public void SetMatches(IEnumerable<FootballMatch> matches)
    {
      AllMatches = matches.ToList();
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
  }

  public class Round
  {
    [Required]
    public int Number { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }
}