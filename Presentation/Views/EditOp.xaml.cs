using Entities;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para EditOp.xaml
    /// </summary>
    public partial class EditOp : UserControl
    {
        private Record rec;
        private Lot selectedLot;
        private readonly UCGrid grid = new UCGrid();
        readonly L_Record l_Record = new L_Record();
        private bool isLoading;

        public EditOp(int recordId)
        {
            InitializeComponent();

            rec = l_Record.GetRecordToEdit(recordId);

            if (rec == null)
            {
                MessageBox.Show(
                    "El registro seleccionado no existe o ya no está disponible.",
                    "Advertencia!");

                return;
            }

            LoadForm();
        }

        private void cbEstablecimiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading ||
                cbEstablecimiento.SelectedValue == null)
            {
                return;
            }

            int farmId = Convert.ToInt32(
                cbEstablecimiento.SelectedValue);

            var lotLogic = new Logic.L_Lot();

            cbLote.ItemsSource = lotLogic.GetAll(farmId);
            cbLote.DisplayMemberPath = "LotName";
            cbLote.SelectedValuePath = "Id";
            cbLote.SelectedIndex = -1;
        }

        private void LoadForm()
        {
            isLoading = true;

            try
            {
                var categoryLogic = new Logic.L_Category();
                var operationLogic = new Logic.L_Operation();
                var farmLogic = new Logic.L_Farm();
                var lotLogic = new Logic.L_Lot();
                var userLogic = new Logic.L_User();



                // Categorías
                cbCategoria.ItemsSource = categoryLogic.GetAll();
                cbCategoria.DisplayMemberPath = "CategoryName";
                cbCategoria.SelectedValuePath = "Id";
                cbCategoria.SelectedValue = rec.CategoryId;

                // Operaciones
                cbOperation.ItemsSource = operationLogic.GetAll();
                cbOperation.DisplayMemberPath = "OperationName";
                cbOperation.SelectedValuePath = "Id";
                cbOperation.SelectedValue = rec.OperationId;

                // Usuarios
                cbUsuario.ItemsSource = userLogic.GetAll();
                cbUsuario.DisplayMemberPath = "UserName";
                cbUsuario.SelectedValuePath = "Id";
                cbUsuario.SelectedValue = rec.UserId;

                // Establecimientos
                cbEstablecimiento.ItemsSource = farmLogic.GetAll();
                cbEstablecimiento.DisplayMemberPath = "FarmName";
                cbEstablecimiento.SelectedValuePath = "Id";

                // Averiguar a qué establecimiento pertenece el lote
                selectedLot = lotLogic.GetFarmToEdit(rec.LotId);

                if (selectedLot == null)
                {
                    MessageBox.Show(
                        "No se encontró el lote asociado al registro.",
                        "Advertencia!");

                    return;
                }

                cbEstablecimiento.SelectedValue = selectedLot.FarmId;

                // Cargar solamente los lotes del establecimiento
                cbLote.ItemsSource = lotLogic.GetAll(selectedLot.FarmId);
                cbLote.DisplayMemberPath = "LotName";
                cbLote.SelectedValuePath = "Id";
                cbLote.SelectedValue = rec.LotId;

                // Fecha y cantidad
                dpDate.DisplayDateEnd = DateTime.Today;
                dpDate.SelectedDate = rec.Date;
                tbCant.Text = rec.Total.ToString();
            }
            finally
            {
                isLoading = false;
            }
        }

        #region BUTTONS
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) { return; }
                var user = cbUsuario.SelectedItem;
                var category = cbCategoria.SelectedItem;
                var lot = cbLote.SelectedItem;
                var ope = cbOperation.SelectedItem;

                rec.Date = (DateTime)dpDate.SelectedDate;
                rec.UserId = (int)user.GetType().GetProperty("Id").GetValue(user);
                rec.CategoryId = (int)category.GetType().GetProperty("Id").GetValue(category);
                rec.LotId = (int)lot.GetType().GetProperty("Id").GetValue(lot);
                rec.OperationId = (int)ope.GetType().GetProperty("Id").GetValue(ope);
                rec.Total = int.Parse(tbCant.Text);

                Logic.L_Record l_Record = new Logic.L_Record();
                l_Record.Update(rec);

                MessageBox.Show("Registro actualizado con éxito.", "Atención");
                grid.GetRecord();
                this.Content = grid;
            }
            catch (Exception)
            {
                MessageBox.Show("Falla en el registro.", "Atención");
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Content = grid;
        }
        #endregion
        //#region DATETIMEPICKER
        //public void GetDate(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    dpDate.DisplayDateEnd = DateTime.Today;
        //    if (rec != null)
        //        dpDate.SelectedDate = rec.Date;
        //}
        //#endregion
        //#region COMBOBOXES
        //public void GetCategory(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    Logic.L_Category category = new Logic.L_Category();
        //    cbCategoria.ItemsSource = category.GetAll();
        //    cbCategoria.DisplayMemberPath = "CategoryName";
        //    cbCategoria.SelectedValuePath = "Id";
        //    if (rec != null)
        //    {
        //        cbCategoria.SelectedValue = rec.CategoryId;
        //    }

        //}

        //public void GetOperation(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    Logic.L_Operation operation = new Logic.L_Operation();
        //    cbOperation.ItemsSource = operation.GetAll();
        //    cbOperation.DisplayMemberPath = "OperationName";
        //    cbOperation.SelectedValuePath = "Id";
        //    if (rec != null)
        //    {
        //        cbOperation.SelectedValue = rec.OperationId;
        //    }
        //}

        //public void GetFarm(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    Logic.L_Farm farm = new Logic.L_Farm();
        //    Logic.L_Lot lot = new Logic.L_Lot();
        //    cbEstablecimiento.ItemsSource = farm.GetAll();
        //    cbEstablecimiento.DisplayMemberPath = "FarmName";
        //    cbEstablecimiento.SelectedValuePath = "Id";
        //    if (rec != null)
        //    {
        //        selectedLot = lot.GetFarmToEdit(rec.LotId);
        //        cbEstablecimiento.SelectedValue = selectedLot.FarmId;
        //    }
        //}

        //public void GetLot(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    Logic.L_Lot lot = new Logic.L_Lot();
        //    int valorIndex = cbEstablecimiento.SelectedIndex;
        //    cbLote.ItemsSource = (IEnumerable)lot.GetAll(valorIndex + 1);
        //    cbLote.DisplayMemberPath = "LotName";
        //    cbLote.SelectedValuePath = "Id";
        //    if (rec != null)
        //    {
        //        cbLote.SelectedValue = rec.LotId;
        //    }
        //}

        //public void GetUser(int idRec)
        //{
        //    //grid.idRecord = idRec;
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    Logic.L_User user = new Logic.L_User();
        //    cbUsuario.ItemsSource = user.GetAll();
        //    cbUsuario.DisplayMemberPath = "UserName";
        //    cbUsuario.SelectedValuePath = "Id";
        //    if (rec != null)
        //    {
        //        cbUsuario.SelectedValue = rec.UserId;
        //    }
        //}

        //private void cbEstablecimiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Logic.L_Lot lot = new Logic.L_Lot();
        //    if (cbEstablecimiento != null)
        //    {
        //        int valorIndex = cbEstablecimiento.SelectedIndex;
        //        cbLote.ItemsSource = lot.GetAll(valorIndex + 1);
        //        cbLote.DisplayMemberPath = "LotName";
        //    }
        //}
        //#endregion
        //#region TOTAL
        //public void GetTotal(int idRec)
        //{
        //    rec = l_Record.GetRecordToEdit(idRec);
        //    if (rec != null)
        //        tbCant.Text = rec.Total.ToString();
        //}
        //#endregion

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
            var selectedOperation = cbOperation.SelectedItem as Entities.Operation;

            var selectedCategory = cbCategoria.SelectedItem as Entities.Category;

            var selectedUser = cbUsuario.SelectedItem as Entities.User;

            var selectedLot = cbLote.SelectedItem as Entities.Lot;

            int operationId = selectedOperation.Id;
            int categoryId = selectedCategory.Id;
            DateTime fecha = dpDate.SelectedDate.Value;


            if (selectedLot == null)
            {
                MessageBox.Show("Debe seleccionar un lote.", "Advertencia!");

                return false;
            }

            if (selectedCategory == null)
            {
                MessageBox.Show("Debe seleccionar una categoría.", "Advertencia!");

                return false;
            }

            if (selectedUser == null)
            {
                MessageBox.Show("Debe seleccionar un usuario.", "Advertencia!");

                return false;
            }

            if (selectedOperation == null)
            {
                MessageBox.Show("Debe seleccionar una operación.", "Advertencia!");

                return false;
            }

            if (!int.TryParse(tbCant.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Debe ingresar una cantidad numérica mayor que cero.", "Advertencia!");

                return false;
            }

            if (!dpDate.SelectedDate.HasValue || dpDate.SelectedDate.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Debe seleccionar una fecha válida.", "Advertencia!");

                return false;
            }

            // Brucelosis: Toros, Vacas Cría y Vaquillonas Cría.
            if (operationId == 3 && categoryId != 3 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Brucelosis solo puede aplicarse a toros, vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Ecografía: Vacas Cría y Vaquillonas Cría.
            if (operationId == 7 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Ecografía solo puede aplicarse a vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Castración: Terneros y Toros.
            if (operationId == 4 && categoryId != 2 && categoryId != 3)
            {
                MessageBox.Show("La operación de Castración solo puede aplicarse a terneros y toros.", "Advertencia!");

                return false;
            }

            // Nacimiento: Terneros.
            if (operationId == 12 && categoryId != 2)
            {
                MessageBox.Show("La operación de Nacimiento solo puede aplicarse a terneros.", "Advertencia!");

                return false;
            }

            // Inseminación: Vacas Cría y Vaquillonas Cría.
            if (operationId == 9 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La operación de Inseminación solo puede aplicarse a vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            // Vacuna reproductiva: Toros, Vacas Cría y Vaquillonas Cría.
            if (operationId == 13 && categoryId != 3 && categoryId != 4 && categoryId != 5)
            {
                MessageBox.Show("La vacuna reproductiva solo puede aplicarse a toros, vacas cría y vaquillonas cría.", "Advertencia!");

                return false;
            }

            /*
             * Calculamos el stock sin contar el registro que se está editando.
             */
            int stockSinRegistro = l_Record.GetStock(selectedLot.Id, selectedCategory.Id, fecha, rec.Id);

            int efectoNuevo = 0;

            bool isPositiveMovement = operationId == 8 ||   // Entrada
                                      operationId == 12;    // Nacimiento

            bool isNegativeMovement = operationId == 14 ||  // Salida
                                      operationId == 11;    // Muerte

            bool isCount = operationId == 5;     // Conteo

            bool requiresExistingStock = !isPositiveMovement && !isNegativeMovement && !isCount;

            if (requiresExistingStock && stockSinRegistro <= 0)
            {
                MessageBox.Show(
                    "No se puede registrar esta operación porque no hay animales " +
                    "en el lote y categoría seleccionados para esa fecha.",
                    "Lote sin stock",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (requiresExistingStock && cantidad > stockSinRegistro)
            {
                MessageBox.Show(
                    $"La cantidad indicada supera el stock disponible.\n\n" +
                    $"Stock disponible: {stockSinRegistro}\n" +
                    $"Cantidad de la operación: {cantidad}",
                    "Cantidad superior al stock",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // Entrada o Nacimiento.
            if (operationId == 8 || operationId == 12)
            {
                efectoNuevo = cantidad;
            }

            // Salida o Muerte.
            if (operationId == 14 || operationId == 11)
            {
                efectoNuevo = -cantidad;
            }

            int stockProyectado = stockSinRegistro + efectoNuevo;

            // Validación inmediata.
            if (stockProyectado < 0)
            {
                MessageBox.Show( $"No se puede actualizar el registro porque el stock quedaría negativo.\n\n" + $"Stock disponible: {stockSinRegistro}\n" + $"Efecto del nuevo movimiento: {efectoNuevo:+#;-#;0}\n" + $"Stock resultante: {stockProyectado}", "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            /*
             * Si cambia de lote o categoría, se comprueba que retirar
             * el registro de su lugar original no rompa ese historial.
             */
            bool changesStockGroup = rec.LotId != selectedLot.Id || rec.CategoryId != selectedCategory.Id;

            if (changesStockGroup)
            {
                var originValidation = l_Record.CanDeactivateRecord(rec.Id);

                if (!originValidation.canDelete)
                {
                    string conflictDate = originValidation.conflictDate.HasValue ? originValidation.conflictDate.Value.ToString("dd/MM/yyyy") : "No disponible";

                    MessageBox.Show($"No se puede mover este registro porque al retirarlo " + $"de su lote o categoría original el stock quedaría negativo.\n\n" + $"Fecha del conflicto: {conflictDate}\n" + $"Stock proyectado: {originValidation.projectedStock}", "Stock de origen insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return false;
                }
            }

            /*
             * Se simula toda la cronología del nuevo lote y categoría,
             * excluyendo la versión anterior del registro.
             */
            var timelineValidation = l_Record.ValidateStockTimeline(selectedLot.Id, selectedCategory.Id, fecha, efectoNuevo, rec.Id);

            if (!timelineValidation.isValid)
            {
                string conflictDate = timelineValidation.conflictDate.HasValue ? timelineValidation.conflictDate.Value.ToString("dd/MM/yyyy") : "No disponible";

                MessageBox.Show($"No se puede actualizar el registro porque provocaría " + $"un stock negativo posteriormente.\n\n" +
                    $"Fecha del conflicto: {conflictDate}\n" + $"Stock proyectado: {timelineValidation.projectedStock}", "Cronología de stock inválida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            // Conteo: compara, pero no modifica el stock.
            if (operationId == 5)
            {
                int diferencia = cantidad - stockSinRegistro;

                if (diferencia != 0)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"El conteo no coincide con el stock registrado.\n\n" +
                        $"Stock esperado: {stockSinRegistro}\n" +
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

            return true;
        }

        #endregion                
    }
}
