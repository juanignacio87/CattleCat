using System;
using CattleCat.Domain.Records;

namespace Dalc.RecordWriting
{
    // Compatibility contract for legacy void/bool entry points. No raw provider exception escapes.
    public sealed class RecordWriteException : Exception
    {
        public RecordWriteException(RecordWriteResult result)
            : base("No se pudo guardar el registro. Resultado: " + result.Status) { Result = result; }
        public RecordWriteResult Result { get; }
    }
}
