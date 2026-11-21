using CattleCat.Domain.Records;

namespace Logic
{
    public static class RecordWriteMessages
    {
        public static string Describe(RecordWriteResult result)
        {
            switch (result.Status)
            {
                case RecordWriteStatus.Success: return "Registro guardado correctamente.";
                case RecordWriteStatus.InvalidQuantity: return "Debe ingresar una cantidad numérica mayor que cero.";
                case RecordWriteStatus.FutureDate: return "Debe seleccionar una fecha válida que no sea futura.";
                case RecordWriteStatus.UserNotFound: return "El usuario seleccionado no existe.";
                case RecordWriteStatus.UserInactive: return "El usuario seleccionado está inactivo.";
                case RecordWriteStatus.LotNotFound: return "El lote seleccionado no existe.";
                case RecordWriteStatus.LotInactive: return "El lote seleccionado está inactivo.";
                case RecordWriteStatus.FarmNotFound: return "El establecimiento del lote no existe.";
                case RecordWriteStatus.FarmInactive: return "El establecimiento del lote está inactivo.";
                case RecordWriteStatus.CategoryNotFound: return "La categoría seleccionada no existe.";
                case RecordWriteStatus.OperationNotFound: return "La operación seleccionada no existe.";
                case RecordWriteStatus.RecordUnavailable: return "El registro no existe o ya está inactivo.";
                case RecordWriteStatus.StockConflict:
                    return $"No se puede guardar el cambio porque provocaría stock negativo.\n\nFecha del conflicto: {result.ConflictDate:dd/MM/yyyy}\nStock proyectado: {result.ProjectedStock}";
                case RecordWriteStatus.InvalidRequest: return "Los datos del registro no son válidos.";
                default: return "No se pudo completar la operación por un error técnico. Actualice los datos antes de volver a intentar.";
            }
        }
    }
}
