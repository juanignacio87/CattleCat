using System.Collections.Generic;

namespace Logic
{
    public class L_User
    {
        private readonly Dalc.D_User dUser = new Dalc.D_User();

        #region CREATE

        public bool Create(string userName, string lastName, string cellPhone)
        {
            Entities.User user = new Entities.User
                {
                    UserName = userName,
                    LastName = lastName,
                    CellPhone = cellPhone,
                    Activo = 1
                };

            return dUser.Create(user);
        }

        #endregion

        #region READ

        public List<Entities.User> GetAll()
        {
            return dUser.GetAll();
        }

        public Entities.User GetById(int userId)
        {
            return dUser.GetById(userId);
        }

        #endregion

        #region UPDATE

        public bool Update(int userId, string userName, string lastName, string cellPhone)
        {
            return dUser.Update(userId, userName, lastName, cellPhone);
        }

        #endregion

        #region DELETE

        public (bool success, string message) Deactivate(int userId)
        {
            return dUser.Deactivate(userId);
        }

        #endregion
    }
}