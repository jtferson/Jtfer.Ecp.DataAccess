using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    [EcpInject]
    public abstract class RepositoryBase : IContainer
    {
        protected DataGateway Gateway = null;
    }

    [EcpInject]
    public abstract class RepositoryBase<T> : RepositoryBase
        where T : DbObject, new()
    {

        public IEnumerable<T> GetData()
        {
            return Gateway.GetData<T>();
        }

        public T GetById(Guid id)
        {
            return Gateway.GetData<T>().FirstOrDefault(q => q.Id == id);
        }
        public IEnumerable<T> GetById(IEnumerable<Guid> ids)
        {
            return Gateway.GetData<T>().Where(q => ids.Contains(q.Id));
        }
        public void RemoveAll()
        {
            Gateway.RemoveAll<T>();
        }
        public void RemoveData(IEnumerable<T> objs)
        {
            Gateway.RemoveRange(objs);
        }

        public void RemoveData(T obj)
        {
            RemoveData(new[] { obj });
        }

        public void RemoveData(Guid id)
        {
            var data = GetById(id);
            if (data != null)
                RemoveData(data);
        }

        public void AddData(IEnumerable<T> objs)
        {
            Gateway.AddData(objs);
        }
        public void AddData(T obj)
        {
            AddData(new[] { obj });
        }

        public void UpdateData(IEnumerable<T> objs)
        {
            Gateway.UpdateData(objs);
        }

        public void UpdateData(T obj)
        {
            UpdateData(new[] { obj });
        }
        public void SaveChanges()
        {
            Gateway.SaveChanges();
        }
    }
}
