using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalc
{
    public class D_Category
    {
        Model db = new Model();

        public List<Category> GetAll()
        {
            var listCategory = from c in db.Categories
                               orderby c.CategoryName
                               select c;
            return listCategory.ToList();
        }

        public Object GetAll(Record farm, Record lot)
        {
            var listCategory = from r in db.Records
                               from l in db.Lots
                               where l.Farm.Id == farm.Id || l.Id == lot.Id
                               select new
                               {
                                   Category = r.CategoryId
                                   //TotalC = r.Total((r.Operation.Id(7).))
                               };
            return listCategory;
        }


    }
}
