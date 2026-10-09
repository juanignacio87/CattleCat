using System;
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
    /// Lógica de interacción para UCOperation.xaml
    /// </summary>
    public partial class UCOperation : UserControl
    {
        Logic.Record logicRecord = new Logic.Record();

        public UCOperation()
        {
            InitializeComponent();
            GetCategory();
            GetLot();
            GetFarm();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Entities.Record modelRecord = new Entities.Record();
                //modelRecord.Date = dpDate.SelectedDate;
                modelRecord.LotId = cbLote.SelectedIndex;
                modelRecord.CategoryId = cbCategoria.SelectedIndex;
                modelRecord.UserId = cbUsuario.SelectedIndex;
                //modelRecord.OperationId = 
                logicRecord.Agregar(modelRecord);
                MessageBox.Show("Registro agregado con exito.");

            }
            catch (Exception)
            {

                MessageBox.Show("Falla en el registro.", "Atención");
            }
        }

        public void GetCategory()
        {
            Logic.Category category = new Logic.Category();
            cbCategoria.ItemsSource = category.GetAll();
            cbCategoria.DisplayMemberPath = "CategoryName";
            cbCategoria.SelectedValuePath = "SelectedCategory";
        }

        public void GetLot()
        {
            Logic.Lot lot = new Logic.Lot();
            //cbLote.ItemsSource = lot.GetAll();
            cbLote.DisplayMemberPath = "LotName";
        }

        public void GetFarm() 
        { 
            Logic.Farm lot = new Logic.Farm();
            //cbEstablecimiento.ItemsSource = lot.GetAll();
        }

        public void GetUser()
        {
            Logic.User user = new Logic.User();
            cbUsuario.ItemsSource = user.GetAll();
            cbUsuario.DisplayMemberPath = "UserName";
        }

        private void cbEstablecimiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logic.Lot lot = new Logic.Lot();
            if (cbEstablecimiento != null)
            {

                int valorIndex = cbEstablecimiento.SelectedIndex;
                //cbLote.ItemsSource = lot.GetId(valorIndex);
                cbLote.SelectedIndex = 0;
                cbLote.IsEnabled = true;
            }
        }
    }
}
