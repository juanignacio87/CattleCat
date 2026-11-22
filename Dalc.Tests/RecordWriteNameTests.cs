using System;
using System.Collections.Generic;
using System.Linq;
using CattleCat.Domain.Records;
using Dalc.RecordWriting;
using Entities;
using Logic;
using NUnit.Framework;

namespace Dalc.Tests
{
    [TestFixture]
    public class RecordWriteNameTests
    {
        private static readonly DateTime Start = new DateTime(2026, 11, 6);
        private MemoryWriteDatabase db;
        private RecordWriter writer;
        private readonly Dictionary<int, string> lots = new Dictionary<int, string>
        {
            [8] = "Prueba_F1B_LOTE", [9] = "Prueba_F1C_DESTINO"
        };
        private readonly Dictionary<int, string> categories = new Dictionary<int, string>
        {
            [2] = "Terneros", [1] = "Vacas"
        };

        private static Record R(int op, int quantity, int day = 0, int id = 0, int lot = 8, int category = 2)
            => new Record { Id = id, OperationId = op, Total = quantity, Date = Start.AddDays(day),
                LotId = lot, CategoryId = category, UserId = 1, Activo = 1 };

        [SetUp]
        public void Setup()
        {
            db = new MemoryWriteDatabase();
            db.Lots[8] = new Lot { Id = 8, FarmId = 1, Active = 1, LotName = lots[8] };
            db.Lots[9] = new Lot { Id = 9, FarmId = 1, Active = 1, LotName = lots[9] };
            writer = new RecordWriter(db.Open, () => Start.AddDays(14));
        }

        private string Describe(RecordWriteResult result)
        {
            var lotIds = new List<int>();
            var categoryIds = new List<int>();
            var context = RecordWriteMessageContext.Resolve(result,
                id => { lotIds.Add(id); return lots[id]; },
                id => { categoryIds.Add(id); return categories[id]; });
            Assert.That(lotIds, Is.EqualTo(new[] { result.LotId.Value }));
            Assert.That(categoryIds, Is.EqualTo(new[] { result.CategoryId.Value }));
            return RecordWriteMessages.Describe(result, context);
        }

        [Test]
        public void ExcessTreatmentUsesNamesAndNoInternalGroupNumbers()
        {
            db.Records.Add(R(8, 35, id: 1));
            db.Records.Add(R(14, 12, 4, 2));
            var result = writer.Create(R(17, 35, 7));
            var message = Describe(result);
            Assert.That(message, Is.EqualTo(
                "No se puede guardar el registro. Había 23 animales disponibles, pero la operación indica 35.\n" +
                "Fecha: 13/11/2026\nOperación: Antidiarreico\nLote: Prueba_F1B_LOTE\nCategoría: Terneros"));
            Assert.That(message, Does.Not.Contain("Lote: 8").And.Not.Contain("Categoría: 2"));
            Assert.That(result.LotId, Is.EqualTo(8));
            Assert.That(result.CategoryId, Is.EqualTo(2));
        }

        [Test]
        public void CountMessageUsesAllNames()
        {
            db.Records.Add(R(8, 35, id: 1));
            var result = writer.Create(R(5, 34, 8));
            Assert.That(Describe(result), Is.EqualTo(
                "No se puede guardar el conteo. El stock calculado para esa fecha es 35 y el conteo informado es 34.\n" +
                "Fecha: 14/11/2026\nOperación: Conteo\nLote: Prueba_F1B_LOTE\nCategoría: Terneros"));
        }

        [Test]
        public void RetroactiveConflictUsesTheStoredConflictGroup()
        {
            db.Records.Add(R(8, 35, id: 1));
            db.Records.Add(R(17, 35, 7, 2));
            var result = writer.Create(R(14, 12, 4));
            Assert.That(Describe(result), Is.EqualTo(
                "No se puede guardar el cambio porque dejaría inválida una operación posterior.\n\n" +
                "No se puede guardar el registro. Había 23 animales disponibles, pero la operación indica 35.\n" +
                "Fecha: 13/11/2026\nOperación: Antidiarreico\nLote: Prueba_F1B_LOTE\nCategoría: Terneros"));
            Assert.That(db.Records.Count, Is.EqualTo(2));
        }

        [TestCase(true)][TestCase(false)]
        public void EditBetweenLotsShowsOriginOrDestinationAsAppropriate(bool originConflict)
        {
            db.Records.Add(R(8, 35, id: 1));
            db.Records.Add(R(17, 35, 7, 2));
            if (!originConflict) db.Records.Add(R(8, 20, id: 3, lot: 9));
            var result = originConflict ? writer.Update(R(8, 35, id: 1, lot: 9)) :
                writer.Update(R(17, 35, 7, 2, lot: 9));
            Assert.That(result.Status, Is.EqualTo(RecordWriteStatus.OperationExceedsStock));
            int expectedLot = originConflict ? 8 : 9;
            Assert.That(result.LotId, Is.EqualTo(expectedLot));
            Assert.That(Describe(result), Does.Contain("Lote: " + lots[expectedLot]).And.Contain("Categoría: Terneros"));
            Assert.That(db.Records.Single(r => r.Id == (originConflict ? 1 : 2)).LotId, Is.EqualTo(8));
        }

        [TestCase(true)][TestCase(false)]
        public void EditBetweenCategoriesShowsTheActualAffectedCategory(bool originConflict)
        {
            db.Records.Add(R(8, 35, id: 1));
            db.Records.Add(R(17, 35, 7, 2));
            if (!originConflict) db.Records.Add(R(8, 20, id: 3, category: 1));
            var result = originConflict ? writer.Update(R(8, 35, id: 1, category: 1)) :
                writer.Update(R(17, 35, 7, 2, category: 1));
            int expectedCategory = originConflict ? 2 : 1;
            Assert.That(result.CategoryId, Is.EqualTo(expectedCategory));
            Assert.That(Describe(result), Does.Contain("Categoría: " + categories[expectedCategory]));
            Assert.That(db.Records.Single(r => r.Id == (originConflict ? 1 : 2)).CategoryId, Is.EqualTo(2));
        }

        [TestCase(null, null)][TestCase("", " ")]
        public void MissingOrEmptyNamesHaveReadableFallbacks(string lot, string category)
        {
            db.Records.Add(R(8, 35, id: 1));
            var result = writer.Create(R(5, 34, 8));
            var context = RecordWriteMessageContext.Resolve(result, id => lot, id => category);
            var message = RecordWriteMessages.Describe(result, context);
            Assert.That(message, Does.Contain("Lote: Lote no disponible").And.Contain("Categoría: Categoría no disponible"));
            Assert.That(message, Does.Not.Contain("Lote: 8").And.Not.Contain("Categoría: 2"));
        }

        [Test]
        public void FailedLotLookupDoesNotHideTheAvailableCategory()
        {
            db.Records.Add(R(8, 35, id: 1));
            var result = writer.Create(R(17, 36, 7));
            var context = RecordWriteMessageContext.Resolve(result,
                id => { throw new InvalidOperationException("Private database detail"); }, id => categories[id]);
            var message = RecordWriteMessages.Describe(result, context);
            Assert.That(message, Does.Contain("Lote no disponible").And.Contain("Categoría: Terneros"));
            Assert.That(message, Does.Not.Contain("Private database detail").And.Not.Contain("InvalidOperationException"));
        }

        [TestCase(false)][TestCase(true)]
        public void NameResolutionDoesNotChangeTheRejectedTransaction(bool failLookup)
        {
            db.Records.Add(R(8, 35, id: 1));
            db.Records.Add(R(17, 35, 7, 2));
            var result = writer.Create(R(14, 12, 4));
            var originalRows = db.Records.Select(r => r.Id + ":" + r.Total + ":" + r.Activo).ToArray();
            var events = db.Events.ToArray();
            var status = result.Status;
            var stock = result.AvailableStock;
            var context = RecordWriteMessageContext.Resolve(result,
                id => { if (failLookup) throw new InvalidOperationException(); return lots[id]; },
                id => { if (failLookup) throw new InvalidOperationException(); return categories[id]; });
            RecordWriteMessages.Describe(result, context);
            Assert.Multiple(() => {
                Assert.That(result.Status, Is.EqualTo(status));
                Assert.That(result.AvailableStock, Is.EqualTo(stock));
                Assert.That(result.LotId, Is.EqualTo(8));
                Assert.That(result.CategoryId, Is.EqualTo(2));
                Assert.That(db.Records.Select(r => r.Id + ":" + r.Total + ":" + r.Activo), Is.EqualTo(originalRows));
                Assert.That(db.Events, Is.EqualTo(events));
                Assert.That(db.Saves, Is.Zero);
                Assert.That(db.Commits, Is.Zero);
                Assert.That(db.Rollbacks, Is.EqualTo(1));
            });
        }

        [Test]
        public void NonConflictResultsDoNotReadReferences()
        {
            foreach (var result in new[] {RecordWriteResult.Success(), RecordWriteResult.Failure(RecordWriteStatus.TechnicalError)})
            {
                int reads = 0;
                var context = RecordWriteMessageContext.Resolve(result,
                    id => { reads++; return "Unexpected lot"; },
                    id => { reads++; return "Unexpected category"; });
                Assert.That(reads, Is.Zero);
                Assert.That(RecordWriteMessages.Describe(result, context), Is.Not.Empty);
            }
        }

        [Test]
        public void CompatibilityFormatterAlsoUsesFallbacksInsteadOfIds()
        {
            db.Records.Add(R(8, 35, id: 1));
            var message = RecordWriteMessages.Describe(writer.Create(R(5, 34, 8)));
            Assert.That(message, Does.Contain("Lote no disponible").And.Contain("Categoría no disponible"));
            Assert.That(message, Does.Not.Contain("Lote: 8").And.Not.Contain("Categoría: 2"));
        }
    }
}
