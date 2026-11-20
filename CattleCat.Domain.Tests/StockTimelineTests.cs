using System;
using System.Collections.Generic;
using System.Linq;
using CattleCat.Compatibility;
using CattleCat.Domain.Stock;
using NUnit.Framework;

namespace CattleCat.Domain.Tests
{
    [TestFixture]
    public class StockTimelineTests
    {
        private static readonly DateTime Start = new DateTime(2024, 1, 1);
        private static StockMovement Move(int day, int quantity, StockEffect effect, bool active = true, int hour = 0)
            => new StockMovement(Start.AddDays(day).AddHours(hour), quantity, active, effect);

        private static void Result(IEnumerable<StockMovement> movements, bool valid, int stock, DateTime? conflict = null)
        {
            var result = StockTimeline.Evaluate(movements);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.EqualTo(valid));
                Assert.That(result.ProjectedStock, Is.EqualTo(stock));
                Assert.That(result.ConflictDate, Is.EqualTo(conflict));
            });
        }

        [Test] public void EmptyTimeline() => Result(new StockMovement[0], true, 0);
        [Test] public void EntryCreatesStock() => Result(new[] { Move(0, 10, StockEffect.Positive) }, true, 10);
        [Test] public void ExitWithoutStock() => Result(new[] { Move(0, 3, StockEffect.Negative) }, false, -3, Start);
        [Test] public void EntryThenValidExit() => Result(new[] { Move(0, 10, StockEffect.Positive), Move(1, 7, StockEffect.Negative) }, true, 3);
        [Test] public void LaterExitMakesStockNegative() => Result(new[] { Move(0, 10, StockEffect.Positive), Move(2, 11, StockEffect.Negative) }, false, -1, Start.AddDays(2));

        [Test]
        public void SameDayUsesClosingBalanceEvenWhenExitHappensFirst()
            => Result(new[] { Move(0, 7, StockEffect.Negative, hour: 1), Move(0, 10, StockEffect.Positive, hour: 23) }, true, 3);

        [Test]
        public void SameDayReportsNegativeClosingBalanceAtMidnight()
            => Result(new[] { Move(0, 7, StockEffect.Negative, hour: 19), Move(0, 2, StockEffect.Positive, hour: 23) }, false, -5, Start);

        [Test]
        public void InactiveRecordsAreIgnored()
            => Result(new[] { Move(0, 100, StockEffect.Negative, false), Move(1, 9, StockEffect.Positive), Move(1, 50, StockEffect.Positive, false) }, true, 9);

        [Test]
        public void NeutralOperationDoesNotChangeStock()
            => Result(new[] { Move(0, 9, StockEffect.Positive), Move(1, int.MaxValue, StockEffect.Neutral) }, true, 9);

        [Test] public void NeutralOperationNeedsNoStockInTheEngine() => Result(new[] { Move(0, 10, StockEffect.Neutral) }, true, 0);

        [TestCase(LegacyStockOperationCatalog.Entry)]
        [TestCase(LegacyStockOperationCatalog.Birth)]
        public void SimulatedRemovalOfIncomeBreaksLaterStock(int incomeId)
        {
            var original = new StockMovement(Start, 10, true, LegacyStockOperationCatalog.Classify(incomeId));
            var history = new[] { original, Move(1, 8, StockEffect.Negative) };
            Result(history, true, 2);
            Result(history.Where(m => m != original), false, -8, Start.AddDays(1));
        }

        [Test]
        public void SimulatedEditReplacesIncomeExactlyOnce()
        {
            var original = Move(0, 10, StockEffect.Positive);
            var history = new[] { original, Move(2, 8, StockEffect.Negative) };
            var edited = history.Where(m => m != original).Concat(new[] { Move(1, 9, StockEffect.Positive) });
            Result(edited, true, 1);
            Result(history.Where(m => m != original).Concat(new[] { Move(1, 7, StockEffect.Positive) }), false, -1, Start.AddDays(2));
        }

        [Test]
        public void MovingAnIncomeLaterCanBreakAnEarlierExit()
            => Result(new[] { Move(2, 10, StockEffect.Positive), Move(1, 8, StockEffect.Negative) }, false, -8, Start.AddDays(1));

        [Test]
        public void ChangingIncomeToNeutralRemovesItsStockEffect()
            => Result(new[] { Move(0, 10, StockEffect.Neutral), Move(1, 8, StockEffect.Negative) }, false, -8, Start.AddDays(1));

        [Test]
        public void MovingBetweenGroupsRequiresCheckingBothTimelines()
        {
            Result(new[] { Move(1, 8, StockEffect.Negative) }, false, -8, Start.AddDays(1));
            Result(new[] { Move(0, 10, StockEffect.Positive) }, true, 10);
        }

        [Test]
        public void UnorderedInputHasTheSameResult()
            => Result(new[] { Move(2, 4, StockEffect.Negative), Move(0, 12, StockEffect.Positive), Move(1, 3, StockEffect.Negative) }, true, 5);

        [Test]
        public void ReportsFirstConflictEvenIfLaterIncomeRepairsFinalBalance()
            => Result(new[] { Move(4, 100, StockEffect.Positive), Move(2, 7, StockEffect.Negative), Move(1, 2, StockEffect.Positive), Move(3, 8, StockEffect.Negative) }, false, -5, Start.AddDays(2));

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void NonPositiveQuantitiesAreRejected(int quantity)
            => Assert.Throws<ArgumentOutOfRangeException>(() => Move(0, quantity, StockEffect.Positive));

        [TestCase(StockEffect.Positive)]
        [TestCase(StockEffect.Negative)]
        [TestCase(StockEffect.Neutral)]
        public void QuantityValidationAppliesToAllClassifications(StockEffect effect)
            => Assert.Throws<ArgumentOutOfRangeException>(() => Move(0, 0, effect));

        [Test] public void MinimumPositiveQuantity() => Result(new[] { Move(0, 1, StockEffect.Positive) }, true, 1);
        [Test] public void MaximumIntQuantity() => Result(new[] { Move(0, int.MaxValue, StockEffect.Positive) }, true, int.MaxValue);
        [Test] public void MaximumQuantityCanBeRemoved() => Result(new[] { Move(0, int.MaxValue, StockEffect.Positive), Move(1, int.MaxValue, StockEffect.Negative) }, true, 0);

        [Test]
        public void DailySumOverflowRetainsLegacyException()
            => Assert.Throws<OverflowException>(() => StockTimeline.Evaluate(new[] { Move(0, int.MaxValue, StockEffect.Positive), Move(0, 1, StockEffect.Positive) }));

        [Test]
        public void RunningIntOverflowRetainsLegacyResultPendingPhase1B()
            => Result(new[] { Move(0, int.MaxValue, StockEffect.Positive), Move(1, 1, StockEffect.Positive) }, false, int.MinValue, Start.AddDays(1));

        [Test] public void NullSequenceIsRejected() => Assert.Throws<ArgumentNullException>(() => StockTimeline.Evaluate(null));
        [Test] public void UndefinedEffectIsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => Move(0, 1, (StockEffect)2));

        [TestCase(8, StockEffect.Positive)]
        [TestCase(12, StockEffect.Positive)]
        [TestCase(11, StockEffect.Negative)]
        [TestCase(14, StockEffect.Negative)]
        [TestCase(5, StockEffect.Neutral)]
        [TestCase(1, StockEffect.Neutral)]
        [TestCase(999, StockEffect.Neutral)]
        public void ExistingOperationIdsKeepTheirMeaning(int operationId, StockEffect effect)
        {
            Assert.That(LegacyStockOperationCatalog.Classify(operationId), Is.EqualTo(effect));
            Assert.That(LegacyStockOperationCatalog.SignedQuantity(operationId, 7), Is.EqualTo(7 * (int)effect));
        }

        [Test]
        public void MovementIdsCannotBeMutatedByConsumers()
        {
            var ids = LegacyStockOperationCatalog.GetMovementIds();
            ids[0] = 999;
            Assert.That(LegacyStockOperationCatalog.GetMovementIds(), Is.EquivalentTo(new[] { 8, 12, 11, 14 }));
        }

        [Test]
        public void ClassifiedTimelineMatchesLegacyAlgorithmForDeterministicHistories()
        {
            // Characterization oracle kept independent of the catalog and engine.
            var random = new Random(472);
            int[] ids = { 8, 12, 11, 14, 5, 1 };
            for (int sample = 0; sample < 250; sample++)
            {
                var records = Enumerable.Range(0, 30).Select(i => new
                {
                    Date = Start.AddDays(random.Next(8)).AddHours(random.Next(24)),
                    Quantity = random.Next(1, 40),
                    Operation = ids[random.Next(ids.Length)],
                    Active = random.Next(4) != 0
                }).ToList();
                int stock = 0;
                DateTime? conflict = null;
                foreach (var day in records.Where(r => r.Active).GroupBy(r => r.Date.Date).OrderBy(g => g.Key))
                {
                    stock += day.Sum(r => r.Operation == 8 || r.Operation == 12 ? r.Quantity :
                        r.Operation == 11 || r.Operation == 14 ? -r.Quantity : 0);
                    if (stock < 0) { conflict = day.Key; break; }
                }
                Result(records.Select(r => new StockMovement(r.Date, r.Quantity, r.Active,
                    LegacyStockOperationCatalog.Classify(r.Operation))), !conflict.HasValue, stock, conflict);
            }
        }
    }
}
