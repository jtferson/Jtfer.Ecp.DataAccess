using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    public abstract class DataAccessContextBase : PipelineContext
    {
        private DbRouter dbRouter;
        public DataAccessContextBase(Domain domain, bool isActive = true, string name = null) : base(domain, isActive, name)
        {
        }

        protected override void AddContainers()
        {
            AddContainer<DataMemoryProvider>();
            dbRouter = AddContainer<DbRouter>();
            AddContainer<DataGateway>();
            var providers = DefineDbConnections();
            dbRouter.SetProviders(providers.ToArray());
        }
        public void CheckDatabases(Action<bool> callback)
        {
            dbRouter.LoadDatabases(callback);
        }
        public abstract IEnumerable<DbConnectionBase> DefineDbConnections();

    }
}
