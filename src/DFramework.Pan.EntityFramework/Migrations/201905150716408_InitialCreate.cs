namespace DFramework.Pan.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.p_Node",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    OwnerId = c.String(),
                    Name = c.String(),
                    Path = c.String(),
                    IsDeleted = c.Boolean(nullable: false),
                    ParentId = c.String(maxLength: 128),
                    Tags = c.String(),
                    CreationTime = c.DateTime(nullable: false),
                    Size = c.Long(),
                    StorageFileId = c.String(),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.p_Node", t => t.ParentId)
                .Index(t => t.ParentId);

            CreateTable(
                "dbo.p_QuotaLog",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    OwnerId = c.String(),
                    Size = c.Long(nullable: false),
                    FileId = c.String(),
                    AppId = c.String(),
                    CreationTime = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.p_Quota",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    OwnerId = c.String(),
                    Max = c.Long(nullable: false),
                    Used = c.Long(nullable: false),
                    CreationTime = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.p_ZipLog",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    ZipKey = c.String(),
                    NodeId = c.String(),
                    InclusionIds = c.String(),
                    CreationTime = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.p_Node", "ParentId", "dbo.p_Node");
            DropIndex("dbo.p_Node", new[] { "ParentId" });
            DropTable("dbo.p_ZipLog");
            DropTable("dbo.p_Quota");
            DropTable("dbo.p_QuotaLog");
            DropTable("dbo.p_Node");
        }
    }
}