using Dalc;
using Entities;
using System.Collections.Generic;

namespace Logic
{
    public class L_Category
    {
        D_Category dCategory = new Dalc.D_Category();

        public List<Category> GetAll()
        {
            return dCategory.GetAll();
        }
    }
}
