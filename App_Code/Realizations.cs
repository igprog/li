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
/// Realizazions
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Realizations : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
  //  Int32 timeDiff = Convert.ToInt32(ConfigurationManager.AppSettings["ServerHostTimeDifference"]);
    public Realizations() {

    }

    public class NewRealization {
        public Int32? id { get; set; }
        public Int32? schoolClassId { get; set; }
        public string school { get; set; }
        public string schoolClass { get; set; }
        public Int32? leaderId { get; set; }
        public string leaderFirstName { get; set; }
        public string leaderLastName { get; set; }
        public Int32? duration { get; set; }
        public DateTime? date { get; set; }
        public string description { get; set; }
        public string note { get; set; }
        public string substitute { get; set; }
        public Int32? type { get; set; }
        public List<Student> students { get; set; }

    }

    public class Student {
        public Int32 id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isSelected { get; set; }

    }

    [WebMethod]
    public string Init() {
        NewRealization x = new NewRealization();
        x.id = null;
        x.schoolClassId = null;
        x.school = "";
        x.schoolClass = "";
        x.leaderId = null;
        x.leaderFirstName = "";
        x.leaderLastName = "";
        x.duration = 0;
        x.date = DateTime.Today;
        x.description = "";
        x.note = "";
        x.substitute = "";
        x.type = 0;
        x.students = new List<Student>();
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT Realizations.[Id], Realizations.[SchoolClass], SchoolClasses.[School], SchoolClasses.[SchoolClass], Realizations.[Leader], Users.[FirstName], Users.[LastName], Realizations.[Duration], Realizations.[Date], Realizations.[Description], Realizations.[Note], Realizations.[Substitute], Realizations.[Type], Realizations.[Students] FROM Realizations
                                                LEFT OUTER JOIN SchoolClasses
                                                ON Realizations.[SchoolClass] = SchoolClasses.[Id]
                                                LEFT OUTER JOIN Users
                                                ON Realizations.[Leader] = Users.[UserId]
                                                ORDER BY Realizations.[Id] DESC", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<NewRealization> xx = new List<NewRealization>();
            while (reader.Read()) {
                NewRealization x = new NewRealization() {
                    id = reader.GetInt32(0),
                    schoolClassId = reader.GetInt32(1),
                    school = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    schoolClass = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                    leaderId = reader.GetInt32(4),
                    leaderFirstName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5),
                    leaderLastName = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6),
                    duration = reader.GetInt32(7),
                    date = reader.GetValue(8) == DBNull.Value ? DateTime.Today : reader.GetDateTime(8),  //.AddHours(timeDiff),   //(timeDiff) = server host time difference
                    description = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9),
                    note = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10),
                    substitute = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11),
                    type = reader.GetInt32(12),
                    students = JsonConvert.DeserializeObject<List<Student>>(reader.GetString(13)),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Greška: " + e); }
    }

    [WebMethod]
    public string Save(NewRealization x) {
            try {
            string students = JsonConvert.SerializeObject(x.students, Formatting.Indented);
            connection.Open();
            string sql = "";
            SqlCommand command = new SqlCommand(sql, connection);
            if (x.id == null) {
                sql = @"INSERT INTO Realizations ([SchoolClass], [Leader], [Duration], [Date], [Description], [Note], [Substitute], [Type], [Students])
                            VALUES (@SchoolClass, @Leader, @Duration, @Date, @Description, @Note, @Substitute, @Type, @Students)";
                command = new SqlCommand(sql, connection);
            } else {
                 sql = @"UPDATE Realizations SET  
                        [SchoolClass] = @SchoolClass, [Leader] = @Leader, [Duration] = @Duration, [Date] = @Date, [Description] = @Description, [Note] = @Note, [Substitute] = @Substitute, [Type] = @Type, [Students] = @Students
                        WHERE [Id] = @Id";
               
                command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("Id", x.id));
            }
            command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClassId));
            command.Parameters.Add(new SqlParameter("Leader", x.leaderId));
            command.Parameters.Add(new SqlParameter("Duration", x.duration));
            command.Parameters.Add(new SqlParameter("Date", x.date));
            command.Parameters.Add(new SqlParameter("Description", x.description));
            command.Parameters.Add(new SqlParameter("Note", x.note));
            command.Parameters.Add(new SqlParameter("Substitute", x.substitute));
            command.Parameters.Add(new SqlParameter("Type", x.type));
            command.Parameters.Add(new SqlParameter("Students", students));
            command.ExecuteNonQuery();
            connection.Close();
                return ("Spremljeno.");
            } catch (Exception e) { return ("Greška: " + e); }
    }

    protected bool CheckRealizations(NewRealization x) {
        try {
            int count = 0;
            connection.Open();
            SqlCommand command = new SqlCommand(
                "SELECT COUNT([Id]) FROM Realizations WHERE [SchoolClass] = @SchoolClass AND [Leader] = @Leader AND [Duration] = @Duration AND [Date] = @Date  ", connection);
            command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClassId));
            command.Parameters.Add(new SqlParameter("Leader", x.leaderId));
            command.Parameters.Add(new SqlParameter("Duration", x.duration));
            command.Parameters.Add(new SqlParameter("Date", x.date));
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
    public string Delete(NewRealization x) {
        try {
            connection.Open();
            string sql = @"DELETE FROM Realizations WHERE [Id] = @Id";
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
            SqlCommand command = new SqlCommand(@"SELECT Realizations.[Id], Realizations.[SchoolClass], SchoolClasses.[School], SchoolClasses.[SchoolClass], Realizations.[Leader], Users.[FirstName], Users.[LastName], Realizations.[Duration], Realizations.[Date], Realizations.[Description], Realizations.[Note], Realizations.[Substitute], Realizations.[Type], Realizations.[Students] FROM Realizations
                                                LEFT OUTER JOIN SchoolClasses
                                                ON Realizations.[SchoolClass] = SchoolClasses.[Id]
                                                LEFT OUTER JOIN Users
                                                ON Realizations.[Leader] = Users.[UserId]
                                                WHERE Realizations.[Id] = @Id", connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            NewRealization x = new NewRealization();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetInt32(0);
                x.schoolClassId = reader.GetInt32(1);
                x.school = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.schoolClass = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.leaderId = reader.GetInt32(4);
                x.leaderFirstName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.leaderLastName = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.duration = reader.GetInt32(7);
                x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today : reader.GetDateTime(8);  //.AddHours(timeDiff);
                x.description = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.note = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.substitute = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                x.type = reader.GetInt32(12);
                x.students = JsonConvert.DeserializeObject<List<Student>>(reader.GetString(13));
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }



}
