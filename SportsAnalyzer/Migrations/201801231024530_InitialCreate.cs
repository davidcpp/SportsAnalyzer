namespace SportsAnalyzer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FootballTeams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Team_Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 80),
                        Country = c.String(nullable: false, maxLength: 60),
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
                        Team = c.String(nullable: false, maxLength: 80),
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
