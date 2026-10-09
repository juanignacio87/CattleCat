using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class L_Farm
    {
        private readonly Dalc.D_Farm dFarm = new Dalc.D_Farm();

        public List<Entities.Farm> GetAll()
        {
            return dFarm.GetAll();
        }

        public Entities.Farm GetById(int id)
        {
            return dFarm.GetById(id);
        }

        public bool Create(string farmName)
        {
            Entities.Farm farm = new Entities.Farm
                {
                    FarmName = farmName,
                    Activo = 1
                };

            return dFarm.Create(farm);
        }

        public bool Update(int farmId, string newName)
        {
            return dFarm.Update(farmId, newName);
        }

        public (bool success, string message) Deactivate(int farmId)
        {
            return dFarm.Deactivate(farmId);
        }
    }
}
