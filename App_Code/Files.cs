using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;

/// <summary>
/// Save data to files
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Files : System.Web.Services.WebService {
    public Files() {
    }

    [WebMethod]
    public string SaveJsonToFile(string foldername, string filename, string json) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" +  filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
            return GetFile(foldername, filename);
        } catch(Exception e) { return ("Error: " + e); }
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    [WebMethod]
    public String GetFile(string foldername, string filename) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" + filename + ".json";
            string value = File.ReadAllText(Server.MapPath(filepath));
            return value;
        }
        catch (Exception e) { return ("Error: " + e); }
    }


}
