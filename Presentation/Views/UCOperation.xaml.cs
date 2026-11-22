using CattleCat.Compatibility;
using CattleCat.Domain.Stock;
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
            btnHelp.IsEnabled = false;

            try
            {
                Help helpWindow = new Help
                {
                    Owner = Window.GetWindow(this)
                };

                helpWindow.ShowDialog();
            }
            finally
            {
                btnHelp.IsEnabled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Evita varios clics mientras se valida y guarda.
            btnAdd.IsEnabled = false;

            try
            {
                if (!Validate()) return;
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
                var result = recordLogic.TryCreateMany(records);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(recordLogic.DescribeWriteResult(result), "No se pudo guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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
            catch (Exception)
            {
                /*
                 * Si falla la transacción, no se limpia el formulario,
                 * permitiendo revisar o volver a intentar.
                 */
                MessageBox.Show(
                    "No se pudo completar el registro por un error técnico. Actualice los datos antes de volver a intentar.",
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
        public const int const_aftosa = LegacyStockOperationCatalog.FootAndMouth;
        public const int const_antiparasitario = LegacyStockOperationCatalog.Antiparasitic;
        public const int const_brucelosis = LegacyStockOperationCatalog.Brucellosis;
        public const int const_castraccion = LegacyStockOperationCatalog.Castration;
        public const int const_conteo = LegacyStockOperationCatalog.Count;
        public const int const_destete = LegacyStockOperationCatalog.Weaning;
        public const int const_ecografia = LegacyStockOperationCatalog.Ultrasound;
        public const int const_entrada = LegacyStockOperationCatalog.Entry;
        public const int const_inseminacion = LegacyStockOperationCatalog.Insemination;
        public const int const_ir9 = LegacyStockOperationCatalog.Ir9;
        public const int const_muerte = LegacyStockOperationCatalog.Death;
        public const int const_nacimiento = LegacyStockOperationCatalog.Birth;
        public const int const_reproductiva = LegacyStockOperationCatalog.Reproductive;
        public const int const_salida = LegacyStockOperationCatalog.Exit;
        public const int const_querato = LegacyStockOperationCatalog.Keratitis;
        public const int const_sarna = LegacyStockOperationCatalog.Scabies;
        public const int const_diarreico = LegacyStockOperationCatalog.Antidiarrheal;
        public const int const_carb = LegacyStockOperationCatalog.Carbuncle;
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

            // La cronología completa se valida al guardar mediante RecordWriter.

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
