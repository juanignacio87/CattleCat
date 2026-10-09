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
            txt.Text = "Para crear un registro exitoso, debe seguir el siguiente procedimiento:\n\n1 - Elegir un Establecimiento en el combobox que se sitúa arriba a la izquierda. Esto habilitará un segundo combobox para elegir el lote de este establecimiento.\n\n2 - Una vez elegidos tanto el lote como establecimiento, debe elegir la categoría del ganado.\n\n3 - Para seleccionar las operaciones se debe clickear sobre el o los cuadrados seleccionados. Puede seleccionar más de uno a la vez, siempre teniendo en cuenta la compatibilidad entre las operaciones, como por ejemplo, no se pueden asignar 'Nacimiento' y 'Muerte' en un mismo registro, así como tampoco 'Entrada' y 'Salida', y también las compatibilidades entre 'Operaciones' y 'Categorías' como podrían ser 'Inseminación' en 'Toros', 'Novillos' 'terneros', por citar ejemplos. Esto hará que no se genere el registro.\n\n4 - Al seleccionar la fecha recuerde que nunca podrá ser una fecha futura.\n\n5 - Seleccione su nombre. \n\n6 - Agregue una cantidad numérica para representar los animales que han sido expuestos a dichas operaciones.\n\n7 - Presione el botón agregar.\n\n8 - Finalizado este proceso un cartel le indicará si el registro tuvo éxito o si hubo un error.\n\n9 - En caso de error deberá repetir el procedimiento.";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UCOperation operation = new UCOperation();
            operation.btnHelp.IsEnabled = true;
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
