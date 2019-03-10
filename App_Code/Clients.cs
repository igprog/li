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
using Newtonsoft.Json;

/// <summary>
/// Summary description for Clients
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Clients : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    public Clients() {
        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }
    public class NewClient {
        public Int32? clientId { get; set; }
        public String firstName { get; set; }
        public String lastName { get; set; }
        public String email { get; set; }
        public String phone { get; set; }
        public DateTime? birthDate { get; set; }
        public Int32 isActive { get; set; }
    }

    [WebMethod]
    public string Init() {
        NewClient client = new NewClient();
        client.clientId = null;
        client.firstName = "";
        client.lastName = "";
        client.email = "";
        client.phone = "";
        client.birthDate = DateTime.Today;
        client.isActive = 1;
        string json = JsonConvert.SerializeObject(client, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Save(NewClient client) {
        if (CheckClient(client) == false){
            return ("Član je već registriran.");
        }
        else {
            try {
                connection.Open();
                string sql = @"INSERT INTO Clients VALUES  
                       (@FirstName, @LastName, @Email, @Phone, @BirthDate, @IsActive)";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("FirstName", client.firstName));
                command.Parameters.Add(new SqlParameter("LastName", client.lastName));
                command.Parameters.Add(new SqlParameter("Email", client.email));
                command.Parameters.Add(new SqlParameter("Phone", client.phone));
                command.Parameters.Add(new SqlParameter("BirthDate", client.birthDate));
                command.Parameters.Add(new SqlParameter("IsActive", client.isActive));
                command.ExecuteNonQuery();
                connection.Close();
                return ("Registracija uspješna.");
            } catch (Exception e) { return ("Registracija nije uspjela! (Error: )" + e); }
        }
    }

    [WebMethod]
    public string Update(NewClient client) {
        try {
            connection.Open();
            string sql = @"UPDATE Clients SET  
                        FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone, BirthDate = @BirthDate, IsActive = @IsActive
                        WHERE ClientId = @ClientId";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("ClientId", client.clientId));
            command.Parameters.Add(new SqlParameter("FirstName", client.firstName));
            command.Parameters.Add(new SqlParameter("LastName", client.lastName));
            command.Parameters.Add(new SqlParameter("Email", client.email));
            command.Parameters.Add(new SqlParameter("Phone", client.phone));
            command.Parameters.Add(new SqlParameter("BirthDate", client.birthDate));
            command.Parameters.Add(new SqlParameter("IsActive", client.isActive));
            command.ExecuteNonQuery();
            connection.Close();
            return ("Spremljeno.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT ClientId, FirstName, LastName, Email, Phone, BirthDate, IsActive FROM Clients ORDER BY ClientId DESC", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<NewClient> clients = new List<NewClient>();
            while (reader.Read()) {
                NewClient xx = new NewClient() {
                    clientId = reader.GetInt32(0),
                    firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    email = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                    phone = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                    birthDate = reader.GetDateTime(5),
                    isActive = reader.GetValue(6) == DBNull.Value ? 1 : reader.GetInt32(6)
                };
                clients.Add(xx);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(clients, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetClient(Int32 clientId) {
        try {
        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        connection.Open();
        SqlCommand command = new SqlCommand("SELECT ClientId, FirstName, LastName, Email, Phone, BirthDate, IsActive FROM Clients WHERE ClientId = @ClientId", connection);
        command.Parameters.Add(new SqlParameter("ClientId", clientId));
        NewClient xx = new NewClient();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            xx.clientId = reader.GetInt32(0);
            xx.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
            xx.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
            xx.email = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
            xx.phone = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
            xx.birthDate = reader.GetDateTime(5);
            xx.isActive = reader.GetValue(6) == DBNull.Value ? 1 : reader.GetInt32(6);
        }
        connection.Close();
        string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
        return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    protected bool CheckClient(NewClient client) {
        try {
        string firstName = "";
        string lastName = "";
        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
        connection.Open();
        SqlCommand command = new SqlCommand(
            "SELECT FirstName, LastName FROM Clients WHERE FirstName = @FirstName AND LastName = @LastName ", connection);

        command.Parameters.Add(new SqlParameter("FirstName", client.firstName));
        command.Parameters.Add(new SqlParameter("LastName", client.lastName));
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            firstName = reader.GetString(0);
            lastName = reader.GetString(1);
        }
        connection.Close();
            if (client.firstName == firstName && client.lastName == lastName) {
                return false;
            }
            return true;
        } catch (Exception e) { return false; }
    }


}
