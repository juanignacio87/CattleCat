using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class L_Lot
    {
        private readonly Dalc.D_Lot dLot = new Dalc.D_Lot();

        public List<Entities.Lot> GetAll()
        {
            return dLot.GetAll();
        }

        public List<Entities.Lot> GetAll(int farmId)
        {
            return dLot.GetAll(farmId);
        }

        public Entities.Lot GetById(int lotId)
        {
            return dLot.GetById(lotId);
        }

        public Entities.Lot GetFarmToEdit(int idLot)
        {
            return dLot.GetFarmToEdit(idLot);
        }

        public bool Create(int farmId, string lotName)
        {
            return dLot.Create(farmId, lotName);
        }

        public bool Update(int lotId, string newName)
        {
            return dLot.Update(lotId, newName);
        }

        public (bool success, string message) Deactivate(int lotId)
        {
            return dLot.Deactivate(lotId);
        }
    }
}
