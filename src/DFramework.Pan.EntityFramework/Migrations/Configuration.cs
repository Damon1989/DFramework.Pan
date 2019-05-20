using System.Data.Entity.Migrations;

namespace DFramework.Pan.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Pan.EntityFramework.PanDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "MyPanConnectionString";
        }

        protected override void Seed(Pan.EntityFramework.PanDbContext context)
        {
            // This method will be called every time after migrating to the latest version.
            // You can add any seed data here...
        }
    }
}