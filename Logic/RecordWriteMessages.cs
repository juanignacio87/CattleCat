using CattleCat.Domain.Records;
using CattleCat.Compatibility;

namespace Logic
{
    public static class RecordWriteMessages
    {
        private static string OperationName(int? id)
        {
            switch (id)
            {
                case LegacyStockOperationCatalog.FootAndMouth: return "Aftosa";
                case LegacyStockOperationCatalog.Antiparasitic: return "Antiparasitario";
                case LegacyStockOperationCatalog.Brucellosis: return "Brucelosis";
                case LegacyStockOperationCatalog.Castration: return "Castración";
                case LegacyStockOperationCatalog.Count: return "Conteo";
                case LegacyStockOperationCatalog.Weaning: return "Destete";
                case LegacyStockOperationCatalog.Ultrasound: return "Ecografía";
                case LegacyStockOperationCatalog.Entry: return "Entrada";
                case LegacyStockOperationCatalog.Insemination: return "Inseminación";
                case LegacyStockOperationCatalog.Ir9: return "I.R.9";
                case LegacyStockOperationCatalog.Death: return "Muerte";
                case LegacyStockOperationCatalog.Birth: return "Nacimiento";
                case LegacyStockOperationCatalog.Reproductive: return "Reproductiva";
                case LegacyStockOperationCatalog.Exit: return "Salida";
                case LegacyStockOperationCatalog.Keratitis: return "Queratoconjuntivitis";
                case LegacyStockOperationCatalog.Scabies: return "Sarna";
                case LegacyStockOperationCatalog.Antidiarrheal: return "Antidiarreico";
                case LegacyStockOperationCatalog.Carbuncle: return "Carbunclo";
                default: return "Operación no disponible";
            }
        }
        public static string Describe(RecordWriteResult result) => Describe(result, null);

        public static string Describe(RecordWriteResult result, RecordWriteMessageContext context)
        {
            context = context ?? new RecordWriteMessageContext(null, null);
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
                case RecordWriteStatus.UnsupportedOperation: return "La operación no tiene una regla de stock reconocida. Revise el catálogo antes de guardar.";
                case RecordWriteStatus.OperationExceedsStock:
                case RecordWriteStatus.CountMismatch:
                    string detail = result.Status == RecordWriteStatus.CountMismatch
                        ? $"No se puede guardar el conteo. El stock calculado para esa fecha es {result.AvailableStock} y el conteo informado es {result.RequestedQuantity}."
                        : $"No se puede guardar el registro. Había {result.AvailableStock} animales disponibles, pero la operación indica {result.RequestedQuantity}.";
                    string prefix = result.IsRetroactiveConflict ? "No se puede guardar el cambio porque dejaría inválida una operación posterior.\n\n" : "";
                    return prefix + detail + $"\nFecha: {result.ConflictDate:dd/MM/yyyy}\nOperación: {OperationName(result.OperationId)}\nLote: {context.LotName}\nCategoría: {context.CategoryName}";
                case RecordWriteStatus.StockConflict:
                    return $"No se puede guardar el cambio porque provocaría stock negativo.\n\nFecha del conflicto: {result.ConflictDate:dd/MM/yyyy}\nStock proyectado: {result.ProjectedStock}";
                case RecordWriteStatus.InvalidRequest: return "Los datos del registro no son válidos.";
                default: return "No se pudo completar la operación por un error técnico. Actualice los datos antes de volver a intentar.";
            }
        }
    }
}
