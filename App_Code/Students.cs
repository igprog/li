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
/// Students
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Students : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

    public Students() {
    }

    public class NewStudent {
        public Int32? id { get; set; }
        public Int32? schoolClassId { get; set; }
        public string school { get; set; }
        public string schoolClass { get; set; }
        public string teacher { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public Int32 gender { get; set; }
        public DateTime? birthDate { get; set; }
        public string birthPlace { get; set; }
        public string address { get; set; }
        public Int32 height { get; set; }
        public Int32 weight { get; set; }
        public string footSize { get; set; }
        public string tShirtSize { get; set; }
        public string parentFirstName { get; set; }
        public string parentLastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string job { get; set; }

    }

    [WebMethod]
    public string Init() {
        NewStudent x = new NewStudent();
        x.id = null;
        x.schoolClassId = 0;
        x.school = "";
        x.schoolClass = "";
        x.teacher = "";
        x.firstName = "";
        x.lastName = "";
        x.gender = 0;
        x.birthDate = DateTime.UtcNow;
        x.birthPlace = "";
        x.address = "";
        x.height = 0;
        x.weight = 0;
        x.footSize = "";
        x.tShirtSize = "";
        x.parentFirstName = "";
        x.parentLastName = "";
        x.phone = "";
        x.email = "";
        x.job = "";
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            string sql = @"SELECT Students.[Id], Students.[SchoolClass], SchoolClasses.[School], SchoolClasses.[SchoolClass], Students.[Teacher], Students.[FirstName], Students.[LastName], Students.[Gender], Students.[BirthDate], Students.[BirthPlace], Students.[Address], Students.[Height], Students.[Weight], Students.[FootSize], Students.[TShirtSize], Students.[ParentFirstName], Students.[ParentLastName], Students.[Phone], Students.[Email], Students.[Job] FROM Students
                            LEFT OUTER JOIN SchoolClasses
                            ON Students.[SchoolClass] = SchoolClasses.[Id]
                            ORDER BY Students.[Id] DESC";
            SqlCommand command = new SqlCommand(sql, connection);
            SqlDataReader reader = command.ExecuteReader();
            List<NewStudent> xx = new List<NewStudent>();
            while (reader.Read()) {
                NewStudent x = new NewStudent() {
                    id = reader.GetInt32(0),
                    schoolClassId = reader.GetInt32(1),
                    school = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    schoolClass = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                    teacher = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                    firstName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5),
                    lastName = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6),
                    gender = reader.GetInt32(7),
                    birthDate = reader.GetDateTime(8),
                    birthPlace = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9),
                    address = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10),
                    height = reader.GetInt32(11),
                    weight = reader.GetInt32(12),
                    footSize = reader.GetValue(13) == DBNull.Value ? "" : reader.GetString(13),
                    tShirtSize = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14),
                    parentFirstName = reader.GetValue(15) == DBNull.Value ? "" : reader.GetString(15),
                    parentLastName = reader.GetValue(16) == DBNull.Value ? "" : reader.GetString(16),
                    phone = reader.GetValue(17) == DBNull.Value ? "" : reader.GetString(17),
                    email = reader.GetValue(18) == DBNull.Value ? "" : reader.GetString(18),
                    job = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19)
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    [WebMethod]
    public string Save(NewStudent x) {
        if (x.id == null && Check(x) == false) {
            return ("Student je već registriran.");
        }
        else {
            try {
                connection.Open();
                string sql = "";
                SqlCommand command = new SqlCommand(sql, connection);
                if (x.id == null) {
                    sql = @"INSERT INTO Students
                            VALUES (@SchoolClass, @Teacher, @FirstName, @LastName, @Gender, @BirthDate, @BirthPlace, @Address, @Height, @Weight, @FootSize, @TShirtSize, @ParentFirstName, @ParentLastName, @Phone, @Email, @Job)";
                    command = new SqlCommand(sql, connection);
                }
                else {
                    sql = @"UPDATE Students SET  
                            SchoolClass = @SchoolClass, Teacher = @Teacher, FirstName = @FirstName, LastName = @LastName, Gender = @Gender, BirthDate = @BirthDate, BirthPlace = @BirthPlace, Address = @Address, Height = @Height, Weight = @Weight, FootSize = @FootSize, TShirtSize = @TShirtSize, ParentFirstName = @ParentFirstName, ParentLastName = @ParentLastName, Phone = @Phone, Email = @Email, Job = @Job
                            WHERE Id = @Id";
                    command = new SqlCommand(sql, connection);
                    command.Parameters.Add(new SqlParameter("Id", x.id));
                }
                command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClassId));
                command.Parameters.Add(new SqlParameter("Teacher", x.teacher));
                command.Parameters.Add(new SqlParameter("FirstName", x.firstName));
                command.Parameters.Add(new SqlParameter("LastName", x.lastName));
                command.Parameters.Add(new SqlParameter("Gender", x.gender));
                command.Parameters.Add(new SqlParameter("BirthDate", x.birthDate));
                command.Parameters.Add(new SqlParameter("BirthPlace", x.birthPlace));
                command.Parameters.Add(new SqlParameter("Address", x.address));
                command.Parameters.Add(new SqlParameter("Height", x.height));
                command.Parameters.Add(new SqlParameter("Weight", x.weight));
                command.Parameters.Add(new SqlParameter("FootSize", x.footSize));
                command.Parameters.Add(new SqlParameter("TShirtSize", x.tShirtSize));
                command.Parameters.Add(new SqlParameter("ParentFirstName", x.parentFirstName));
                command.Parameters.Add(new SqlParameter("ParentLastName", x.parentLastName));
                command.Parameters.Add(new SqlParameter("Phone", x.phone));
                command.Parameters.Add(new SqlParameter("Email", x.email));
                command.Parameters.Add(new SqlParameter("Job", x.job));
                command.ExecuteNonQuery();
                connection.Close();
                return ("Spremljeno.");
            } catch (Exception e) { return ("Registracija nije uspjela! (Error: )" + e); }
        }
    }

    protected bool Check(NewStudent x) {
        try {
            int count = 0;
            connection.Open();
            SqlCommand command = new SqlCommand(
                "SELECT COUNT([Id]) FROM Students WHERE SchoolClass = @SchoolClass AND FirstName = @FirstName AND LastName = @LastName AND ParentFirstName = @ParentFirstName AND ParentLastName = @ParentLastName ", connection);
            command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClassId));
            command.Parameters.Add(new SqlParameter("FirstName", x.firstName));
            command.Parameters.Add(new SqlParameter("LastName", x.lastName));
            command.Parameters.Add(new SqlParameter("ParentFirstName", x.firstName));
            command.Parameters.Add(new SqlParameter("ParentLastName", x.lastName));
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                count = reader.GetInt32(0);
            }
            connection.Close();
            if (count == 0) {
                return true;
            }
            else {
                return false;
            }
        } catch (Exception e) { return false; }
    }

    [WebMethod]
    public string Delete(NewStudent x) {
        try {
            connection.Open();
            string sql = @"DELETE Students WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Id", x.id));
            command.ExecuteNonQuery();
            connection.Close();
            return ("Izbrisano.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(Int32 id) {
        try {
            connection.Open();
            string sql = @"SELECT Students.[Id], Students.[SchoolClass], SchoolClasses.[School], SchoolClasses.[SchoolClass], Students.[Teacher], Students.[FirstName], Students.[LastName], Students.[Gender], Students.[BirthDate], Students.[BirthPlace], Students.[Address], Students.[Height], Students.[Weight], Students.[FootSize], Students.[TShirtSize], Students.[ParentFirstName], Students.[ParentLastName], Students.[Phone], Students.[Email], Students.[Job] FROM Students
                        LEFT OUTER JOIN SchoolClasses
                        ON Students.[SchoolClass] = SchoolClasses.[Id]
                        WHERE Students.[Id] = @Id";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            NewStudent x = new NewStudent();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetInt32(0);
                x.schoolClassId = reader.GetInt32(1);
                x.school = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.schoolClass = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.teacher = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.firstName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.lastName = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.gender = reader.GetInt32(7);
                x.birthDate = reader.GetDateTime(8);
                x.birthPlace = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.address = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.height = reader.GetInt32(11);
                x.weight = reader.GetInt32(12);
                x.footSize = reader.GetValue(13) == DBNull.Value ? "" : reader.GetString(13);
                x.tShirtSize = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                x.parentFirstName = reader.GetValue(15) == DBNull.Value ? "" : reader.GetString(15);
                x.parentLastName = reader.GetValue(16) == DBNull.Value ? "" : reader.GetString(16);
                x.phone = reader.GetValue(17) == DBNull.Value ? "" : reader.GetString(17);
                x.email = reader.GetValue(18) == DBNull.Value ? "" : reader.GetString(18);
                x.job = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

}
