using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jtfer.Ecp.DataAccess
{
    public abstract class DbConnectionBase : IInitContainer
    {
        private Queue<Action> _transactions = new Queue<Action>();
        protected abstract string DatabaseName { get; }
        protected abstract string FileFormat { get; }
        protected string DbPath { get; private set; }
 
        public void Initialize()
        {
            var dbVersion = GetDbVersion();
            DbPath = GetDbPath(dbVersion);
        }

        public void Destroy()
        {
            if (_transactions != null)
                _transactions.Clear();
            _transactions = null;
        }

        public void ExecuteTransactions()
        {
            var transactions = _transactions.ToArray();
            if (transactions.Any())
            {
                RunInTransaction((Action)Delegate.Combine(transactions));
                _transactions.Clear();
            }
        }
        public void AddTransaction(Action transaction)
        {
            _transactions.Enqueue(transaction);
        }
        protected abstract string GetDbVersion();
        protected abstract string GetDbPath(string dbVersion);
        
        public abstract IEnumerable<T> Get<T>() where T : DbObject;
        public abstract IEnumerable<T> Get<T>(Expression<Func<T, bool>> query) where T : DbObject;
        public abstract void Insert<T>(IEnumerable<T> dtos) where T : DbObject;
        public abstract void Insert<T>(T dto) where T : DbObject;
        public abstract void Update<T>(IEnumerable<T> dtos) where T : DbObject;
        public abstract bool Update<T>(T dto) where T : DbObject;
        public abstract bool Upsert<T>(T dto) where T : DbObject;
        public abstract bool Delete<T>(Guid id) where T : DbObject;
        public abstract void DeleteAll<T>() where T : DbObject;
        public abstract void RunInTransaction(Action transaction);
        public abstract IEnumerable<Type> GetMappedTypes();
        public abstract void MapEntityToTable<T>() where T : DbObject;

    }
}
