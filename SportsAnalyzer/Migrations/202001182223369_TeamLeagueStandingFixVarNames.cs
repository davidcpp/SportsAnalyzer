namespace SportsAnalyzer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class TeamLeagueStandingFixVarNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamLeagueStandings", "GoalsFor", c => c.Int(nullable: false));
            AddColumn("dbo.TeamLeagueStandings", "GoalsAgainst", c => c.Int(nullable: false));
            AddColumn("dbo.TeamLeagueStandings", "GoalsDifference", c => c.Int(nullable: false));
            DropColumn("dbo.TeamLeagueStandings", "Goals_For");
            DropColumn("dbo.TeamLeagueStandings", "Goals_Against");
            DropColumn("dbo.TeamLeagueStandings", "Goal_Difference");
        }

        public override void Down()
        {
            AddColumn("dbo.TeamLeagueStandings", "Goal_Difference", c => c.Int(nullable: false));
            AddColumn("dbo.TeamLeagueStandings", "Goals_Against", c => c.Int(nullable: false));
            AddColumn("dbo.TeamLeagueStandings", "Goals_For", c => c.Int(nullable: false));
            DropColumn("dbo.TeamLeagueStandings", "GoalsDifference");
            DropColumn("dbo.TeamLeagueStandings", "GoalsAgainst");
            DropColumn("dbo.TeamLeagueStandings", "GoalsFor");
        }
    }
}
