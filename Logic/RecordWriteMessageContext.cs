using System;
using CattleCat.Domain.Records;

namespace Logic
{
    // Presentation metadata only. Resolution happens after the write result is final.
    public sealed class RecordWriteMessageContext
    {
        public RecordWriteMessageContext(string lotName, string categoryName)
        {
            LotName = string.IsNullOrWhiteSpace(lotName) ? "Lote no disponible" : lotName.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? "Categoría no disponible" : categoryName.Trim();
        }

        public string LotName { get; }
        public string CategoryName { get; }

        public static RecordWriteMessageContext Resolve(RecordWriteResult result,
            Func<int, string> findLotName, Func<int, string> findCategoryName)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return new RecordWriteMessageContext(ReadName(result.LotId, findLotName),
                ReadName(result.CategoryId, findCategoryName));
        }

        private static string ReadName(int? id, Func<int, string> lookup)
        {
            if (!id.HasValue || lookup == null) return null;
            try { return lookup(id.Value); }
            // A failed label lookup must never replace a business result or repeat a write.
            catch (Exception) { return null; }
        }
    }
}
