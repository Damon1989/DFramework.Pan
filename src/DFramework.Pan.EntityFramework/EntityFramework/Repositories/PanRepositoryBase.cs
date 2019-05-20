using Abp.Domain.Entities;
using Abp.EntityFramework;
using Abp.EntityFramework.Repositories;

namespace DFramework.Pan.EntityFramework.Repositories
{
    public abstract class PanRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<PanDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected PanRepositoryBase(IDbContextProvider<PanDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        //add common methods for all repositories
    }

    public abstract class PanRepositoryBase<TEntity> : PanRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected PanRepositoryBase(IDbContextProvider<PanDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        //do not add any method here, add to the class above (since this inherits it)
    }
}