using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalc
{
    public class D_Lot
    {
        private readonly Entities.Model db = new Entities.Model();

        #region CREATE

        public bool Create(int farmId, string lotName)
        {
            string normalizedName = lotName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            bool farmExists = db.Farms.Any(f => f.Id == farmId && f.Activo == 1);

            if (!farmExists)
            {
                return false;
            }

            bool alreadyExists = db.Lots.Any(l => l.FarmId == farmId && l.Active == 1 && l.LotName.ToLower() == normalizedName.ToLower());

            if (alreadyExists)
            {
                return false;
            }

            Entities.Lot lot = new Entities.Lot
            {
                FarmId = farmId,
                LotName = normalizedName,
                Active = 1
            };

            db.Lots.Add(lot);
            db.SaveChanges();

            return true;
        }

        #endregion

        #region READ

        public List<Entities.Lot> GetAll()
        {
            return db.Lots
                .Where(l =>
                    l.Active == 1 &&
                    l.Farm.Activo == 1)
                .OrderBy(l => l.LotName)
                .ToList();
        }

        public List<Entities.Lot> GetAll(int farmId)
        {
            return db.Lots
                .Where(l =>
                    l.FarmId == farmId &&
                    l.Active == 1)
                .OrderBy(l => l.LotName)
                .ToList();
        }

        public Entities.Lot GetById(int lotId)
        {
            return db.Lots.FirstOrDefault(l =>
                l.Id == lotId &&
                l.Active == 1);
        }

        /*
         * Se conserva porque otras partes del proyecto
         * podrían seguir utilizándolo.
         */
        public Entities.Lot GetFarmToEdit(int idLot)
        {
            return GetById(idLot);
        }

        #endregion

        #region UPDATE

        public bool Update(int lotId, string newName)
        {
            string normalizedName = newName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            Entities.Lot existingLot = db.Lots.FirstOrDefault(l => l.Id == lotId && l.Active == 1);

            if (existingLot == null)
            {
                return false;
            }

            bool duplicatedName = db.Lots.Any(l => l.Active == 1 && l.FarmId == existingLot.FarmId && l.Id != lotId && l.LotName.ToLower() == normalizedName.ToLower());

            if (duplicatedName)
            {
                return false;
            }

            existingLot.LotName = normalizedName;

            db.SaveChanges();

            return true;
        }

        #endregion

        #region DELETE

        public (bool success, string message) Deactivate(int lotId)
        {
            Entities.Lot lot = db.Lots.FirstOrDefault(l => l.Id == lotId && l.Active == 1);

            if (lot == null)
            {
                return (false, "El lote no existe o ya está inactivo.");
            }

            int stock = db.Records.Where(r =>
                                        r.Activo == 1 &&
                                        r.LotId == lotId &&
                                        (
                                            r.OperationId == 8 ||
                                            r.OperationId == 12 ||
                                            r.OperationId == 14 ||
                                            r.OperationId == 11
                                        ))
                                    .Select(r =>
                                        r.OperationId == 8 ||
                                        r.OperationId == 12
                                            ? r.Total
                                            : -r.Total)
                                    .DefaultIfEmpty(0)
                                    .Sum();

            if (stock != 0)
            {
                return (false, $"No se puede eliminar el lote porque su stock actual es {stock}.");
            }

            lot.Active = 0;

            db.SaveChanges();

            return (true, "Lote eliminado correctamente.");
        }

        #endregion
    }
}
