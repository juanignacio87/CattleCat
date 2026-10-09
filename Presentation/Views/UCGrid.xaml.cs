using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para UCGrid.xaml
    /// </summary>
    public partial class UCGrid : UserControl
    {
        public int idRecord { get; set; } // Variable to store the ID of the record to be edited
                
        public UCGrid()
        {
            InitializeComponent();
            GetRecord();
        }
        
        #region METHODS
        /// <summary>
        /// Method: To fit the data on the datatable
        /// Created by: Juani
        /// Date: 12/12/2023
        /// Updated by: 12/12/2023
        /// </summary>
        public void GetRecord()
        {
            Logic.L_Record lRecord = new Logic.L_Record();
            dgRecord.ItemsSource = (IEnumerable)lRecord.L_GetAll();

            dgRecord.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (dgRecord.Items.Count > 0)
                {
                    object lastItem = dgRecord.Items[dgRecord.Items.Count - 1];
                    dgRecord.ScrollIntoView(lastItem);
                }
            }),
            DispatcherPriority.Loaded);
        }

        private static object GetPropertyValue(object source, params string[] propertyNames)
        {
            if (source == null)
            {
                return null;
            }

            Type sourceType = source.GetType();

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo property = sourceType.GetProperty(
                    propertyName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (property != null)
                {
                    return property.GetValue(source);
                }
            }

            return null;
        }

        private static string FormatCsvDate(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime date)
            {
                return date.ToString("dd/MM/yyyy");
            }

            DateTime parsedDate;

            if (DateTime.TryParse(
                value.ToString(),
                out parsedDate))
            {
                return parsedDate.ToString("dd/MM/yyyy");
            }

            return value.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            /*
             * Evita que Excel interprete texto procedente de la base
             * como una fórmula.
             */
            if (value.StartsWith("=") ||
                value.StartsWith("+") ||
                value.StartsWith("-") ||
                value.StartsWith("@"))
            {
                value = "'" + value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        #endregion
        #region EVENTS
        #region BUSQUEDA
        public void Buscar(string buscar)
        {
            Logic.L_Record l_Record = new Logic.L_Record();
            dgRecord.ItemsSource = l_Record.LBuscar(buscar);
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Buscar(txtBuscar.Text);
            if (txtBuscar.Text == "")
            {
                GetRecord();
            }
        }
        #endregion
        #region BUTTON'S EVENTS
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Se toman directamente los elementos visibles en la grilla.
             * De esta manera, si existe una búsqueda activa,
             * solamente se exportan los resultados filtrados.
             */
            List<object> visibleRecords = dgRecord.Items
                .Cast<object>()
                .Where(item =>
                    item != CollectionView.NewItemPlaceholder)
                .ToList();

            if (visibleRecords.Count == 0)
            {
                MessageBox.Show(
                    "No hay registros para exportar.",
                    "Exportación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Title = "Exportar registros",
                Filter = "Archivo CSV (*.csv)|*.csv",
                DefaultExt = ".csv",
                AddExtension = true,
                FileName =
                    $"Registros_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv"
            };

            bool? result = saveDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            btnExport.IsEnabled = false;

            try
            {
                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "Fecha;Usuario;Operación;Establecimiento;" +
                    "Lote;Categoría;Cantidad");

                foreach (object record in visibleRecords)
                {
                    string date = FormatCsvDate(GetPropertyValue(record, "date", "Date"));

                    string user = Convert.ToString(GetPropertyValue(record, "user", "User"));

                    string operation = Convert.ToString(GetPropertyValue(record, "operation", "Operation"));

                    string farm = Convert.ToString(GetPropertyValue(record, "farm", "Farm"));

                    string lot = Convert.ToString(GetPropertyValue(record, "lot", "Lot"));

                    string category = Convert.ToString(GetPropertyValue(record, "category", "Category"));

                    string total = Convert.ToString(GetPropertyValue(record, "total", "Total"), CultureInfo.InvariantCulture);

                    csv.AppendLine(string.Join(
                        ";",
                        EscapeCsv(date),
                        EscapeCsv(user),
                        EscapeCsv(operation),
                        EscapeCsv(farm),
                        EscapeCsv(lot),
                        EscapeCsv(category),
                        EscapeCsv(total)));
                }

                /*
                 * UTF-8 con BOM permite que Excel reconozca correctamente
                 * tildes, eñes y otros caracteres.
                 */
                File.WriteAllText(saveDialog.FileName, csv.ToString(), new UTF8Encoding(true));

                MessageBox.Show( $"Se exportaron {visibleRecords.Count} registros correctamente.",
                    "Exportación completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"No se pudo guardar el archivo. Puede estar abierto " +
                    $"en otro programa.{Environment.NewLine}{Environment.NewLine}" +
                    $"{ex.Message}",
                    "Error de exportación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    $"No tiene permisos para guardar el archivo en esa ubicación." +
                    $"{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Error de exportación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron exportar los registros." +
                    $"{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Error de exportación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnExport.IsEnabled = true;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecord.SelectedItem == null)
            {
                MessageBox.Show(
                    "Debe seleccionar un registro para editar.",
                    "Atención");

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "¿Está seguro de que desea editar el registro?",
                "Atención",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                object selectedRow = dgRecord.SelectedItem;

                var idProperty =
                    selectedRow.GetType().GetProperty("id") ??
                    selectedRow.GetType().GetProperty("Id");

                if (idProperty == null)
                {
                    MessageBox.Show(
                        "No se pudo identificar el registro seleccionado.",
                        "Error");

                    return;
                }

                int recordId = Convert.ToInt32(
                    idProperty.GetValue(selectedRow));

                frame.Navigate(new EditOp(recordId));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo abrir el registro para editar.{Environment.NewLine}{ex.Message}",
                    "Error");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgRecord.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un registro para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            try
            {
                object selectedRow = dgRecord.SelectedItem;

                var idProperty =
                    selectedRow.GetType().GetProperty("id") ??
                    selectedRow.GetType().GetProperty("Id");

                if (idProperty == null)
                {
                    MessageBox.Show("No se pudo identificar el registro seleccionado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                int recordId = Convert.ToInt32(idProperty.GetValue(selectedRow));

                Logic.L_Record recordLogic = new Logic.L_Record();

                var validation = recordLogic.CanDeactivateRecord(recordId);

                if (!validation.canDelete)
                {
                    if (validation.conflictDate.HasValue)
                    {
                        MessageBox.Show($"No se puede eliminar este registro porque provocaría " + $"un stock negativo posteriormente.\n\n" + $"Fecha del conflicto: " + $"{validation.conflictDate.Value:dd/MM/yyyy}\n" + $"Stock proyectado: {validation.projectedStock}", "No se puede eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("El registro no existe, ya fue eliminado o no está disponible.", "No se puede eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    return;
                }

                MessageBoxResult confirmation = MessageBox.Show("¿Está seguro de que desea eliminar el registro?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmation != MessageBoxResult.Yes)
                {
                    return;
                }

                bool deleted = recordLogic.Delete(recordId);

                if (!deleted)
                {
                    MessageBox.Show("El registro no pudo eliminarse porque cambió su estado o afectaría el stock.", "No se pudo eliminar",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                GetRecord();

                MessageBox.Show("Registro eliminado correctamente.", "Eliminación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar el registro.{Environment.NewLine}{ex.Message}","Error",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        #endregion
    }
}
