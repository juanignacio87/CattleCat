using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalc
{
    public class D_Farm
    {
        private readonly Entities.Model db = new Entities.Model();

        #region CREATE

        public bool Create(Entities.Farm farm)
        {
            if (farm == null)
            {
                throw new ArgumentNullException(nameof(farm));
            }

            string normalizedName = farm.FarmName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            bool alreadyExists = db.Farms.Any(f => f.Activo == 1 && f.FarmName.ToLower() == normalizedName.ToLower());

            if (alreadyExists)
            {
                return false;
            }

            farm.FarmName = normalizedName;
            farm.Activo = 1;

            db.Farms.Add(farm);
            db.SaveChanges();

            return true;
        }

        #endregion

        #region READ

        public List<Entities.Farm> GetAll()
        {
            return db.Farms.Where(f => f.Activo == 1).OrderBy(f => f.FarmName).ToList();
        }

        public Entities.Farm GetById(int id)
        {
            return db.Farms.FirstOrDefault(f =>f.Id == id && f.Activo == 1);
        }

        #endregion

        #region UPDATE

        public bool Update(int farmId, string newName)
        {
            string normalizedName = newName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            Entities.Farm existingFarm = db.Farms.FirstOrDefault(f => f.Id == farmId && f.Activo == 1);

            if (existingFarm == null)
            {
                return false;
            }

            bool duplicatedName = db.Farms.Any(f => f.Activo == 1 && f.Id != farmId && f.FarmName.ToLower() == normalizedName.ToLower());

            if (duplicatedName)
            {
                return false;
            }

            existingFarm.FarmName = normalizedName;

            db.SaveChanges();

            return true;
        }

        #endregion

        #region DELETE

        public (bool success, string message) Deactivate(int farmId)
        {
            Entities.Farm farm =db.Farms.FirstOrDefault(f =>f.Id == farmId && f.Activo == 1);

            if (farm == null)
            {
                return (
                    false,
                    "El establecimiento no existe o ya está inactivo.");
            }

            bool hasActiveLots = db.Lots.Any(l =>
                l.FarmId == farmId &&
                l.Active == 1);

            if (hasActiveLots)
            {
                return (
                    false,
                    "No se puede eliminar el establecimiento porque todavía tiene lotes activos.");
            }

            farm.Activo = 0;

            db.SaveChanges();

            return (
                true,
                "Establecimiento eliminado correctamente.");
        }

        #endregion
    }
}
