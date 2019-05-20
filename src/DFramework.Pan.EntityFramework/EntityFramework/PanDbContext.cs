using Abp.EntityFramework;
using DFramework.Pan.Domain;
using System.Data.Common;
using System.Data.Entity;

namespace DFramework.Pan.EntityFramework
{
    public class PanDbContext : AbpDbContext
    {
        //TODO: Define an IDbSet for each Entity...

        //Example:
        //public virtual IDbSet<User> Users { get; set; }

        /* NOTE:
         *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
         *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
         *   pass connection string name to base classes. ABP works either way.
         */

        public PanDbContext()
            : base("MyPanConnectionString")
        {
        }

        /* NOTE:
         *   This constructor is used by ABP to pass connection string defined in PanDataModule.PreInitialize.
         *   Notice that, actually you will not directly create an instance of PanDbContext since ABP automatically handles it.
         */

        public PanDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        //This constructor is used in tests
        public PanDbContext(DbConnection existingConnection)
         : base(existingConnection, false)
        {
        }

        public PanDbContext(DbConnection existingConnection, bool contextOwnsConnection)
         : base(existingConnection, contextOwnsConnection)
        {
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<Quota> Quotas { get; set; }
        public DbSet<QuotaLog> QuotaLogs { get; set; }
        public DbSet<ZipLog> ZipLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().ToTable("p_Node");
            modelBuilder.Entity<Quota>().ToTable("p_Quota");
            modelBuilder.Entity<QuotaLog>().ToTable("p_QuotaLog");
            modelBuilder.Entity<ZipLog>().ToTable("p_ZipLog");
            base.OnModelCreating(modelBuilder);
        }
    }
}