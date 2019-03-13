using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    public abstract class DataAccessContextBase : PipelineContext
    {
        public DataAccessContextBase(Domain domain, bool isActive = true, string name = null) : base(domain, isActive, name)
        {
        }

        protected override void AddContainers()
        {
            AddContainer<DataMemoryProvider>();
            var dbRouter = AddContainer<DbRouter>();
            AddContainer<DataGateway>();
            var providers = DefineDbConnections();
            dbRouter.SetProviders(providers.ToArray());
        }
        public abstract IEnumerable<DbConnectionBase> DefineDbConnections();

    }
}
