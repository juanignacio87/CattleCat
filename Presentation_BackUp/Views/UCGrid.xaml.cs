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
    /// Lógica de interacción para UCGrid.xaml
    /// </summary>
    public partial class UCGrid : UserControl
    {
        Logic.Record logicRecord = new Logic.Record();

        public UCGrid()
        {
            InitializeComponent();
            GetRecord();
        }

        void GetRecord()
        {
            GridData.ItemsSource = logicRecord.GetAll();
            GridData.DataContext = logicRecord;
            //GridData.AlternatingRowBackground
            
        }

        void Delete()
        {
            // la siguiente línea me tiré a hacerla yo, sin mucha idea
            GridData.SelectedItem = logicRecord.GetRecord();
            
        }
    }
}
