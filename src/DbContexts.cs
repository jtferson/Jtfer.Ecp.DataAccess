using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    public class DbSet<T>
       where T : DbObject, new()
    {
        public DbSet(DbConnectionBase provider)
        {
            provider.MapEntityToTable<T>();
        }
    }
    [EcpInject]
    public abstract class DbContextBase<T> : IInitContainer
        where T : DbConnectionBase
    {
        protected T _ = null;

        public void Initialize()
        {
            DefineDbContext();
        }
        public void Destroy()
        {
        }
        protected abstract void DefineDbContext();
    }
}
