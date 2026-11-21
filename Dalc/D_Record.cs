using System.Data.Entity;
using CattleCat.Domain.Records;
using Dalc.RecordWriting;
using CattleCat.Compatibility;
using CattleCat.Domain.Stock;
using Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dalc
{
    public class D_Record
    {
        #region ATRIBUTES
        private readonly Entities.Model db = new Entities.Model();
        #endregion
        #region CREATE
        /// <summary>
        /// 
        /// </summary>
        private readonly RecordWriter writer = new RecordWriter(() => new EfRecordWriteSession());

        public RecordWriteResult TryCreate(Record record) => writer.Create(record);
        public RecordWriteResult TryCreateMany(IEnumerable<Record> records) => writer.CreateMany(records);
        public RecordWriteResult TryUpdate(Record record) => writer.Update(record);
        public RecordWriteResult TryDeactivate(int id) => writer.Deactivate(id);

        public void d_Record(Record record) => RequireSuccess(TryCreate(record));
        public void AddRecords(IEnumerable<Record> records) => RequireSuccess(TryCreateMany(records));

        private static void RequireSuccess(RecordWriteResult result)
        {
            if (!result.IsSuccess) throw new RecordWriteException(result);
        }

        #endregion
        #region READ

        /// <summary>
        /// Method: The following method is for Get all the records to fill the datagrid
        /// Created by: Juani
        /// Date: 11/12/2023
        /// Last Update: 16/04/2024
        /// </summary>
        /// <returns>Each operation with date, user, lot, farm and categories</returns>
        public object GetRecords()
        {
            var getRecord = (from r in db.Records
                             from u in db.Users
                             from l in db.Lots
                             from c in db.Categories
                             from o in db.Operations
                             where r.UserId == u.Id
                             where r.LotId == l.Id
                             where r.CategoryId == c.Id
                             where r.OperationId == o.Id
                             where r.Activo == 1
                             orderby r.Date ascending
                             select new
                             {
                                 id = r.Id,
                                 date = r.Date,
                                 user = u.UserName,
                                 lot = l.LotName,
                                 farm = l.Farm.FarmName,
                                 category = c.CategoryName,
                                 operation = o.OperationName,
                                 total = r.Total
                             }).ToList();

            var formatedRecord = getRecord.Select(r => new
            {
                r.id,
                date = r.date.ToString("dd/MM/yyyy"),
                r.user,
                r.lot,
                r.farm,
                r.category,
                r.operation,
                r.total
            }).ToList();

            return formatedRecord;
        }

        /// <summary>
        /// Method: The following method is for Get a record to update it
        /// Created by: Juani
        /// Date: 11/12/2023
        /// Last Update: 16/04/2024
        /// </summary>
        /// <returns>Each operation with date, user, lot, farm and categories</returns>
        public Record GetRecordToEdit(int id)
        {
            return db.Records.AsNoTracking().FirstOrDefault(r => r.Id == id && r.Activo == 1);
        }

        public (bool isValid, DateTime? conflictDate, int projectedStock) ValidateStockTimeline(int lotId, int categoryId, DateTime newDate, int newEffect, int? excludedRecordId = null)
        {
            Entities.Record originalRecord = null;
            if (excludedRecordId.HasValue)
                originalRecord = db.Records.FirstOrDefault(r => r.Id == excludedRecordId.Value && r.Activo == 1);

            bool removesMovementFromThisGroup = originalRecord != null &&
                originalRecord.LotId == lotId && originalRecord.CategoryId == categoryId &&
                LegacyStockOperationCatalog.SignedQuantity(originalRecord.OperationId, originalRecord.Total) != 0;

            // Preserve the existing no-change shortcut, including its zero projected balance.
            if (newEffect == 0 && !removesMovementFromThisGroup)
                return (true, null, 0);

            int[] movementIds = LegacyStockOperationCatalog.GetMovementIds();
            var query = db.Records.Where(r => r.Activo == 1 && r.LotId == lotId &&
                r.CategoryId == categoryId && movementIds.Contains(r.OperationId));
            if (excludedRecordId.HasValue)
                query = query.Where(r => r.Id != excludedRecordId.Value);

            var movements = query.Select(r => new { r.Date, r.Total, r.OperationId }).ToList()
                .Select(r => new StockMovement(r.Date, r.Total, true,
                    LegacyStockOperationCatalog.Classify(r.OperationId))).ToList();

            // newEffect already contains the net effect of a multi-operation submission.
            // Zero needs no movement: omitting a zero day cannot change the running balance.
            if (newEffect != 0)
                movements.Add(new StockMovement(newDate, Math.Abs(newEffect), true,
                    newEffect > 0 ? StockEffect.Positive : StockEffect.Negative));

            var result = StockTimeline.Evaluate(movements);
            return (result.IsValid, result.ConflictDate, result.ProjectedStock);
        }

        public int GetStock(int lotId, int categoryId, DateTime untilDate, int? excludedRecordId = null)
        {
            DateTime endDate = untilDate.Date.AddDays(1);

            var query = db.Records.Where(r => r.Activo == 1 && r.LotId == lotId && r.CategoryId == categoryId && r.Date < endDate);

            if (excludedRecordId.HasValue)
            {
                query = query.Where(r =>
                    r.Id != excludedRecordId.Value);
            }

            int[] movementIds = LegacyStockOperationCatalog.GetMovementIds();
            var records = query.Where(r => movementIds.Contains(r.OperationId)).ToList();

            int entradas = records.Where(r => LegacyStockOperationCatalog.Classify(r.OperationId) == StockEffect.Positive).Sum(r => r.Total);
            int salidas = records.Where(r => LegacyStockOperationCatalog.Classify(r.OperationId) == StockEffect.Negative).Sum(r => r.Total);

            return entradas - salidas;
        }

        /// <summary>
        /// Event to looking for, with text change
        /// </summary>
        /// <param name="buscar"></param>
        /// <returns></returns>
        /// 
        public IEnumerable<object> Buscar(string buscar)
        {
            var get = (from r in db.Records
                       from u in db.Users
                       from l in db.Lots
                       from c in db.Categories
                       from o in db.Operations
                       where r.UserId == u.Id
                       where r.LotId == l.Id
                       where r.CategoryId == c.Id
                       where r.OperationId == o.Id
                       where r.Activo == 1
                       orderby r.Date ascending
                       select new
                       {
                           id = r.Id,
                           date = r.Date,
                           user = u.UserName,
                           lot = l.LotName,
                           farm = l.Farm.FarmName,
                           category = c.CategoryName,
                           operation = o.OperationName,
                           total = r.Total
                       }).ToList();

            var get2 = get.Where(x => x.user.ToLower().StartsWith(buscar.ToLower()) ||
                                      x.lot.ToLower().StartsWith(buscar.ToLower()) ||
                                      x.farm.ToLower().StartsWith(buscar.ToLower()) ||
                                      x.category.ToLower().StartsWith(buscar.ToLower()) ||
                                      x.operation.ToLower().StartsWith(buscar.ToLower())).ToList();


            var formatedRecord = get2.Select(r => new
            {
                r.id,
                date = r.date.ToString("dd/MM/yyyy"),
                r.user,
                r.lot,
                r.farm,
                r.category,
                r.operation,
                r.total
            }).ToList();

            return (IEnumerable<object>)formatedRecord.ToList();
        }

        /// <summary>
        /// Retrieves the total values grouped by category and the overall total for the specified farm and lot.
        /// </summary>
        /// <remarks>The totals for each category are calculated by summing the values of records with
        /// specific operation IDs  and subtracting the values of records with other operation IDs. The method filters
        /// records based on the  provided farm and lot identifiers.</remarks>
        /// <param name="farmId">The identifier of the farm to filter records by. Use 0 to include all farms.</param>
        /// <param name="lotId">The identifier of the lot to filter records by. Use 0 to include all lots.</param>
        /// <returns>A tuple containing: <list type="bullet"> <item> <description> A dictionary where the key is the category ID
        /// and the value is the calculated total for that category. </description> </item> <item> <description> The
        /// overall total across all categories. </description> </item> </list></returns>
        public (Dictionary<int, int> categories, int totalGral) GetTotal2(int farmId, int lotId)
        {
            var query = db.Records.Where(r => r.Activo == 1);

            if (farmId != 0)
            {
                query = query.Where(r => db.Lots.Any(l => l.Id == r.LotId && l.FarmId == farmId));
            }

            if (lotId != 0)
            {
                query = query.Where(r =>
                    r.LotId == lotId);
            }

            var records = query.ToList();



            var categories = records.GroupBy(r => r.CategoryId).ToDictionary(group => group.Key, group =>group.Where(r => LegacyStockOperationCatalog.Classify(r.OperationId) == StockEffect.Positive).Sum(r => r.Total) - group.Where(r => LegacyStockOperationCatalog.Classify(r.OperationId) == StockEffect.Negative).Sum(r => r.Total));

            int totalGral = categories.Values.Sum();

            return (categories, totalGral);
        }

        public (bool canDelete, DateTime? conflictDate, int projectedStock) CanDeactivateRecord(int recordId)
        {
            var record = db.Records.FirstOrDefault(r => r.Id == recordId && r.Activo == 1);

            if (record == null)
            {
                return (false, null, 0);
            }

            // Removing a negative or neutral operation cannot reduce stock.
            if (LegacyStockOperationCatalog.Classify(record.OperationId) != StockEffect.Positive)
                return (true, null, 0);

            int[] movementIds = LegacyStockOperationCatalog.GetMovementIds();
            var movements = db.Records.Where(r => r.Activo == 1 && r.Id != recordId &&
                r.LotId == record.LotId && r.CategoryId == record.CategoryId &&
                movementIds.Contains(r.OperationId))
                .Select(r => new { r.Date, r.Total, r.OperationId }).ToList()
                .Select(r => new StockMovement(r.Date, r.Total, true,
                    LegacyStockOperationCatalog.Classify(r.OperationId)));

            var result = StockTimeline.Evaluate(movements);
            return (result.IsValid, result.ConflictDate, result.ProjectedStock);
        }

        public (int totalStock, int farms, int lots, int activeRecords, string lastRecord)GetDashboardSummary()
        {
            var totals = GetTotal2(0, 0);

            int farms = db.Farms.Count(f => f.Activo == 1);

            int lots = db.Lots.Count(l => l.Active == 1);

            int activeRecords = db.Records.Count(r => r.Activo == 1);

            var last = (
                from r in db.Records
                join u in db.Users
                    on r.UserId equals u.Id
                join l in db.Lots
                    on r.LotId equals l.Id
                join c in db.Categories
                    on r.CategoryId equals c.Id
                join o in db.Operations
                    on r.OperationId equals o.Id
                where r.Activo == 1
                orderby r.Date descending, r.Id descending
                select new
                {
                    r.Date,
                    u.UserName,
                    l.LotName,
                    c.CategoryName,
                    o.OperationName,
                    r.Total
                })
                .FirstOrDefault();

            string lastRecord = last == null
                ? "Todavía no existen registros."
                : $"{last.Date:dd/MM/yyyy} · " +
                  $"{last.OperationName} · " +
                  $"{last.CategoryName} · " +
                  $"{last.Total} animales · " +
                  $"{last.LotName} · " +
                  $"{last.UserName}";

            return (totals.totalGral, farms, lots, activeRecords, lastRecord);
        }

        #endregion
        #region UPDATE

        /// <summary>
        /// Method: Update a record on the Record Table   
        /// Created by: Juani   
        /// Date: 13/11/23
        /// Last Update: 13/11/23
        /// </summary>
        /// <param name="id"></param>
        public void UpdateOne(Record record) => RequireSuccess(TryUpdate(record));

        #endregion
        #region DELETE
        /// <summary>
        /// Method: Delete a record on the Record Table   
        /// Created by: Juani   
        /// Date: 13/11/23
        /// Last Update: 13/11/23
        /// </summary>
        /// <param name="id"></param>
        /// 
        public bool Delete(int id)
        {
            var result = TryDeactivate(id);
            if (result.Status == RecordWriteStatus.TechnicalError) throw new RecordWriteException(result);
            return result.IsSuccess;
        }

        #endregion

    }
}
