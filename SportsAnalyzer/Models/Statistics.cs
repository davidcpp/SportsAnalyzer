using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static System.Math;

namespace SportsAnalyzer.Models
{


  public class Statistics
  {
    /* Constant fields*/

    private const double _defaultMatchTime = 90.0;
    private const double _defaultNumberOfMatchIntervals = 6.0;
    private const string _defaultTeamName = "*";
    private const string _defaultLeagueName = "*";

    /* Fields */

    private double[] _goalsInIntervals = null;
    private double[] _goalsInIntervalsPercent = null;
    private double[] _timeIntervalsLimits = null;


    /* Constructors */

    public Statistics(string leagueName = _defaultLeagueName,
                      string teamName = _defaultTeamName,
                      double numberOfMatchIntervals = _defaultNumberOfMatchIntervals)
    {
      NumberOfMatchIntervals = numberOfMatchIntervals;
      MatchIntervalLength = MatchTime / NumberOfMatchIntervals;
      LeagueName = leagueName;
      TeamName = teamName;
    }

    /* Properties */

    public double NumberOfMatchIntervals { get; }
    public double MatchIntervalLength { get; }
    public double MatchTime { get; } = _defaultMatchTime;

    [Display(Name = "League name")]
    public string LeagueName { get; }
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
        if (_goalsInIntervals == null)
          _goalsInIntervals = new double[(int)NumberOfMatchIntervals];
        return _goalsInIntervals;
      }
      set { _goalsInIntervals = value; }
    }

    public double[] GoalsInIntervalsPercent
    {
      get
      {
        if (_goalsInIntervalsPercent == null)
          _goalsInIntervalsPercent = new double[(int)NumberOfMatchIntervals];
        return _goalsInIntervalsPercent;
      }
      set { _goalsInIntervalsPercent = value; }
    }

    public double[] TimeIntervalsLimits
    {
      get
      {
        if (_timeIntervalsLimits == null)
        {
          _timeIntervalsLimits = new double[(int)NumberOfMatchIntervals + 1];
          for (int i = 0; i <= NumberOfMatchIntervals; i++)
          {
            _timeIntervalsLimits[i] = (MatchTime / NumberOfMatchIntervals) * i;
          }
        }
        return _timeIntervalsLimits;
      }
      set { _timeIntervalsLimits = value; }
    }

    /* Methods */

    public void CalculateAll(IEnumerable<XMLSoccerCOM.Match> xmlLeagueMatches)
    {
      //// TODO: Why is it necessary to multiply by 2 to reach average number of goals?
      //goalsAvg = xmlLeagueMatches.Average((match) => (match.HomeGoals ?? 0 + match.AwayGoals ?? 0) * 2);
      //goalsAvg = xmlLeagueMatches.Average((match) => match.HomeGoals.Value + match.AwayGoals.Value);

      GoalsSum = xmlLeagueMatches.Sum((match) => match.HomeGoals.Value + match.AwayGoals.Value);
      GoalsAvg = Round(GoalsSum / xmlLeagueMatches.Count(), 2);
      GoalsAvgHome = Round(xmlLeagueMatches.Average((match) => match.HomeGoals.Value), 2);
      GoalsAvgAway = Round(xmlLeagueMatches.Average((match) => match.AwayGoals.Value), 2);
      MatchesNumber = xmlLeagueMatches.Count();

      var regexGoalTime = new Regex("\\d{1,}");
      foreach (var xmlMatch in xmlLeagueMatches)
      {
        string[] detailedGoals = xmlMatch.HomeGoalDetails;
        foreach (var detailedGoal in xmlMatch.HomeGoalDetails)
        {
          if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
          {
            // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
            GoalsInIntervals[(int)Ceiling(goalTime / MatchIntervalLength) - 1]++;
          }
        }

        foreach (var detailedGoal in xmlMatch.AwayGoalDetails)
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
  }
}