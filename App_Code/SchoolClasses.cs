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
/// SchoolClasses
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class SchoolClasses : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

    public SchoolClasses() {
    }

    public class NewSchoolClass {
        public Int32? id { get; set; }
        public string school { get; set; }
        public string schoolClass { get; set; }
        public Int32? leaderId { get; set; }
        public string leaderFirstName { get; set; }
        public string leaderLastName { get; set; }
        public Int32 defaultHours { get; set; }
        public string teacher { get; set; }
        public Int32 totalHours { get; set; }
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewSchoolClass x = new NewSchoolClass();
        x.id = null;
        x.school = "";
        x.schoolClass = "";
        x.leaderId = null;
        x.leaderFirstName = "";
        x.leaderLastName = "";
        x.defaultHours = 0;
        x.teacher = "";
        x.totalHours = 0;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT SchoolClasses.[Id], SchoolClasses.[School], SchoolClasses.[SchoolClass], SchoolClasses.[Leader], Users.[FirstName], Users.[LastName], SchoolClasses.[DefaultHours], SchoolClasses.[Teacher], SUM(Realizations.[Duration])     
                                                FROM SchoolClasses                                  
                                                LEFT OUTER JOIN Users
                                                ON SchoolClasses.[Leader] = Users.[UserId]
                                                LEFT OUTER JOIN Realizations
                                                ON SchoolClasses.[Id] = Realizations.[SchoolClass]
                                                GROUP BY SchoolClasses.[Id], SchoolClasses.[School], SchoolClasses.[SchoolClass], SchoolClasses.[Leader], Users.[FirstName], Users.[LastName], SchoolClasses.[DefaultHours], SchoolClasses.[Teacher]
                                                ORDER BY SchoolClasses.[School] ASC", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<NewSchoolClass> xx = new List<NewSchoolClass>();
            while (reader.Read()) {
                NewSchoolClass x = new NewSchoolClass() {
                    id = reader.GetInt32(0),
                    school = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    schoolClass = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    leaderId = reader.GetInt32(3),
                    leaderFirstName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                    leaderLastName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5),
                    defaultHours = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6),
                    teacher = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7),
                    totalHours = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    [WebMethod]
    public string Save(NewSchoolClass x) {
        if (x.id == null && Check(x) == false) {
            return ("Odjeljenje je već registrirano.");
        }
        else {
            try {
                connection.Open();
                string sql = "";
                SqlCommand command = new SqlCommand(sql, connection);
                if (x.id == null) {
                    sql = @"INSERT INTO SchoolClasses ([School], [SchoolClass], [Leader], [DefaultHours], [Teacher]) 
                            VALUES (@School, @SchoolClass, @Leader, @DefaultHours, @Teacher)";
                    command = new SqlCommand(sql, connection);
                }
                else {
                    sql = @"UPDATE SchoolClasses SET  
                            School = @School, SchoolClass = @SchoolClass, Leader= @Leader, DefaultHours= @DefaultHours, @Teacher = Teacher
                            WHERE Id = @Id";
                    command = new SqlCommand(sql, connection);
                    command.Parameters.Add(new SqlParameter("Id", x.id));
                }
                command.Parameters.Add(new SqlParameter("School", x.school));
                command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClass));
                command.Parameters.Add(new SqlParameter("Leader", x.leaderId));
                command.Parameters.Add(new SqlParameter("DefaultHours", x.defaultHours));
                command.Parameters.Add(new SqlParameter("Teacher", x.teacher));
                command.ExecuteNonQuery();
                connection.Close();
                return ("Spremljeno.");
            } catch (Exception e) { return ("Registracija nije uspjela! (Error: )" + e); }
        }
    }

    [WebMethod]
    public string Get(int id) {
        try {
            connection.Open();
            string sql = @"SELECT SchoolClasses.[Id], SchoolClasses.[School], SchoolClasses.[SchoolClass], SchoolClasses.[Leader], Users.[FirstName], Users.[LastName], SchoolClasses.[DefaultHours], SchoolClasses.[Teacher], SUM(Realizations.[Duration])     
                        FROM SchoolClasses                                  
                        LEFT OUTER JOIN Users
                        ON SchoolClasses.[Leader] = Users.[UserId]
                        LEFT OUTER JOIN Realizations
                        ON SchoolClasses.[Id] = Realizations.[SchoolClass]
                        WHERE SchoolClasses.[Id] = @Id
                        GROUP BY SchoolClasses.[Id], SchoolClasses.[School], SchoolClasses.[SchoolClass], SchoolClasses.[Leader], Users.[FirstName], Users.[LastName], SchoolClasses.[DefaultHours], SchoolClasses.[Teacher]
                        ORDER BY SchoolClasses.[School] ASC";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            SqlDataReader reader = command.ExecuteReader();
            NewSchoolClass x = new NewSchoolClass();
            while (reader.Read()) {
                x.id = reader.GetInt32(0);
                x.school = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.schoolClass = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.leaderId = reader.GetInt32(3);
                x.leaderFirstName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.leaderLastName = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.defaultHours = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6);
                x.teacher = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.totalHours = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
            };
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    [WebMethod]
    public string Delete(NewSchoolClass x) {
        try {
            connection.Open();
            string sql = @"DELETE SchoolClasses WHERE Id = @Id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Id", x.id));
            command.ExecuteNonQuery();
            connection.Close();
            return ("Izbrisano.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    public class School {
        public string name { get; set; }
    }

    [WebMethod]
    public string GetSchools() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT DISTINCT SchoolClasses.[School] FROM SchoolClasses
                                                ORDER BY SchoolClasses.[School] ASC", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<School> xx = new List<School>();
            while (reader.Read()) {
                School x = new School() {
                    name = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Greška: " + e); }
    }


    public class SchoolClass {
        public Int32 id { get; set; }
        public string name { get; set; }

    }

    [WebMethod]
    public string GetSchoolClassesBySchool(string school) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT DISTINCT SchoolClasses.[Id], SchoolClasses.[SchoolClass] FROM SchoolClasses
                                                WHERE SchoolClasses.[School] = @School
                                                ORDER BY SchoolClasses.[SchoolClass] ASC", connection);
            command.Parameters.Add(new SqlParameter("School", school));
            SqlDataReader reader = command.ExecuteReader();
            List<SchoolClass> xx = new List<SchoolClass>();
            while (reader.Read()) {
                SchoolClass x = new SchoolClass() {
                    id = reader.GetInt32(0),
                    name = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    public class Teacher {
        public string name { get; set; }
    }

    [WebMethod]
    public string GetTeachersBySchoolClass(Int32 id) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT SchoolClasses.[Teacher] FROM SchoolClasses
                                                WHERE SchoolClasses.[Id] = @Id
                                                ORDER BY SchoolClasses.[Teacher] ASC", connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            SqlDataReader reader = command.ExecuteReader();
            List<Teacher> xx = new List<Teacher>();
            while (reader.Read()) {
                Teacher x = new Teacher() {
                    name = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    public class Leader {
        public Int32 id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

    }

    [WebMethod]
    public string GetLeadersBySchoolClass(Int32 id) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT SchoolClasses.[Leader], Users.[FirstName], Users.[LastName] FROM SchoolClasses
                                                LEFT OUTER JOIN Users
                                                ON SchoolClasses.Leader = Users.[UserId]
                                                WHERE SchoolClasses.[Id] = @Id
                                                ORDER BY Users.[FirstName] ASC", connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            SqlDataReader reader = command.ExecuteReader();
            List<Leader> xx = new List<Leader>();
            while (reader.Read()) {
                Leader x = new Leader() {
                    id = reader.GetInt32(0),
                    firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }

    public class Student {
        public Int32 id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isSelected { get; set; }

    }

    [WebMethod]
    public string GetStudentsBySchoolClass(Int32 id) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT Students.[Id], Students.[FirstName], Students.[LastName] FROM Students
                                                LEFT OUTER JOIN SchoolClasses
                                                ON Students.[SchoolClass] = SchoolClasses.[Id]
                                                WHERE SchoolClasses.[Id] = @Id
                                                ORDER BY Students.[LastName] ASC", connection);
            command.Parameters.Add(new SqlParameter("Id", id));
            SqlDataReader reader = command.ExecuteReader();
            List<Student> xx = new List<Student>();
            while (reader.Read()) {
                Student x = new Student() {
                    id = reader.GetInt32(0),
                    firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    isSelected = false
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Greška: " + e); }
    }
    #endregion

    #region Methods
    protected bool Check(NewSchoolClass x) {
        try {
            int count = 0;
            connection.Open();
            SqlCommand command = new SqlCommand(
                "SELECT COUNT([Id]) FROM SchoolClasses WHERE School = @School AND SchoolClass = @SchoolClass ", connection);
            command.Parameters.Add(new SqlParameter("School", x.school));
            command.Parameters.Add(new SqlParameter("SchoolClass", x.schoolClass));
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
    #endregion

}
