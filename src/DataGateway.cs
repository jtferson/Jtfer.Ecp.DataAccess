using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    [EcpInject]
    public class DataGateway : IContainer
    {
        private DataMemoryProvider memory;
        private DbRouter db;

        public IEnumerable<T> GetData<T>()
            where T : DbObject, new()
        {
            var result = Enumerable.Empty<T>();
            if (!memory.IsInitialized<T>())
            {
                var objs = Enumerable.Empty<T>();
                objs = db.GetData<T>();
                if (!memory.GetData<T>().Any())
                    memory.AddRange(objs);
            }
            result = memory.GetData<T>();
            return result;
        }

        public IEnumerable<T> AddData<T>(IEnumerable<T> objs)
            where T : DbObject, new()
        {
            memory.AddRange(objs);
            db.AddRange(objs);
            return objs;
        }

        public T AddData<T>(T obj)
            where T : DbObject, new()
        {
            return AddData<T>(new[] { obj }).FirstOrDefault();
        }

        public IEnumerable<T> UpdateData<T>(IEnumerable<T> objs)
             where T : DbObject, new()
        {
            memory.UpdateRange(objs);
            db.UpdateRange(objs);
            return objs;
        }

        public void RemoveRange<T>(IEnumerable<T> objs)
           where T : DbObject, new()
        {
            memory.RemoveRange(objs);
            db.RemoveRange(objs);
        }

        public void RemoveAll<T>()
            where T : DbObject, new()
        {
            memory.RemoveAll<T>();
            db.RemoveAll<T>();
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
