using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRegApplication1
{
    /// <summary>
    /// This class describes the different actions that can be performed on the database.
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam

    public class Database
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;
        private string port;
        private string user;
        private string pass;
        private string firstName;
        private string lastName;
        private string connectionString;


        public Database()
        {
            Initialize();
        }

        private void Initialize()
        {

            EncodingProvider ppp;
            ppp = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(ppp);

            server = "eu-cdbr-azure-north-e.cloudapp.net";
            database = "chipfacedb";
            uid = "be40841959364d";
            password = "375d76e9";
            port = "3306";
           
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "PORT=" + port + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";" + "SslMode=None;";

            connection = new MySqlConnection(connectionString);


        }

        /// <summary>
        /// Open connection to the database
        /// </summary>
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;

            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Number);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                // When handling errors, you can your application's response based 
                // on the error number.
                // The two most common error numbers when connecting are as follows:
                // 0: Cannot connect to server.
                // 1045: Invalid user name and/or password.

                switch (ex.Number)
                {
                    case 0:
                        System.Diagnostics.Debug.Write("Cannot connect to server");
                        break;

                    case 1045:
                        System.Diagnostics.Debug.Write("Invalid username/password");
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Close connection to the database
        /// </summary>
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                ex.ToString();
                System.Diagnostics.Debug.Write("Failed to close connection");
                return false;
            }
        }

        /// <summary>
        /// Login a user to the system
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Passowrd</param>
        /// <returns>True if successful login, otherwise false</returns>
        public bool LoginUser(string username, string password)
        {
            string query = "SELECT username, password FROM login WHERE username = ?username and password = ?password";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("?username", username);
            cmd.Parameters.AddWithValue("?password", password);
            try
            {
                if (OpenConnection() == true)
                {
                    //Create Command
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        user = dataReader.GetString("username");
                        pass = dataReader.GetString("password");
                    }


                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    CloseConnection();

                    if (user == null || password == null) { return false; }

                    if (user.Equals(username) && pass.Equals(password)) { return true; }
                    else { return false; }
                }

            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.Write("ERROR: " + ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Add a new profile to the database
        /// </summary>
        /// <param name="email">Email for the regarded profile to be added</param>
        /// <param name="firstName">First name for the regarded profile to be added</param>
        /// <param name="lastName">Last name for the regarded profile to be added</param>
        public void addProfile(string email, string firstName, string lastName)
        {
            try
            {
                if (OpenConnection() == true)
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Profiles (email, firstName, lastName) " +
                                           "VALUES (@email, @firstName, @lastName) ";

                    command.Parameters.AddWithValue(@"email", email);
                    command.Parameters.AddWithValue(@"firstName", firstName);
                    command.Parameters.AddWithValue(@"lastName", lastName);

                    command.ExecuteNonQuery();

                    CloseConnection();
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Find a profile in the database, with the specified 'email'
        /// </summary>
        /// <param name="email">Email to be searched for</param>
        /// <returns></returns>
        public List<string> searchProfile(string email)
        {
            List<String> list = new List<String>();
            string query = "SELECT * FROM Profiles where email like ?email";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("?email", email + "%");
            try
            {
                if (OpenConnection() == true)
                {
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        firstName = dataReader.GetString("firstName");
                        list.Add(firstName);
                        lastName = dataReader.GetString("lastName");
                        list.Add(lastName);
                        email = dataReader.GetString("email");
                        list.Add(email);
                    }

                    // Close Data Reader
                    dataReader.Close();

                    // Close Connection
                    CloseConnection();
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.StackTrace);
            }
            return list;
        }

        /// <summary>
        /// Update a existing profile in the database
        /// </summary>
        /// <param name="email">New email</param>
        /// <param name="firstName">New first name</param>
        /// <param name="lastName">New last name</param>
        public void updateProfile(string email, string firstName, string lastName)
        {
            try
            {
                if (OpenConnection() == true)
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Profiles SET firstName = ?firstName, lastName = ?lastName where email = ?email";
              
                    command.Parameters.AddWithValue(@"email", email);
                    command.Parameters.AddWithValue(@"firstName", firstName);
                    command.Parameters.AddWithValue(@"lastName", lastName);

                    // Command.Prepare();
                    command.ExecuteNonQuery();

                    CloseConnection();
               
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Delete a profile from the database, with the specified 'email'
        /// </summary>
        /// <param name="email"></param>
        public void deleteProfile(string email)
        {
            try
            {
                if (OpenConnection() == true)
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "DELETE from Profiles WHERE email = ?email";

                    command.Parameters.AddWithValue(@"email", email);

                    // command.Prepare();
                    command.ExecuteNonQuery();

                    CloseConnection();

                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.StackTrace);
            }
        }
    }
}