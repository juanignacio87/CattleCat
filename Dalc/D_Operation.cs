using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Dalc
{
    public class D_Operation
    {
        Entities.Model db = new Entities.Model();

        public List<Entities.Operation> GetAll()
        {
            var listOperation = from o in db.Operations
                                orderby o.OperationName
                                select o;
            return listOperation.ToList();
        }

        public DbSet<Entities.Operation> Operations{ get; set; }
        
    }
}
