using Dalc;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic
{
    public class L_Record
    {
        private readonly D_Record dRecord = new D_Record();

        #region CREATE

        public void LRecord(Record record)
        {
            dRecord.d_Record(record);
        }

        public void LRecords(IEnumerable<Entities.Record> records)
        {
            Dalc.D_Record dRecord = new Dalc.D_Record();

            dRecord.AddRecords(records);
        }

        #endregion
        #region READ

        public object L_GetAll()
        {
            return dRecord.GetRecords();
        }
        
        public IEnumerable<object> LBuscar(string buscar)
        {
            return dRecord.Buscar(buscar).ToList();
        }
         
        public Record GetRecordToEdit(int id)
        {
            return dRecord.GetRecordToEdit(id);
        }

        public (bool isValid, DateTime? conflictDate, int projectedStock)ValidateStockTimeline(int lotId, int categoryId, DateTime newDate, int newEffect, int? excludedRecordId = null)
        {
            return dRecord.ValidateStockTimeline(lotId, categoryId, newDate, newEffect, excludedRecordId);
        }

        public (Dictionary<int, int>, int) GetTotal2(int farmId, int lotId)
        {
            return dRecord.GetTotal2(farmId, lotId);
        }

        public int GetStock(int lotId, int categoryId, DateTime untilDate, int? excludedRecordId = null)
        {
            return dRecord.GetStock(lotId, categoryId, untilDate, excludedRecordId);
        }

        public (int totalStock, int farms, int lots, int activeRecords, string lastRecord)GetDashboardSummary()
        {
            return dRecord.GetDashboardSummary();
        }

        #endregion
        #region UPDATE
        public void Update(Entities.Record record)
        {
            dRecord.UpdateOne(record);
        }

        #endregion
        #region DELETE

        public bool Delete(int id)
        {
            Dalc.D_Record dRecord = new Dalc.D_Record();
            return dRecord.Delete(id);
        }

        public (bool canDelete, DateTime? conflictDate, int projectedStock)CanDeactivateRecord(int id)
        {
            Dalc.D_Record dRecord = new Dalc.D_Record();
            return dRecord.CanDeactivateRecord(id);
        }

        #endregion
    }
}
