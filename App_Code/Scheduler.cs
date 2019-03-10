using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

/// <summary>
/// Scheduler
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Scheduler : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    public Scheduler() {
    }

    public class Event {
        public String content { get; set; }
        public Int64 startDate { get; set; }
        public Int64 endDate { get; set; }
        public Int32 room { get; set; }
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT [Content], [StartDate], [EndDate], [Room] FROM Scheduler", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<Event> events = new List<Event>();
            while (reader.Read()) {
                Event x = new Event();
                x.content = reader.GetString(0);
                x.startDate = reader.GetInt64(1);
                x.endDate = reader.GetInt64(2);
                x.room = reader.GetInt32(3);
                events.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(events, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(Event newEvent) {
        try {
            connection.Open();
            string sql = @"INSERT INTO Scheduler ([Content], [StartDate], [EndDate], [Room])
                        VALUES (@Content, @StartDate, @EndDate, @Room)";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Content", newEvent.content));
            command.Parameters.Add(new SqlParameter("StartDate", newEvent.startDate));
            command.Parameters.Add(new SqlParameter("EndDate", newEvent.endDate));
            command.Parameters.Add(new SqlParameter("Room", newEvent.room));
            command.ExecuteNonQuery();
            connection.Close();
            return ("OK.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Delete(Event newEvent) {
        try {
            connection.Open();
            string sql = @"DELETE Scheduler WHERE [Content] = @Content AND [StartDate] = @StartDate AND [Room] = @Room";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Content", newEvent.content));
            command.Parameters.Add(new SqlParameter("StartDate", newEvent.startDate));
            command.Parameters.Add(new SqlParameter("Room", newEvent.room));
            command.ExecuteNonQuery();
            connection.Close();
            return ("OK.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetSchedulerByRoom(Int32 room) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT [Content], [StartDate], [EndDate], [Room] FROM Scheduler WHERE [Room] = @Room", connection);
            command.Parameters.Add(new SqlParameter("Room", room));
            SqlDataReader reader = command.ExecuteReader();
            List<Event> events = new List<Event>();
            while (reader.Read()) {
                Event x = new Event();
                x.content = reader.GetString(0);
                x.startDate = reader.GetInt64(1);
                x.endDate = reader.GetInt64(2);
                x.room = reader.GetInt32(3);
                events.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(events, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }
    
}
