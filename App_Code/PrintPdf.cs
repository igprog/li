using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Newtonsoft.Json;
using System.Text;



/// <summary>
/// Summary description for PrintPdf
/// </summary>
[WebService(Namespace = "http://igprog.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class PrintPdf : System.Web.Services.WebService {

    public PrintPdf() {
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    [WebMethod]
    public string RealizationsPdf(string foldername, string filename, string json) {
        var doc = new Document();
        List<Realizations.NewRealization> xx = new List<Realizations.NewRealization>();
        xx = JsonConvert.DeserializeObject<List<Realizations.NewRealization>>(json);

        string path = "~/UsersFiles/" + foldername + "/pdf/";
        CreateFolder(path);
        PdfWriter.GetInstance(doc, new FileStream(Server.MapPath(path + filename + ".pdf"), FileMode.Create));

        doc.Open();

        Font arial = FontFactory.GetFont("Arial", 8, Color.BLACK);
        Font arial16 = FontFactory.GetFont("Arial", 16, Color.CYAN);
        Font courier = new Font(Font.COURIER, 9f);
        Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
        Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));

        PdfPTable table = new PdfPTable(6);
        
        PdfPCell cell = new PdfPCell(new Phrase("Realizacija", verdana));
        cell.Colspan = 6;
        cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        cell.BackgroundColor = new Color(0, 179, 179);
        table.AddCell(cell);

        table.AddCell("Id");
        table.AddCell("GIR");
        table.AddCell("Škola");
        table.AddCell("Odjeljenje");
        table.AddCell("Trajanje");
        table.AddCell("Datum");
        foreach (var x in xx) {
            PdfPCell cell1 = new PdfPCell(new Phrase(x.id.ToString(), courier));
            cell1.Border = 0;
            table.AddCell(cell1);
            PdfPCell cell2 = new PdfPCell(new Phrase(x.type.ToString(), courier));
            cell2.Border = 0;
            table.AddCell(cell2);
            PdfPCell cell3 = new PdfPCell(new Phrase(x.school.ToString(), courier));
            cell3.Border = 0;
            table.AddCell(cell3);
            PdfPCell cell4 = new PdfPCell(new Phrase(x.schoolClass.ToString(), courier));
            cell4.Border = 0;
            table.AddCell(cell4);
            PdfPCell cell5 = new PdfPCell(new Phrase(x.duration.ToString(), courier));
            cell5.Border = 0;
            table.AddCell(cell5);
            PdfPCell cell6 = new PdfPCell(new Phrase(x.date.ToString(), courier));
            cell6.Border = 0;
            table.AddCell(cell6);
        }
        doc.Add(table);

        string text = @"Lorem ipsum dolor sit amet, civibus epicurei pericula cum te, cu eos audire denique. Ei electram voluptaria usu. Tale saperet te vim, sea meliore quaerendum scribentur ne, ad ridens corpora pro. Eam id purto cibo timeam, sale dissentias cu duo. Ex scaevola electram has, ei eius mazim nominati pri. Dolor expetendis est at. ";
        //StringBuilder sb = new StringBuilder();
        //sb.AppendLine(string.Format(@"
        //                    <div><h1>Test</h1>
        //                    <br/>
        //                    <p>Drugi red</p>
        //                    </div>"));

     //   text = sb.ToString();

        doc.Add(new Paragraph(text, brown));
        doc.Close();   
             
        return "OK.";
    }


}
