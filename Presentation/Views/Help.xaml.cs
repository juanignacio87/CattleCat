using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
            txt.Text = "Para crear un registro exitoso, debe seguir el siguiente procedimiento:\n\n" +
                   "1 - Elegir un establecimiento.\n\n" +
                   "2 - Elegir el lote correspondiente.\n\n" +
                   "3 - Seleccionar la categoría y las operaciones compatibles.\n\n" +
                   "4 - Seleccionar una fecha que no sea futura.\n\n" +
                   "5 - Seleccionar el usuario.\n\n" +
                   "6 - Ingresar la cantidad de animales.\n\n" +
                   "7 - Presionar el botón Agregar.\n\n" +
                   "8 - Verificar el mensaje con el resultado de la operación.";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

    }
}
