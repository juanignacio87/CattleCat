using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para UCOperation.xaml
    /// </summary>
    public partial class UCOperation : UserControl
    {        
        #region ATRIBUTS
                                
        private List<int> checkBoxes = new List<int>();
        Help help = new Help();

        #endregion
        #region EVENTS
        #region FORM

        public UCOperation()
        {
            InitializeComponent();
            GetCategory();
            GetFarm();
            GetUser();
            if (help.IsActive)
            {
                btnHelp.IsEnabled = false;
            }
        }

        #endregion
        #region BUTTONS

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            if (!help.IsActive)
            {
                help.Show();
            }
            else { return; }
            btnHelp.IsEnabled = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            // Evita varios clics mientras se guarda.
            btnAdd.IsEnabled = false;

            try
            {
                int userId = Convert.ToInt32(cbUsuario.SelectedValue);

                int categoryId = Convert.ToInt32(cbCategoria.SelectedValue);

                int lotId = Convert.ToInt32(cbLote.SelectedValue);

                DateTime date = dpDate.SelectedDate.Value;

                int quantity = int.Parse(tbCant.Text);

                List<Entities.Record> records = checkBoxes
                    .Distinct()
                    .Select(operationId => new Entities.Record
                    {
                        Date = date,
                        UserId = userId,
                        CategoryId = categoryId,
                        LotId = lotId,
                        OperationId = operationId,
                        Total = quantity,
                        Activo = 1
                    })
                    .ToList();

                Logic.L_Record recordLogic = new Logic.L_Record();

                // Todas las operaciones se guardan juntas.
                recordLogic.LRecords(records);

                /*
                 * Limpiar el formulario solamente después
                 * de que la transacción termine correctamente.
                 */
                dpDate.SelectedDate = DateTime.Today;

                cbCategoria.SelectedIndex = -1;
                cbEstablecimiento.SelectedIndex = -1;

                cbLote.SelectedIndex = -1;
                cbLote.ItemsSource = null;
                cbLote.IsEnabled = false;

                cbUsuario.SelectedIndex = -1;

                tbCant.Text = string.Empty;

                foreach (CheckBox checkBox in grChBoxes.Children.OfType<CheckBox>())
                {
                    checkBox.IsChecked = false;
                }

                // Protección adicional frente a valores residuales.
                checkBoxes.Clear();

                MessageBox.Show("Operación agregada con éxito.", "Registro completado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                /*
                 * Si falla la transacción, no se limpia el formulario,
                 * permitiendo revisar o volver a intentar.
                 */
                MessageBox.Show(
                    $"No se pudo completar el registro.{Environment.NewLine}" +
                    $"{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnAdd.IsEnabled = true;
            }
        }

        #endregion
        #region COMBOBOXES

        public void GetCategory()
        {
            Logic.L_Category category = new Logic.L_Category();
            cbCategoria.ItemsSource = category.GetAll();
            cbCategoria.DisplayMemberPath = "CategoryName";
            cbCategoria.SelectedValuePath ="Id";
        }

        //Las siguientes líneas de código quedan como recordatorio de lo mal hecho
        //public void GetLot()
        //{
        //    Logic.L_Lot lot = new Logic.L_Lot();
        //    cbLote.ItemsSource = (IEnumerable)lot.GetAll();
        //    cbLote.DisplayMemberPath = "LotName";
        //}

        public void GetFarm()
        {
            Logic.L_Farm farm = new Logic.L_Farm();
            cbEstablecimiento.ItemsSource = farm.GetAll();
            cbEstablecimiento.DisplayMemberPath = "FarmName";
            cbEstablecimiento.SelectedValuePath = "Id";
        }

        public void GetUser()
        {
            Logic.L_User user = new Logic.L_User();
            cbUsuario.ItemsSource = user.GetAll();
            cbUsuario.DisplayMemberPath = "UserName";
            cbUsuario.SelectedValuePath = "Id";
        }

        private void cbEstablecimiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbLote.SelectedIndex = -1;
            cbLote.ItemsSource = null;

            if (cbEstablecimiento.SelectedValue == null)
            {
                cbLote.IsEnabled = false;
                return;
            }

            int farmId = Convert.ToInt32(cbEstablecimiento.SelectedValue);

            Logic.L_Lot lotLogic = new Logic.L_Lot();

            cbLote.ItemsSource = lotLogic.GetAll(farmId);

            cbLote.DisplayMemberPath = "LotName";

            cbLote.SelectedValuePath = "Id";

            cbLote.IsEnabled = true;
        }

        #endregion
        #region OPERATIONS

        #region CONSTANTES OPERACIONES
        public const int const_aftosa = 1;
        public const int const_antiparasitario = 2;
        public const int const_brucelosis = 3;
        public const int const_castraccion = 4;
        public const int const_conteo = 5;
        public const int const_destete = 6;
        public const int const_ecografia = 7;
        public const int const_entrada = 8;
        public const int const_inseminacion = 9;
        public const int const_ir9 = 10;
        public const int const_muerte = 11;
        public const int const_nacimiento = 12;
        public const int const_reproductiva = 13;
        public const int const_salida = 14;
        public const int const_querato = 15;
        public const int const_sarna = 16;
        public const int const_diarreico = 17;
        public const int const_carb = 18;
        #endregion
        #region CHECKED EVENTS
        private void chbAftosa_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_aftosa);
        }

        private void chbAntipa_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_antiparasitario);
        }

        private void chbBrucelosis_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_brucelosis);
        }

        private void chbCastra_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_castraccion);
        }

        private void chbConteo_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_conteo);
        }

        private void chbDestete_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_destete);
        }

        private void chbEcografia_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_ecografia);
        }

        private void chbEntrada_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_entrada);
        }

        private void chbInseminacion_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_inseminacion);
        }

        private void chbIR9_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_ir9);
        }

        private void chbMuerte_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_muerte);
        }

        private void chbNacimiento_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_nacimiento);
        }

        private void chbReproductiva_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_reproductiva);
        }

        private void chbSalida_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_salida);
        }

        private void chbQuerato_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_querato);
        }

        private void chbSarna_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_sarna);
        }

        private void chbDiarreico_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_diarreico);
        }

        private void chbCarb_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Add(const_carb);
        }
        #endregion
        #region UNCHECKED EVENTS
        private void chbAftosa_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_aftosa);
        }

        private void chbDiarreico_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_diarreico);
        }

        private void chbAntipa_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_antiparasitario);
        }

        private void chbBrucelosis_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_brucelosis);
        }

        private void chbCarb_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_carb);
        }

        private void chbCastra_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_castraccion);
        }

        private void chbConteo_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_conteo);
        }

        private void chbDestete_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_destete);
        }

        private void chbEcografia_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_ecografia);
        }

        private void chbEntrada_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_entrada);
        }

        private void chbInseminacion_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_inseminacion);
        }

        private void chbIR9_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_ir9);
        }

        private void chbMuerte_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_muerte);
        }

        private void chbNacimiento_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_nacimiento);
        }

        private void chbQuerato_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_querato);
        }

        private void chbReproductiva_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_reproductiva);
        }

        private void chbSalida_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_salida);
        }

        private void chbSarna_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxes.Remove(const_sarna);
        }
        #endregion
        #endregion
        #endregion
        #region METHODS

        /// <summary>
        /// Method: Validation of the required elements
        /// Created by: Juani
        /// Date: 11/12/2023
        /// Last Update: 11/12/2023
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            var selectedLot = cbLote.SelectedItem as Entities.Lot;

            var selectedCategory = cbCategoria.SelectedItem as Entities.Category;

            var selectedUser = cbUsuario.SelectedItem as Entities.User;

            if (selectedLot == null)
            {
                MessageBox.Show("Debe seleccionar un lote. No se pudo completar el registro.", "Advertencia!");

                return false;
            }

            if (selectedCategory == null)
            {
                MessageBox.Show("Debe seleccionar una categoría. No se pudo completar el registro.", "Advertencia!");

                return false;
            }

            if (selectedUser == null)
            {
                MessageBox.Show("Debe seleccionar un usuario. No se pudo completar el registro.", "Advertencia!");

                return false;
            }

            if (!int.TryParse(tbCant.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Debe ingresar una cantidad numérica mayor que cero.", "Advertencia!");

                return false;
            }

            if (checkBoxes.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos una operación.", "Advertencia!");

                return false;
            }

            if (!dpDate.SelectedDate.HasValue || dpDate.SelectedDate.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Debe seleccionar una fecha válida.", "Advertencia!");

                return false;
            }

            int categoryId = selectedCategory.Id;

            if (HasIncompatibleOperations())
            {
                MessageBox.Show("Hay una incompatibilidad en la selección de operaciones.", "Advertencia!");

                return false;
            }

            Logic.L_Record recordLogic = new Logic.L_Record();

            int stockActual = recordLogic.GetStock(selectedLot.Id, selectedCategory.Id, dpDate.SelectedDate.Value);

            // Cantidad de movimientos seleccionados que suman stock.
            int movimientosPositivos = checkBoxes.Count(operationId => operationId == const_entrada || operationId == const_nacimiento);

            // Cantidad de movimientos seleccionados que restan stock.
            int movimientosNegativos = checkBoxes.Count(operationId => operationId == const_salida || operationId == const_muerte);

            /*
             * Las operaciones que no son movimientos ni conteos
             * requieren animales existentes en el lote y categoría.
             */
            bool hasLivestockOperation = checkBoxes.Any(operationId => operationId != const_entrada && operationId != const_nacimiento && operationId != const_salida && operationId != const_muerte && operationId != const_conteo);

            /*
             * Si en el mismo registro existe una entrada o nacimiento,
             * esos animales quedan disponibles para las operaciones
             * sanitarias seleccionadas simultáneamente.
             */
            int stockAvailableForOperations = stockActual + (cantidad * movimientosPositivos);

            if (hasLivestockOperation && stockAvailableForOperations <= 0)
            {
                MessageBox.Show("No se puede registrar esta operación porque no hay animales en el lote, de la categoría seleccionada.", "Lote sin stock", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            if (hasLivestockOperation && cantidad > stockAvailableForOperations)
            {
                MessageBox.Show( $"La cantidad indicada supera el stock disponible.\n\n" + $"Stock disponible: {stockAvailableForOperations}\n" + $"Cantidad de la operación: {cantidad}", "Cantidad superior al stock", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            int efectoNuevo = (cantidad * movimientosPositivos) - (cantidad * movimientosNegativos);

            var timelineValidation = recordLogic.ValidateStockTimeline(selectedLot.Id, selectedCategory.Id, dpDate.SelectedDate.Value, efectoNuevo);

            if (!timelineValidation.isValid)
            {
                MessageBox.Show($"No se puede guardar el registro porque provocaría " + $"un stock negativo posteriormente.\n\n" +
                    $"Fecha del conflicto: " + $"{timelineValidation.conflictDate.Value:dd/MM/yyyy}\n" + $"Stock proyectado: " +
                    $"{timelineValidation.projectedStock}", "Cronología de stock inválida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            int stockProyectado = stockActual + (cantidad * movimientosPositivos) - (cantidad * movimientosNegativos);

            if (stockProyectado < 0)
            {
                int animalesDisponibles = stockActual + (cantidad * movimientosPositivos);
                int animalesARestar = cantidad * movimientosNegativos;

                MessageBox.Show(
                    $"No se puede completar el registro porque el stock quedaría negativo.\n\n" +
                    $"Stock disponible: {animalesDisponibles}\n" +
                    $"Cantidad a descontar: {animalesARestar}\n" +
                    $"Stock resultante: {stockProyectado}",
                    "Stock insuficiente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (checkBoxes.Contains(const_conteo))
            {
                int diferencia = cantidad - stockProyectado;

                if (diferencia != 0)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"El conteo no coincide con el stock registrado.\n\n" +
                        $"Stock anterior: {stockActual}\n" +
                        $"Stock esperado después de los movimientos: {stockProyectado}\n" +
                        $"Cantidad contada: {cantidad}\n" +
                        $"Diferencia: {diferencia:+#;-#;0}\n\n" +
                        $"¿Desea guardar igualmente el conteo?",
                        "Diferencia de stock",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }

            // Brucelosis: Toros, Vacas Cría y Vaquillonas Cría.
            if (checkBoxes.Contains(const_brucelosis) && categoryId != 3 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Brucelosis solo puede aplicarse a toros, vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Ecografía: Vacas Cría y Vaquillonas Cría.
            if (checkBoxes.Contains(const_ecografia) && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Ecografía solo puede aplicarse a vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Castración: Terneros y Toros.
            if (checkBoxes.Contains(const_castraccion) && categoryId != 2 && categoryId != 3)
            {
                MessageBox.Show("La operación de Castración solo puede aplicarse a terneros y toros.", "Advertencia!");
                    
                return false;
            }

            // Nacimiento: Terneros.
            if (checkBoxes.Contains(const_nacimiento) && categoryId != 2)
            {
                MessageBox.Show("La operación de Nacimiento solo puede aplicarse a terneros.", "Advertencia!");

                return false;
            }

            // Inseminación: Vacas Cría y Vaquillonas Cría.
            if (checkBoxes.Contains(const_inseminacion) && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Inseminación solo puede aplicarse a vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Vacuna reproductiva: Toros, Vacas Cría y Vaquillonas Cría.
            if (checkBoxes.Contains(const_reproductiva) && categoryId != 3 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La vacuna reproductiva solo puede aplicarse a toros, vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            return true;
        }

        private bool HasIncompatibleOperations()
        {
            bool Has(int operationId) => checkBoxes.Contains(operationId);

            if (Has(const_entrada) && Has(const_salida))
            {
                return true;
            }

            if (Has(const_nacimiento) && Has(const_muerte))
            {
                return true;
            }

            if (Has(const_entrada) && Has(const_muerte))
            {
                return true;
            }

            if (Has(const_nacimiento) && Has(const_salida))
            {
                return true;
            }

            if (Has(const_castraccion) && (Has(const_brucelosis) || Has(const_ecografia)))
            {
                return true;
            }

            if (Has(const_inseminacion) && Has(const_reproductiva))
            {
                return true;
            }

            return false;
        }


        #endregion

    }
}
