using Presentation.Views;
using System.Windows;
using System.Windows.Input;

namespace Presentation
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region ATRIBUTS



        #endregion
        #region EVENTS

        #region BAR'S BUTTONS

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            UserManagementWindow window = new UserManagementWindow
                {
                    Owner = this
                };

            window.ShowDialog();

            /*
             * Si Nueva operación estaba abierta, se vuelve a cargar
             * para actualizar inmediatamente su lista de usuarios.
             */
            if (DataContext is UCOperation)
            {
                DataContext = new UCOperation();
            }
        }

        private void Minimizar(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnAbout(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }

        #endregion

        #region LATERAL BAR'S BUTTONS

        private void TBShow(object sender, RoutedEventArgs e)
        {
            GridContent.Opacity = 0.5;
        }

        private void TBHide(object sender, RoutedEventArgs e)
        {
            GridContent.Opacity = 1;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null; // Clear the DataContext to reset the view
        }

        private void btnOperation_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UCOperation();
        }

        private void btnRodeo_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UCRodeo();
        }

        private void btnFarm_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UCFarm();
        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UCGrid();
        }

        #endregion

        private void PreviewMouseLeftButtonDownBG(object sender, MouseButtonEventArgs e)
        {
            btnShowHide.IsChecked = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion
    }
}
