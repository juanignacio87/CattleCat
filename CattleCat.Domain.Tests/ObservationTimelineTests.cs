using System;
using System.Linq;
using CattleCat.Compatibility;
using CattleCat.Domain.Stock;
using NUnit.Framework;
namespace CattleCat.Domain.Tests
{
    [TestFixture]
    public class ObservationTimelineTests
    {
        private static readonly DateTime Start = new DateTime(2026, 11, 6);
        private static StockMovement M(int id, int quantity, int day = 0, bool active = true)
            => new StockMovement(Start.AddDays(day), quantity, active, LegacyStockOperationCatalog.Behavior(id), id);

        [TestCase(10, true)][TestCase(23, true)][TestCase(24, false)][TestCase(35, false)]
        public void TreatmentIsBoundedByAvailableStock(int quantity, bool valid)
        {
            var result = StockTimeline.Evaluate(new[] {M(8,23),M(17,quantity,1)});
            Assert.That(result.IsValid, Is.EqualTo(valid));
            Assert.That(result.ProjectedStock, Is.EqualTo(23));
            if (!valid) Assert.Multiple(() => {
                Assert.That(result.Conflict, Is.EqualTo(StockTimelineConflict.OperationExceedsStock));
                Assert.That(result.OperationId, Is.EqualTo(17));
                Assert.That(result.RequestedQuantity, Is.EqualTo(quantity));
                Assert.That(result.AvailableStock, Is.EqualTo(23));
            });
        }
        [Test] public void TreatmentNeedsPositiveStock()
        {
            var result = StockTimeline.Evaluate(new[] {M(17,1)});
            Assert.That(result.Conflict, Is.EqualTo(StockTimelineConflict.OperationExceedsStock));
            Assert.That(result.AvailableStock, Is.Zero);
        }
        [TestCase(22, false)][TestCase(23, true)][TestCase(24, false)]
        public void CountRequiresExactStock(int quantity, bool valid)
        {
            var result = StockTimeline.Evaluate(new[] {M(8,23),M(5,quantity,1)});
            Assert.That(result.IsValid, Is.EqualTo(valid));
            if (!valid) Assert.That(result.Conflict, Is.EqualTo(StockTimelineConflict.CountMismatch));
        }
        [TestCase(8,14)][TestCase(12,11)]
        public void SameDayOrderIsIncomeThenObservationsThenOutcomes(int income, int outcome)
        {
            var rows = new[] {M(outcome,12),M(17,35),M(5,35),M(income,35)};
            var result = StockTimeline.Evaluate(rows);
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ProjectedStock, Is.EqualTo(23));
            Assert.That(StockTimeline.Evaluate(rows.Reverse()).IsValid, Is.True);
        }
        [TestCase(-1, true)][TestCase(1, false)][TestCase(0, true)]
        public void EntryMustPrecedeOrShareObservationDate(int incomeDay, bool valid)
            => Assert.That(StockTimeline.Evaluate(new[] {M(17,12),M(8,12,incomeDay)}).IsValid, Is.EqualTo(valid));
        [TestCase(false)][TestCase(true)]
        public void RealCaseReportsTreatmentConflictRegardlessOfInputOrder(bool reverse)
        {
            var rows = new[] {M(8,35),M(14,12,4),M(17,35,7)};
            var result = StockTimeline.Evaluate(reverse ? rows.Reverse() : rows);
            Assert.Multiple(() => {
                Assert.That(result.Conflict, Is.EqualTo(StockTimelineConflict.OperationExceedsStock));
                Assert.That(result.ConflictDate, Is.EqualTo(Start.AddDays(7)));
                Assert.That(result.AvailableStock, Is.EqualTo(23));
                Assert.That(result.OperationId, Is.EqualTo(17));
                Assert.That(result.RequestedQuantity, Is.EqualTo(35));
            });
        }
        [TestCase(6, true)][TestCase(8, false)]
        public void NewIncomeOnlyRepairsLaterObservations(int day, bool valid)
            => Assert.That(StockTimeline.Evaluate(new[] {M(8,35),M(14,12,4),M(17,35,7),M(8,12,day)}).IsValid, Is.EqualTo(valid));
        [Test] public void InactiveObservationsAreIgnored()
            => Assert.That(StockTimeline.Evaluate(new[] {M(17,100,active:false),M(5,100,active:false)}).IsValid, Is.True);
        [Test] public void InactiveIncomeDoesNotJustifyTreatment()
            => Assert.That(StockTimeline.Evaluate(new[] {M(8,35,active:false),M(17,35)}).IsValid, Is.False);
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(6)][TestCase(7)]
        [TestCase(9)][TestCase(10)][TestCase(13)][TestCase(15)][TestCase(16)][TestCase(17)][TestCase(18)]
        public void AllExistingAnimalOperationsHaveExplicitRules(int id)
            => Assert.That(LegacyStockOperationCatalog.Behavior(id), Is.EqualTo(StockOperationBehavior.ExistingAnimals));
        [Test] public void UnknownOperationHasNoInventedRule()
            => Assert.That(LegacyStockOperationCatalog.Behavior(999), Is.EqualTo(StockOperationBehavior.Unknown));
        [Test] public void StableConflictSelectionWithinDay()
        {
            var rows = new[] {M(8,23),M(17,35),M(5,24)};
            var a = StockTimeline.Evaluate(rows);var b = StockTimeline.Evaluate(rows.Reverse());
            Assert.That(a.OperationId, Is.EqualTo(b.OperationId));
            Assert.That(a.Conflict, Is.EqualTo(b.Conflict));
        }
        [TestCase(0)][TestCase(-1)]
        public void ObservationRejectsInvalidQuantity(int quantity)
            => Assert.Throws<ArgumentOutOfRangeException>(()=>M(17,quantity));
        [Test] public void NegativeConflictReportsTheOutcomeThatExceedsRemainingStock()
        {
            var result=StockTimeline.Evaluate(new[]{M(8,10),M(14,7,1),M(14,6,1)});
            Assert.That(result.Conflict,Is.EqualTo(StockTimelineConflict.NegativeStock));
            Assert.That(result.ProjectedStock,Is.EqualTo(-3));
            Assert.That(result.RequestedQuantity,Is.EqualTo(7));
            Assert.That(result.AvailableStock,Is.EqualTo(4));
        }
    }
}
