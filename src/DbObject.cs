using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Jtfer.Ecp.DataAccess
{
    [DataContract]
    public abstract class DbObject
    {
        [DataMember]
        public virtual Guid Id { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }

        public DbObject()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
        }
    }
}
