using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SportsAnalyzer.Models
{
  public class Statistics
  {
    /* Constant fields*/
    private const double _defaultMatchTime = 90.0;
    private const double _defaultNumberOfMatchIntervals = 6.0;

    /* Fields */

    public double GoalsAvg;
    public double GoalsAvgHome;
    public double GoalsAvgAway;
    public double GoalsSum;

    private double[] _goalsInIntervals = null;
    private double[] _goalsInIntervalsPercent = null;
    private double[] _timeIntervalsLimits = null;

    /* Constructors */

    public Statistics(double numberOfMatchIntervals = _defaultNumberOfMatchIntervals)
    {
      MatchTime = _defaultMatchTime;
      NumberOfMatchIntervals = numberOfMatchIntervals;
      MatchIntervalLength = MatchTime / NumberOfMatchIntervals;
    }

    /* Properties */

    public double NumberOfMatchIntervals { get; private set; }
    public double MatchIntervalLength { get; private set; }
    public double MatchTime { get; private set; }

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
      GoalsAvg = GoalsSum / xmlLeagueMatches.Count();
      GoalsAvgHome = xmlLeagueMatches.Average((match) => match.HomeGoals.Value);
      GoalsAvgAway = xmlLeagueMatches.Average((match) => match.AwayGoals.Value);

      var regexGoalTime = new Regex("\\d{1,}");
      foreach (var xmlMatch in xmlLeagueMatches)
      {
        string[] detailedGoals = xmlMatch.HomeGoalDetails;
        foreach (var detailedGoal in xmlMatch.HomeGoalDetails)
        {
          if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
          {
            // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
            GoalsInIntervals[(int)Math.Ceiling(goalTime / MatchIntervalLength) - 1]++;
          }
        }

        foreach (var detailedGoal in xmlMatch.AwayGoalDetails)
        {
          if (double.TryParse(regexGoalTime.Match(detailedGoal).Value, out double goalTime))
          {
            // surely there isn't IndexOutOfRangeException - goalTime is equal to 90 at most 
            GoalsInIntervals[(int)Math.Ceiling(goalTime / MatchIntervalLength) - 1]++;
          }
        }
      }

      for (int i = 0; i < NumberOfMatchIntervals; i++)
      {
        GoalsInIntervalsPercent[i] = (GoalsInIntervals[i] / GoalsSum) * 100;
      }
    }
  }
}