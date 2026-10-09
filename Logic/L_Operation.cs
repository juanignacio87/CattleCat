using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class L_Operation
    {
        Dalc.D_Operation dOperation = new Dalc.D_Operation();
        Entities.Model db = new Entities.Model();

        public List<Entities.Operation> GetAll()
        {
            return dOperation.GetAll();
        }
                
    }
}
