namespace SportsAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FootballMatchFix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FootballMatches", "Date", c => c.DateTime());
            AddColumn("dbo.FootballMatches", "AwayGoalDetails", c => c.String());
            AddColumn("dbo.FootballMatches", "HomeGoalDetails", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FootballMatches", "HomeGoalDetails");
            DropColumn("dbo.FootballMatches", "AwayGoalDetails");
            DropColumn("dbo.FootballMatches", "Date");
        }
    }
}
