using Logic;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para UCRodeo.xaml
    /// </summary>
    public partial class UCRodeo : UserControl
    {
        public UCRodeo()
        {
            InitializeComponent();
            GetFarm();
            CTotal();
        }
        #region COMBOBOXES

        public void GetFarm()
        {
            Logic.L_Farm farm = new Logic.L_Farm();

            cbEstablecimiento.ItemsSource = farm.GetAll();
            cbEstablecimiento.DisplayMemberPath = "FarmName";
            cbEstablecimiento.SelectedValuePath = "Id";
        }

        /// <summary>
        /// Event: The following event link two comboboxes, unlock a second combobox when the first is changed on its selection
        /// Created by: Juani
        /// Date: 17/04/2024
        /// Last Update: 17/04/2024
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbEstablecimiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbLote.SelectedIndex = -1;
            cbLote.ItemsSource = null;

            if (cbEstablecimiento.SelectedValue == null)
            {
                cbLote.IsEnabled = false;

                CTotal();
                return;
            }
            int farmId = Convert.ToInt32(cbEstablecimiento.SelectedValue);

            Logic.L_Lot lotLogic = new Logic.L_Lot();

            cbLote.ItemsSource = lotLogic.GetAll(farmId);
            cbLote.DisplayMemberPath = "LotName";
            cbLote.SelectedValuePath = "Id";
            cbLote.IsEnabled = true;

            CTotal();
        }

        private void cbLote_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CTotal();
        }
        #endregion
        #region BUTTON
        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            cbEstablecimiento.SelectedIndex = -1;
            cbLote.SelectedIndex = -1;
            cbLote.ItemsSource = null;
            cbLote.IsEnabled = false;

            CTotal();
        }
        #endregion
        #region METHODS
        private readonly L_Record l_Record = new L_Record();

        private void CTotal()
        {
            int farmId = cbEstablecimiento.SelectedValue == null ? 0 : Convert.ToInt32(cbEstablecimiento.SelectedValue);

            int lotId = cbLote.SelectedValue == null ? 0 : Convert.ToInt32(cbLote.SelectedValue);

            var (categories, total) = l_Record.GetTotal2(farmId, lotId);

            tbNovillos.Text = categories.TryGetValue(1, out int novillos) ? novillos.ToString() : "0";

            tbTerneros.Text = categories.TryGetValue(2, out int terneros) ? terneros.ToString() : "0";

            tbToros.Text = categories.TryGetValue(3, out int toros) ? toros.ToString() : "0";

            tbVacas.Text = categories.TryGetValue(4, out int vacas) ? vacas.ToString() : "0";

            tbVaquillonas.Text = categories.TryGetValue(5, out int vaquillonas) ? vaquillonas.ToString() : "0";

            tbVaqEngorde.Text = categories.TryGetValue(6, out int vaquillonasEngorde) ? vaquillonasEngorde.ToString() : "0";

            tbVacasEng.Text = categories.TryGetValue(8, out int vacasEngorde) ? vacasEngorde.ToString() : "0";

            tbTotal.Text = total.ToString();
        }

        #endregion
    }
}
