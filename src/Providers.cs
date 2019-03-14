using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    public abstract class DataProviderBase : IContainer
    {
        public abstract IEnumerable<TChild> GetData<TChild>()
            where TChild : DbObject, new();
        public abstract IEnumerable<TChild> GetDataById<TChild>(IEnumerable<Guid> ids)
            where TChild : DbObject, new();
        public abstract IEnumerable<TChild> AddRange<TChild>(IEnumerable<TChild> objs)
            where TChild : DbObject, new();
        public abstract IEnumerable<TChild> UpdateRange<TChild>(IEnumerable<TChild> objs)
           where TChild : DbObject, new();
        public abstract void RemoveRange<TChild>(IEnumerable<TChild> objs)
            where TChild : DbObject, new();
        public abstract void RemoveAll<TChild>()
            where TChild : DbObject, new();

        public abstract void ResetTable<TChild>()
            where TChild : DbObject, new();
    }



    public class DataMemoryProvider : DataProviderBase
    {
        private Dictionary<Type, List<DbObject>> map = new Dictionary<Type, List<DbObject>>();

        public override IEnumerable<T> GetData<T>()
        {
            return GetData(typeof(T)).OfType<T>();
        }

        public IEnumerable<DbObject> GetData(Type type)
        {
            if (map.Any(q => q.Key == type))
                return map.FirstOrDefault(q => q.Key == type).Value;
            return Enumerable.Empty<DbObject>();
        }

        public override IEnumerable<T> AddRange<T>(IEnumerable<T> objs)
        {
            AddRange(objs.OfType<DbObject>(), typeof(T));
            return objs;
        }

        public IEnumerable<DbObject> AddRange(IEnumerable<DbObject> objs, Type childType)
        {
            if (!map.ContainsKey(childType))
                map.Add(childType, new List<DbObject>());
            map[childType].AddRange(objs);
            return objs;
        }

        public override IEnumerable<T> UpdateRange<T>(IEnumerable<T> objs)
        {
            UpdateRange(objs.OfType<DbObject>(), typeof(T));
            return objs;
        }

        public IEnumerable<DbObject> UpdateRange(IEnumerable<DbObject> objs, Type childType)
        {
            if (map.ContainsKey(childType))
            {
                var ids = objs.Select(q => q.Id);
                map[childType] = map[childType].Where(q => !ids.Contains(q.Id)).ToList();
                map[childType].AddRange(objs);
            }
            return objs;
        }

        public void RemoveRange<T>(IEnumerable<Guid> ids)
            where T : DbObject
        {
            RemoveRange(ids, typeof(T));
        }

        public void RemoveRange(IEnumerable<Guid> ids, Type childType)
        {
            if (map.Any(q => q.Key == childType))
            {
                foreach (var id in ids)
                {
                    var index = GetObjIndex(id, childType);
                    map.FirstOrDefault(q => q.Key == childType).Value.RemoveAt(index);
                }
            }
        }

        private int GetObjIndex<T>(Guid id)
           where T : DbObject
        {
            return GetObjIndex(id, typeof(T));
        }

        private int GetObjIndex(Guid id, Type childType)
        {
            return map.FirstOrDefault(q => q.Key == childType).Value.FindIndex(q => q.Id == id);
        }

        /// <summary>
        /// Проверяет инициализированы ли данные
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsInitialized<T>()
        {
            return IsInitialized(typeof(T));
        }

        public bool IsInitialized(Type type)
        {
            return map.Any(q => q.Key == type);
        }

        public override IEnumerable<TChild> GetDataById<TChild>(IEnumerable<Guid> ids)
        {
            return Enumerable.Empty<TChild>();
        }

        public override void RemoveRange<TChild>(IEnumerable<TChild> objs)
        {
        }

        public override void RemoveAll<TChild>()
        {
        }

        public override void ResetTable<TChild>()
        {
        }
    }


    public class DbRouter : DataProviderBase
    {
        private DbConnectionBase[] providers;
        private Dictionary<Type, DbConnectionBase> mapping;

        private DbConnectionBase cachedProvider;
        private Type lastType;

        internal void SetProviders(params DbConnectionBase[] providers)
        {
            this.providers = providers;
        }
        public override IEnumerable<TChild> AddRange<TChild>(IEnumerable<TChild> objs)
        {
            var provider = GetProvider<TChild>();
            AddTransaction(() =>
            {
                provider.Insert(objs);
            });
            return objs;
        }

        public override IEnumerable<TChild> UpdateRange<TChild>(IEnumerable<TChild> objs)
        {
            var provider = GetProvider<TChild>();
            foreach (var obj in objs)
            {
                AddTransaction(() =>
                {
                    provider.Update(obj);
                });
            }
            return objs;
        }

        public override IEnumerable<TChild> GetData<TChild>()
        {
            var provider = GetProvider<TChild>();
            var table = provider.Get<TChild>();
            return table;
        }

        public override IEnumerable<TChild> GetDataById<TChild>(IEnumerable<Guid> dbIds)
        {
            var provider = GetProvider<TChild>();
            return provider.Get<TChild>(q => dbIds.Contains(q.Id));
        }


        public override void RemoveRange<TChild>(IEnumerable<TChild> objs)
        {
            var provider = GetProvider<TChild>();
            foreach (var obj in objs)
                AddTransaction(() =>
                {
                    provider.Delete<TChild>(obj.Id);

                });
        }


        public override void RemoveAll<TChild>()
        {
            var provider = GetProvider<TChild>();
            AddTransaction(() =>
            {
                provider.DeleteAll<TChild>();
            });
        }

        public override void ResetTable<TChild>()
        {
           
        }

        public void SaveChanges()
        {
            foreach (var provider in providers)
                provider.ExecuteTransactions();
        }

        private void AddTransaction(Action transaction)
        {
            cachedProvider.AddTransaction(transaction);
        }

        private DbConnectionBase GetProvider<T>()
            where T : DbObject
        {
            return GetProvider(typeof(T));
        }

        private DbConnectionBase GetProvider(Type type)
        {
            if (mapping == null)
            {
                mapping = providers.SelectMany(q => q.GetMappedTypes(), (q, x) => new { prov = q, type = x })
                    .ToDictionary(q => q.type, q => q.prov);
            }
#if DEBUG
            if (providers.Length == 0)
            {
                throw new Exception("Provider count is 0");
            }
            if (mapping.Count == 0)
            {
                throw new Exception("Mapping is empty");
            }
#endif
            if (lastType == type)
                return cachedProvider;
            lastType = type;
#if DEBUG
            if (!mapping.ContainsKey(lastType))
            {
                throw new Exception(string.Format("Mapping doesn't contain \"{0}\" type", lastType));
            }
#endif
            cachedProvider = mapping[lastType];
            return cachedProvider;
        }
    }
}
