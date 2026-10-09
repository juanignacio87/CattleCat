using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalc
{
    public class D_User
    {
        private readonly Entities.Model db = new Entities.Model();

        #region CREATE

        public bool Create(Entities.User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string userName = user.UserName?.Trim();

            string lastName = user.LastName?.Trim();

            string cellPhone = user.CellPhone?.Trim();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return false;
            }

            bool alreadyExists = db.Users.Any(u => u.Activo == 1 && u.UserName.ToLower() == userName.ToLower() && (u.LastName ?? "").ToLower() == (lastName ?? "").ToLower());

            if (alreadyExists)
            {
                return false;
            }

            user.UserName = userName;
            user.LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName;

            user.CellPhone = string.IsNullOrWhiteSpace(cellPhone) ? null : cellPhone;

            user.Activo = 1;

            db.Users.Add(user);
            db.SaveChanges();

            return true;
        }

        #endregion
        #region READ

        public List<Entities.User> GetAll()
        {
            return db.Users
                .Where(u => u.Activo == 1)
                .OrderBy(u => u.UserName)
                .ThenBy(u => u.LastName)
                .ToList();
        }

        public Entities.User GetById(int userId)
        {
            return db.Users.FirstOrDefault(u => u.Id == userId && u.Activo == 1);
        }

        #endregion
        #region UPDATE

        public bool Update(int userId, string userName, string lastName, string cellPhone)
        {
            string normalizedUserName = userName?.Trim();

            string normalizedLastName = lastName?.Trim();

            string normalizedCellPhone = cellPhone?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUserName))
            {
                return false;
            }

            Entities.User existingUser = db.Users.FirstOrDefault(u => u.Id == userId && u.Activo == 1);

            if (existingUser == null)
            {
                return false;
            }

            bool duplicatedUser = db.Users.Any(u => u.Activo == 1 && u.Id != userId && u.UserName.ToLower() == normalizedUserName.ToLower() && (u.LastName ?? "").ToLower() == (normalizedLastName ?? "").ToLower());

            if (duplicatedUser)
            {
                return false;
            }

            existingUser.UserName = normalizedUserName;

            existingUser.LastName = string.IsNullOrWhiteSpace(normalizedLastName) ? null : normalizedLastName;

            existingUser.CellPhone = string.IsNullOrWhiteSpace(normalizedCellPhone) ? null : normalizedCellPhone;

            db.SaveChanges();

            return true;
        }

        #endregion
        #region DELETE

        public ( bool success, string message) Deactivate(int userId)
        {
            Entities.User user = db.Users.FirstOrDefault(u => u.Id == userId && u.Activo == 1);

            if (user == null)
            {
                return (false,"El usuario no existe o ya está inactivo.");
            }

            int activeUsers = db.Users.Count(u => u.Activo == 1);

            if (activeUsers <= 1)
            {
                return (false, "No se puede eliminar el único usuario activo. Agregue otro usuario antes de desactivarlo.");
            }

            user.Activo = 0;

            db.SaveChanges();

            return (true, "Usuario eliminado correctamente.");
        }

        #endregion
    }
}