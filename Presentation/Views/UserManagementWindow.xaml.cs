using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Presentation.Views
{
    public partial class UserManagementWindow : Window
    {
        private readonly Logic.L_User userLogic =
            new Logic.L_User();

        private bool isLoading;

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers(
            int? selectedUserId = null)
        {
            isLoading = true;

            try
            {
                List<Entities.User> users =
                    userLogic.GetAll();

                cBoxUser.ItemsSource = users;
                cBoxUser.SelectedIndex = -1;

                if (selectedUserId.HasValue)
                {
                    cBoxUser.SelectedItem =
                        users.FirstOrDefault(u =>
                            u.Id == selectedUserId.Value);
                }

                if (cBoxUser.SelectedItem == null)
                {
                    ClearFields();
                }
            }
            finally
            {
                isLoading = false;
            }

            if (cBoxUser.SelectedItem is Entities.User user)
            {
                ShowUser(user);
            }
        }

        private void ShowUser(
            Entities.User user)
        {
            txtUserName.Text =
                user.UserName ?? string.Empty;

            txtLastName.Text =
                user.LastName ?? string.Empty;

            txtCellPhone.Text =
                user.CellPhone ?? string.Empty;

            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = true;
        }

        private void ClearFields()
        {
            txtUserName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            txtCellPhone.Text = string.Empty;

            btnEdit.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(
                txtUserName.Text))
            {
                MessageBox.Show(
                    "Ingrese el nombre del usuario.",
                    "Dato requerido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtUserName.Focus();
                return false;
            }

            string phone =
                txtCellPhone.Text.Trim();

            if (!string.IsNullOrWhiteSpace(phone) &&
                !phone.All(char.IsDigit))
            {
                MessageBox.Show(
                    "El teléfono solamente puede contener números.",
                    "Teléfono inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtCellPhone.Focus();
                return false;
            }

            return true;
        }

        private void cBoxUser_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            if (cBoxUser.SelectedItem is Entities.User user)
            {
                ShowUser(user);
            }
            else
            {
                ClearFields();
            }
        }

        private void btnAdd_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            string userName =
                txtUserName.Text.Trim();

            string lastName =
                txtLastName.Text.Trim();

            string cellPhone =
                txtCellPhone.Text.Trim();

            try
            {
                bool created = userLogic.Create(
                    userName,
                    lastName,
                    cellPhone);

                if (!created)
                {
                    MessageBox.Show(
                        "No se pudo crear el usuario. Verifique que no exista otro usuario activo con el mismo nombre y apellido.",
                        "No se pudo agregar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Entities.User createdUser =
                    userLogic.GetAll()
                        .FirstOrDefault(u =>
                            string.Equals(
                                u.UserName,
                                userName,
                                StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(
                                u.LastName ?? string.Empty,
                                lastName,
                                StringComparison.OrdinalIgnoreCase));

                LoadUsers(createdUser?.Id);

                MessageBox.Show(
                    "Usuario agregado correctamente.",
                    "Operación completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError(
                    "No se pudo agregar el usuario.",
                    ex);
            }
        }

        private void btnEdit_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!(cBoxUser.SelectedItem is Entities.User user))
            {
                MessageBox.Show(
                    "Seleccione un usuario para actualizar.",
                    "Selección requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!ValidateFields())
            {
                return;
            }

            try
            {
                bool updated = userLogic.Update(user.Id, txtUserName.Text, txtLastName.Text, txtCellPhone.Text);

                if (!updated)
                {
                    MessageBox.Show("No se pudo actualizar el usuario. Verifique que el nombre y apellido no estén repetidos.", "No se pudo actualizar", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                LoadUsers(user.Id);

                MessageBox.Show("Usuario actualizado correctamente.", "Operación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError( "No se pudo actualizar el usuario.", ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(cBoxUser.SelectedItem is Entities.User user))
            {
                MessageBox.Show("Seleccione un usuario para eliminar.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation = MessageBox.Show( $"¿Desea eliminar al usuario " + $"\"{user.UserName} {user.LastName}\"?\n\n" + $"Sus registros históricos se conservarán.", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var result = userLogic.Deactivate(user.Id);

                MessageBox.Show(result.message, result.success ? "Eliminación completada" : "No se puede eliminar", MessageBoxButton.OK, result.success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.success)
                {
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowError("No se pudo eliminar el usuario.", ex);
            }
        }

        private static void ShowError(string message, Exception exception)
        {
            MessageBox.Show($"{message}{Environment.NewLine}{Environment.NewLine}" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}