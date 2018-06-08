using System;
using System.Data.Entity;

namespace SportsAnalyzer
{
  public static class Common
  {
    /* Constant Fields*/

    public const string DefaultLeagueShortName = "SPL";
    public const string DefaultLeagueFullName = "Scottish Premier League";
    public const string DefaultLeagueId = "3";
    public const int DefaultSeasonYear = 2017;
    public const int DefaultRoundsNumber = 33;
    public const int RequestsBreakMinutes = 5;
    public const int RequestsBreakSeconds = 15;

    public const double DefaultMatchTime = 90.0;
    public const double DefaultNumberOfMatchIntervals = 6.0;
    public const string DefaultTeamName = "*";
    public const string DefaultLeagueName = "*";

    /* Fields */

    public static DateTime LastUpdateTime;
    public static DateTime MatchesLastUpdateTime;
    public static DateTime TableLastUpdateTime;
    public static DateTime TeamsLastUpdateTime;

    public static void ClearDBSet(DbSet dbList)
    {
      foreach (var dbItem in dbList)
      {
        dbList.Remove(dbItem);
      }
    }
  }
}