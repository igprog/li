using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;

[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    string supervisorUserName = ConfigurationManager.AppSettings["SupervisorUserName"];
    string supervisorPassword = ConfigurationManager.AppSettings["SupervisorPassword"];
    public Users () {
    }
    public class NewUser {
        public Int32? userId { get; set; }
        public Int32 userType { get; set; }
        public String firstName { get; set; }
        public String lastName { get; set; }
        public String companyName { get; set; }
        public String address { get; set; }
        public String postalCode { get; set; }
        public String city { get; set; }
        public String country { get; set; }
        public String pin { get; set; }
        public String phone { get; set; }
        public String email { get; set; }
        public String userName { get; set; }
        public String password { get; set; }
        public Int32 adminType { get; set; }
        public Int32? userGroupId { get; set; }
        public DateTime? activationDate { get; set; }
        public DateTime? expirationDate { get; set; }
        public Int32 isActive { get; set; }
        public String ipAddress { get; set; }
    }

    [WebMethod]
    public string Init() {
        NewUser user = new NewUser();
            user.userId = null;
            user.userType = 0;
            user.firstName = "";
            user.lastName = "";
            user.companyName = "";
            user.address = "";
            user.postalCode = "";
            user.city = "";
            user.country = "";
            user.pin = "";
            user.phone = "";
            user.email = "";
            user.userName = "";
            user.password = "";
            user.adminType = 0;
            user.userGroupId = null;
            user.activationDate = DateTime.Today;
            user.expirationDate = DateTime.Today;
            user.isActive = 1;
            user.ipAddress = "";
        
        string json = JsonConvert.SerializeObject(user, Formatting.Indented);
        return json;
    }

    #region WebMethods

    [WebMethod]
    public string Login(string userName, string password) {
        if (userName == supervisorUserName && password == supervisorPassword) {
            return Supervisor();
        }
        else {
            try {
                connection.Open();
                SqlCommand command = new SqlCommand(
                      "SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress FROM Users WHERE UserName = @UserName AND Password = @Password ", connection);
                command.Parameters.Add(new SqlParameter("UserName", userName));
                command.Parameters.Add(new SqlParameter("Password", Encrypt(password)));
                NewUser user = new NewUser();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    user.userId = reader.GetInt32(0);
                    user.userType = reader.GetInt32(1);
                    user.firstName = reader.GetString(2);
                    user.lastName = reader.GetString(3);
                    user.companyName = reader.GetString(4);
                    user.address = reader.GetString(5);
                    user.postalCode = reader.GetString(6);
                    user.city = reader.GetString(7);
                    user.country = reader.GetString(8);
                    user.pin = reader.GetString(9);
                    user.phone = reader.GetString(10);
                    user.email = reader.GetString(11);
                    user.userName = reader.GetString(12);
                    user.password = Decrypt(reader.GetString(13));
                    user.adminType = reader.GetInt32(14);
                    user.userGroupId = reader.GetInt32(15);
                    user.activationDate = reader.GetDateTime(16);
                    user.expirationDate = reader.GetDateTime(17);
                    user.isActive = reader.GetInt32(18);
                    user.ipAddress = reader.GetString(19);
                }
                connection.Close();
                string json = JsonConvert.SerializeObject(user, Formatting.Indented);
                return json;
            } catch (Exception e) { return ("Error: " + e); }
        }
    }

    [WebMethod]
    public string Singup(NewUser user) {
        if (checkUser(user.email) == false) {
            return ("The user is already registered.");
        }
        else {
            try {
                user.password = Encrypt(user.password);
                connection.Open();
                string sql = @"INSERT INTO Users VALUES  
                       (@UserType, @FirstName, @LastName, @CompanyName, @Address, @PostalCode, @City, @Country, @Pin, @Phone, @Email, @UserName, @Password, @AdminType, @UserGroupId, @ActivationDate, @ExpirationDate, @IsActive, @IPAddress)";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("UserType", user.userType));
                command.Parameters.Add(new SqlParameter("FirstName", user.firstName));
                command.Parameters.Add(new SqlParameter("LastName", user.lastName));
                command.Parameters.Add(new SqlParameter("CompanyName", user.companyName));
                command.Parameters.Add(new SqlParameter("Address", user.address));
                command.Parameters.Add(new SqlParameter("PostalCode", user.postalCode));
                command.Parameters.Add(new SqlParameter("City", user.city));
                command.Parameters.Add(new SqlParameter("Country", user.country));
                command.Parameters.Add(new SqlParameter("Pin", user.pin));
                command.Parameters.Add(new SqlParameter("Phone", user.phone));
                command.Parameters.Add(new SqlParameter("Email", user.email));
                command.Parameters.Add(new SqlParameter("UserName", user.userName));
                command.Parameters.Add(new SqlParameter("Password", user.password));
                command.Parameters.Add(new SqlParameter("adminType", user.adminType));
                command.Parameters.Add(new SqlParameter("UserGroupId", user.userGroupId == null ? DateTime.Now.Millisecond : user.userGroupId));
                command.Parameters.Add(new SqlParameter("ActivationDate", user.activationDate));
                command.Parameters.Add(new SqlParameter("ExpirationDate", user.expirationDate));
                command.Parameters.Add(new SqlParameter("IsActive", user.isActive));
                command.Parameters.Add(new SqlParameter("IPAddress", user.ipAddress));
                command.ExecuteNonQuery();
                connection.Close();
                return ("Registracija uspješno završena.");
            } catch (Exception e) { return ("Greška: " + e); }
        }
    }

    [WebMethod]
    public string Save(NewUser x) {
        try {
            x.password = Encrypt(x.password);
            string sql = "";
            SqlCommand command = new SqlCommand(sql, connection);
            if (x.userId == null) {
                sql = @"INSERT INTO Users
                        VALUES (@UserType, @FirstName, @LastName, @CompanyName, @Address, @PostalCode, @City, @Country, @Pin, @Phone, @Email, @UserName, @Password, @AdminType, @UserGroupId, @ActivationDate, @ExpirationDate, @IsActive, @IPAddress)";
                command = new SqlCommand(sql, connection);
            }
            else {
                sql = @"UPDATE Users SET  
                        UserType = @UserType, FirstName = @FirstName, LastName = @LastName, CompanyName = @CompanyName, Address = @Address, PostalCode = @PostalCode, City = @City, Country = @Country, Pin = @Pin, Phone = @Phone, Email = @Email, UserName = @UserName, Password = @Password, AdminType = @AdminType, UserGroupId = @UserGroupId, ActivationDate = @ActivationDate, ExpirationDate = @ExpirationDate, IsActive = @IsActive, IPAddress = @IPAddress
                        WHERE UserId = @UserId";
                command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("UserId", x.userId));
            }
            connection.Open();
            command.Parameters.Add(new SqlParameter("UserType", x.userType));
            command.Parameters.Add(new SqlParameter("FirstName", x.firstName));
            command.Parameters.Add(new SqlParameter("LastName", x.lastName));
            command.Parameters.Add(new SqlParameter("CompanyName", x.companyName));
            command.Parameters.Add(new SqlParameter("Address", x.address));
            command.Parameters.Add(new SqlParameter("PostalCode", x.postalCode));
            command.Parameters.Add(new SqlParameter("City", x.city));
            command.Parameters.Add(new SqlParameter("Country", x.country));
            command.Parameters.Add(new SqlParameter("Pin", x.pin));
            command.Parameters.Add(new SqlParameter("Phone", x.phone));
            command.Parameters.Add(new SqlParameter("Email", x.email));
            command.Parameters.Add(new SqlParameter("UserName", x.userName));
            command.Parameters.Add(new SqlParameter("Password", x.password));
            command.Parameters.Add(new SqlParameter("adminType", x.adminType));
            command.Parameters.Add(new SqlParameter("UserGroupId", x.userGroupId));
            command.Parameters.Add(new SqlParameter("ActivationDate", x.activationDate));
            command.Parameters.Add(new SqlParameter("ExpirationDate", x.expirationDate));
            command.Parameters.Add(new SqlParameter("IsActive", x.isActive));
            command.Parameters.Add(new SqlParameter("IPAddress", x.ipAddress));
            command.ExecuteNonQuery();
            connection.Close();
            return ("Spremljeno");
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Load(int adminType, int userGroupId, int userId) {
        try {
        connection.Open();
            string sql = "";
            switch (adminType) {
                case 0:
                    sql = @"SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress
                        FROM Users";
                    break;
                case 1:
                    sql = @"SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress
                                FROM Users
                                WHERE UserGroupId = @UserGroupId OR UserGroupId = @UserId";
                    break;
                case 2:
                    sql = @"SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress
                                FROM Users
                                WHERE UserId = @UserId";
                    break;
                default:
                    sql = "";
                    break;
            }
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("UserGroupId", userGroupId));
            command.Parameters.Add(new SqlParameter("UserId", userId));
            SqlDataReader reader = command.ExecuteReader();
            List<NewUser> users = new List<NewUser>();
            while (reader.Read()) {
            NewUser xx = new NewUser() {
                userId = reader.GetInt32(0),
                userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1),
                firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5),
                postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6),
                city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7),
                country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8),
                pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9),
                phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10),
                email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11),
                userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12),
                password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13)),
                adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14),
                userGroupId = reader.GetInt32(15),
                activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.Today : reader.GetDateTime(16),
                expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.Today : reader.GetDateTime(17),
                isActive = reader.GetValue(18) == DBNull.Value ? 1 : reader.GetInt32(18),
                ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19)
            };
            users.Add(xx);
        }
        connection.Close();
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetUser(String userId) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress FROM Users WHERE UserId = @UserId", connection);
            command.Parameters.Add(new SqlParameter("UserId", userId));
            NewUser x = new NewUser();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.userId = reader.GetInt32(0);
                x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                x.userGroupId = reader.GetInt32(15);
                x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.Today : reader.GetDateTime(16);
                x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.Today : reader.GetDateTime(17);
                x.isActive = reader.GetValue(18) == DBNull.Value ? 1 : reader.GetInt32(18);
                x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetUsersByAdminType(string adminType) {
        try {
            connection.Open();
            string sql = @"SELECT UserId, UserType, FirstName, LastName, CompanyName, Address, PostalCode, City, Country, Pin, Phone, Email, UserName, Password, AdminType, UserGroupId, ActivationDate, ExpirationDate, IsActive, IPAddress
                        FROM Users WHERE AdminType = @AdminType";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("AdminType", adminType));
            SqlDataReader reader = command.ExecuteReader();
            List<NewUser> xx = new List<NewUser>();
            while (reader.Read()) {
                NewUser x = new NewUser() {
                    userId = reader.GetInt32(0),
                    userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1),
                    firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                    companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                    address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5),
                    postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6),
                    city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7),
                    country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8),
                    pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9),
                    phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10),
                    email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11),
                    userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12),
                    password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13)),
                    adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14),
                    userGroupId = reader.GetInt32(15),
                    activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.Today : reader.GetDateTime(16),
                    expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.Today : reader.GetDateTime(17),
                    isActive = reader.GetValue(18) == DBNull.Value ? 1 : reader.GetInt32(18),
                    ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19)
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }
    #endregion

    #region Methods
    protected bool checkUser(string email) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(
                "SELECT Email FROM Users WHERE Email = @Email ", connection);
            command.Parameters.Add(new SqlParameter("Email", email));
            string userEmail = "";
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                userEmail = reader.GetString(0);
            }
            connection.Close();
            if (userEmail == email) {
                return false;
            }
            return true;
        } catch (Exception e) { return false; }
    }

    protected string Encrypt(string clearText) {
        string EncryptionKey = "MDOLD54FLSK5123";  // "MAKV2SPBNI99212";
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create()) {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)) {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }

    protected string Decrypt(string cipherText) {
        string EncryptionKey = "MDOLD54FLSK5123";  // "MAKV2SPBNI99212";
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create()) {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write)) {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    private string Supervisor() {
        NewUser user = new NewUser();
        user.userId = 999999999;
        user.userType = 0;
        user.firstName = "Igor";
        user.lastName = "Gašparović";
        user.companyName = "IGPROG";
        user.address = "Ludvetov breg 5";
        user.postalCode = "51000";
        user.city = "Rijeka";
        user.country = "Hrvatska";
        user.pin = "58331314923";
        user.phone = "098330966";
        user.email = "igprog@yahoo.com";
        user.userName = supervisorUserName;
        user.password = supervisorPassword;
        user.adminType = 0;
        user.userGroupId = 999999999;
        user.activationDate = DateTime.Today;
        user.expirationDate = DateTime.Today;
        user.isActive = 1;
        user.ipAddress = "";

        string json = JsonConvert.SerializeObject(user, Formatting.Indented);
        return json;
    }
    #endregion


}
