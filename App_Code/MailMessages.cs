using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

/// <summary>
/// SendMail
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MailMessages : System.Web.Services.WebService {
    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

    public MailMessages() { 
    }

    public class NewMail {
        public Int32? id { get; set; }
        public string email { get; set; }
        public List<GroupEmails> groupEmails { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public DateTime date { get; set; }
    }

    public class GroupEmails {
        public string email { get; set; }
    }

    [WebMethod]
    public string Init() {
        NewMail x = new NewMail();
        x.id = null;
        x.email = null;
        x.groupEmails = new List<GroupEmails>();
        x.subject = "";
        x.message = "";
        x.date = DateTime.Today;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    public class MailSettings {
        public string email { get; set; }
        public string password { get; set; }
        public string serverPort { get; set; }
        public string serverHost { get; set; }
    }


    [WebMethod]
    public string SendNewMail(NewMail mail, List<Group> groups, string sitename ) {
        mail.groupEmails = GetEmailsFromGroups(groups);
        mail.date = DateTime.Today;
        MailSettings settings = GetMailSettings(sitename);
        try {
            SmtpClient Smtp_Server = new SmtpClient();
            Smtp_Server.UseDefaultCredentials = false;
            Smtp_Server.Credentials = new NetworkCredential(settings.email, settings.password);
            Smtp_Server.Port = Convert.ToInt32(settings.serverPort);


            Smtp_Server.EnableSsl = true;
            Smtp_Server.Host = settings.serverHost;
            MailMessage mailMessage = new MailMessage();
            if (!string.IsNullOrWhiteSpace(mail.email)){
                mailMessage.To.Add(mail.email);
            }
            foreach (var x in mail.groupEmails) {
                mailMessage.To.Add(x.email);
            }
            mailMessage.From = new MailAddress(settings.email);
            mailMessage.Subject = mail.subject;
            mailMessage.Body = mail.message;
            Smtp_Server.Send(mailMessage);
            Save(mail);
            return ("Poruka uspješno poslana.");
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    public MailSettings GetMailSettings(string sitename) {
        Files file = new Files();
        string json = file.GetFile(sitename, "mailsettings");
        MailSettings settings = JsonConvert.DeserializeObject<MailSettings>(json);
        return settings;
    }

    public class Group {
        public string service { get; set; }
        public string option { get; set; }
        public bool isSelected { get; set; }
    }


    [WebMethod]
    public string GetGroups() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT DISTINCT [Service], [Option] FROM ClientServices
                                                ORDER BY [Service] ASC", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<Group> xx = new List<Group>();
            while (reader.Read()) {
                Group x = new Group() {
                    service = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                    option = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    isSelected = false
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    public List<GroupEmails> GetEmailsFromGroups(List<Group> groups) {
        List<GroupEmails> xx = new List<GroupEmails>();
        foreach (Group group in groups) {
            if(group.isSelected == true) {
                try {
                    connection.Open();
                    SqlCommand command = new SqlCommand(@"SELECT DISTINCT Clients.[Email] FROM ClientServices
                                                    LEFT OUTER JOIN Clients
                                                    ON ClientServices.[ClientId] = Clients.[ClientId]
                                                    WHERE ClientServices.[Service] = @Service AND ClientServices.[Option] = @Option ", connection);
                    command.Parameters.Add(new SqlParameter("Service", group.service));
                    command.Parameters.Add(new SqlParameter("Option", group.option));
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        GroupEmails x = new GroupEmails() {
                            email = reader.GetString(0),
                        };
                        xx.Add(x);
                    }
                    connection.Close();
                } catch (Exception e) { return null; }
            }
        }
        var distinctEmail = xx.GroupBy(x => x.email).Select(x => x.First()).ToList();
        return distinctEmail;
    }

    [WebMethod]
    public string Load() {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT * FROM MailMessages ORDER BY [Id] DESC", connection);
            List<NewMail> xx = new List<NewMail>();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewMail x = new NewMail() {
                    id = reader.GetInt32(0),
                    email = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    groupEmails = JsonConvert.DeserializeObject<List<GroupEmails>>(reader.GetString(2)),
                    subject = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3),
                    message = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4),
                    date = reader.GetValue(5) == DBNull.Value ? DateTime.Today : reader.GetDateTime(5)
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }


    public void Save(NewMail mail) {
        try {
            string groupEmails = JsonConvert.SerializeObject(mail.groupEmails, Formatting.Indented);
            connection.Open();
            string sql = @"INSERT INTO MailMessages VALUES  
                       (@Email, @GroupEmails, @Subject, @Message, @Date)";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("Email", mail.email));
            command.Parameters.Add(new SqlParameter("GroupEmails", groupEmails));
            command.Parameters.Add(new SqlParameter("Subject", mail.subject));
            command.Parameters.Add(new SqlParameter("Message", mail.message));
            command.Parameters.Add(new SqlParameter("Date", mail.date));
            command.ExecuteNonQuery();
            connection.Close();
        } catch (Exception e) { }
    }

    [WebMethod]
    public string GetMessage(NewMail mail) {
        try {
            connection.Open();
            SqlCommand command = new SqlCommand(@"SELECT * FROM MailMessages WHERE [Id] = @Id", connection);
            command.Parameters.Add(new SqlParameter("Id", mail.id));
            NewMail x = new NewMail();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetInt32(0);
                x.email = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.groupEmails = JsonConvert.DeserializeObject<List<GroupEmails>>(reader.GetString(2));
                x.subject = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.message = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.date = reader.GetValue(5) == DBNull.Value ? DateTime.Today : reader.GetDateTime(5);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }






}
