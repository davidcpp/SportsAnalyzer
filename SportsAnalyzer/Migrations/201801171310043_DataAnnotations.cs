namespace SportsAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataAnnotations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FootballTeams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Team_Id = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        Country = c.String(nullable: false),
                        Stadium = c.String(),
                        HomePageURL = c.String(),
                        WIKILink = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeamLeagueStandings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Team_Id = c.Int(nullable: false),
                        Team = c.String(nullable: false),
                        Played = c.Int(nullable: false),
                        Points = c.Int(nullable: false),
                        Won = c.Int(nullable: false),
                        Draw = c.Int(nullable: false),
                        Lost = c.Int(nullable: false),
                        Goals_For = c.Int(nullable: false),
                        Goals_Against = c.Int(nullable: false),
                        Goal_Difference = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TeamLeagueStandings");
            DropTable("dbo.FootballTeams");
        }
    }
}
