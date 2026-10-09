using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para UCFarm.xaml
    /// </summary>
    public partial class UCFarm : UserControl
    {
        private readonly Logic.L_Farm farmLogic = new Logic.L_Farm();

        private readonly Logic.L_Lot lotLogic = new Logic.L_Lot();

        private bool isLoading;

        public UCFarm()
        {
            InitializeComponent();
            LoadFarms();
        }

        #region LOAD

        private void LoadFarms(int? selectedFarmId = null)
        {
            isLoading = true;

            try
            {
                List<Entities.Farm> farms = farmLogic.GetAll();

                cBoxFarm.ItemsSource = farms;
                cBoxFarm.SelectedIndex = -1;

                if (selectedFarmId.HasValue)
                {
                    cBoxFarm.SelectedItem = farms.FirstOrDefault(f => f.Id == selectedFarmId.Value);
                }

                if (cBoxFarm.SelectedItem == null)
                {
                    txtFarmName.Text = string.Empty;
                    ClearLots();
                }
            }
            finally
            {
                isLoading = false;
            }

            /*
             * SelectionChanged no siempre se ejecuta mientras
             * isLoading está activo, por eso actualizamos manualmente.
             */
            if (cBoxFarm.SelectedItem is Entities.Farm selectedFarm)
            {
                ShowSelectedFarm(selectedFarm);
            }
        }

        private void LoadLots(int farmId, int? selectedLotId = null)
        {
            isLoading = true;

            try
            {
                List<Entities.Lot> lots = lotLogic.GetAll(farmId);

                cBoxLot.ItemsSource = lots;
                cBoxLot.IsEnabled = true;
                txtLotName.IsEnabled = true;
                btnAddLot.IsEnabled = true;

                cBoxLot.SelectedIndex = -1;
                txtLotName.Text = string.Empty;

                if (selectedLotId.HasValue)
                {
                    cBoxLot.SelectedItem = lots.FirstOrDefault(l => l.Id == selectedLotId.Value);
                }
            }
            finally
            {
                isLoading = false;
            }

            if (cBoxLot.SelectedItem is Entities.Lot selectedLot)
            {
                ShowSelectedLot(selectedLot);
            }
            else
            {
                UpdateLotButtons();
            }
        }

        private void ClearLots()
        {
            cBoxLot.ItemsSource = null;
            cBoxLot.SelectedIndex = -1;
            cBoxLot.IsEnabled = false;

            txtLotName.Text = string.Empty;
            txtLotName.IsEnabled = false;

            btnAddLot.IsEnabled = false;
            btnEditLot.IsEnabled = false;
            btnDeleteLot.IsEnabled = false;
        }

        private void ShowSelectedFarm(Entities.Farm farm)
        {
            txtFarmName.Text = farm.FarmName;

            btnEditFarm.IsEnabled = true;
            btnDeleteFarm.IsEnabled = true;

            LoadLots(farm.Id);
        }

        private void ShowSelectedLot(Entities.Lot lot)
        {
            txtLotName.Text = lot.LotName;
            UpdateLotButtons();
        }

        private void UpdateLotButtons()
        {
            bool hasSelectedLot = cBoxLot.SelectedItem != null;

            btnEditLot.IsEnabled = hasSelectedLot;
            btnDeleteLot.IsEnabled = hasSelectedLot;
        }

        #endregion

        #region SELECTION

        private void cBoxFarm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            if (cBoxFarm.SelectedItem is Entities.Farm farm)
            {
                ShowSelectedFarm(farm);
            }
            else
            {
                txtFarmName.Text = string.Empty;
                btnEditFarm.IsEnabled = false;
                btnDeleteFarm.IsEnabled = false;

                ClearLots();
            }
        }

        private void cBoxLot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }
            if (cBoxLot.SelectedItem is Entities.Lot lot)
            {
                ShowSelectedLot(lot);
            }
            else
            {
                txtLotName.Text = string.Empty;
                UpdateLotButtons();
            }
        }

        #endregion

        #region FARM EVENTS

        private void btnAddFarm_Click(object sender, RoutedEventArgs e)
        {
            string farmName = txtFarmName.Text.Trim();

            if (string.IsNullOrWhiteSpace(farmName))
            {
                MessageBox.Show("Ingrese el nombre del establecimiento.", "Dato requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFarmName.Focus();
                return;
            }

            try
            {
                bool created = farmLogic.Create(farmName);

                if (!created)
                {
                    MessageBox.Show("No se pudo crear el establecimiento. Verifique que el nombre no esté repetido.", "No se pudo agregar", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                List<Entities.Farm> farms = farmLogic.GetAll();

                Entities.Farm createdFarm = farms.FirstOrDefault(f => string.Equals( f.FarmName, farmName, StringComparison.OrdinalIgnoreCase));

                LoadFarms(createdFarm?.Id);

                MessageBox.Show( "Establecimiento agregado correctamente.", "Operación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("No se pudo agregar el establecimiento.", ex);
            }
        }

        private void btnEditFarm_Click(object sender, RoutedEventArgs e)
        {
            if (!(cBoxFarm.SelectedItem is Entities.Farm farm))
            {
                MessageBox.Show("Seleccione un establecimiento para renombrar.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            string newName = txtFarmName.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show( "Ingrese el nuevo nombre del establecimiento.", "Dato requerido", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            try
            {
                bool updated = farmLogic.Update( farm.Id, newName);

                if (!updated)
                {
                    MessageBox.Show("No se pudo renombrar el establecimiento. Verifique que el nombre no esté repetido.", "No se pudo actualizar", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                LoadFarms(farm.Id);

                MessageBox.Show( "Establecimiento actualizado correctamente.", "Operación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("No se pudo actualizar el establecimiento.", ex);
            }
        }

        private void btnDeleteFarm_Click( object sender, RoutedEventArgs e)
        {
            if (!(cBoxFarm.SelectedItem is Entities.Farm farm))
            {
                MessageBox.Show( "Seleccione un establecimiento para eliminar.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation =MessageBox.Show($"¿Desea eliminar el establecimiento \"{farm.FarmName}\"?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var result = farmLogic.Deactivate(farm.Id);

                MessageBox.Show(result.message, result.success ? "Eliminación completada" : "No se puede eliminar", MessageBoxButton.OK, result.success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.success)
                {
                    LoadFarms();
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo eliminar el establecimiento.", ex);
            }
        }

        #endregion

        #region LOT EVENTS

        private void btnAddLot_Click( object sender, RoutedEventArgs e)
        {
            if (!(cBoxFarm.SelectedItem is Entities.Farm farm))
            {
                MessageBox.Show("Seleccione primero un establecimiento.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            string lotName = txtLotName.Text.Trim();

            if (string.IsNullOrWhiteSpace(lotName))
            {
                MessageBox.Show("Ingrese el nombre del lote.", "Dato requerido", MessageBoxButton.OK, MessageBoxImage.Warning);

                txtLotName.Focus();
                return;
            }

            try
            {
                bool created = lotLogic.Create(farm.Id,lotName);

                if (!created)
                {
                    MessageBox.Show("No se pudo crear el lote. Verifique que no exista otro lote con el mismo nombre en este establecimiento.", "No se pudo agregar", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                List<Entities.Lot> lots = lotLogic.GetAll(farm.Id);

                Entities.Lot createdLot = lots.FirstOrDefault(l => string.Equals(l.LotName, lotName, StringComparison.OrdinalIgnoreCase));

                LoadLots(farm.Id, createdLot?.Id);

                MessageBox.Show("Lote agregado correctamente.", "Operación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("No se pudo agregar el lote.", ex);
            }
        }

        private void btnEditLot_Click(object sender, RoutedEventArgs e)
        {
            if (!(cBoxLot.SelectedItem is Entities.Lot lot))
            {
                MessageBox.Show( "Seleccione un lote para renombrar.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            string newName = txtLotName.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Ingrese el nuevo nombre del lote.", "Dato requerido", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            try
            {
                bool updated = lotLogic.Update( lot.Id, newName);

                if (!updated)
                {
                    MessageBox.Show( "No se pudo renombrar el lote. Verifique que el nombre no esté repetido.", "No se pudo actualizar", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                LoadLots(lot.FarmId, lot.Id);

                MessageBox.Show("Lote actualizado correctamente.", "Operación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("No se pudo actualizar el lote.", ex);
            }
        }

        private void btnDeleteLot_Click( object sender, RoutedEventArgs e)
        {
            if (!(cBoxLot.SelectedItem is Entities.Lot lot))
            {
                MessageBox.Show("Seleccione un lote para eliminar.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation = MessageBox.Show( $"¿Desea eliminar el lote \"{lot.LotName}\"?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var result = lotLogic.Deactivate(lot.Id);

                MessageBox.Show(result.message, result.success ? "Eliminación completada" : "No se puede eliminar", MessageBoxButton.OK, result.success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.success)
                {
                    LoadLots(lot.FarmId);
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo eliminar el lote.", ex);
            }
        }

        #endregion

        #region ERROR

        private static void ShowError( string message, Exception exception)
        {
            MessageBox.Show($"{message}{Environment.NewLine}{Environment.NewLine}" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
