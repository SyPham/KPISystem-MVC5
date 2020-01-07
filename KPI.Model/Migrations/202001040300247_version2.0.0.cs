namespace KPI.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version200 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionPlans", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActionPlans", "Name");
        }
    }
}
