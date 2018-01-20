namespace SportsAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StringLengthChange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.FootballTeams", "Name", c => c.String(nullable: false, maxLength: 80));
            AlterColumn("dbo.FootballTeams", "Country", c => c.String(nullable: false, maxLength: 60));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FootballTeams", "Country", c => c.String(nullable: false));
            AlterColumn("dbo.FootballTeams", "Name", c => c.String(nullable: false));
        }
    }
}
