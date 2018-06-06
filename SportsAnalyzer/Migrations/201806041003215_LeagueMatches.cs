namespace SportsAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LeagueMatches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FootballMatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AwayTeam = c.String(),
                        AwayGoals = c.Int(),
                        Round = c.Int(),
                        League = c.String(),
                        HomeTeam = c.String(),
                        HomeGoals = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FootballMatches");
        }
    }
}
