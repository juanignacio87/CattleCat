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
using System.Windows.Shapes;

namespace Presentation.Views
{
    /// <summary>
    /// Lógica de interacción para About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            txt.Text = "Este software fue desarrollado por Juan Ignacio.\n\n" +
                       "El objetivo principal es facilitar el registro y seguimiento de rodeos bovinos en los establecimientos ganaderos, permitiendo una gestión más eficiente y organizada.\n\n" +
                       "Para cualquier consulta o sugerencia, puede ponerse en contacto por correo electrónico con juanbastonero@gmail.com";
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
