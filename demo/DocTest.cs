using just4net.doc;
using System;
using System.IO;

namespace demo
{
    public class DocTest
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;

        public void Test()
        {
            string doc = Path.Combine(dir, "doc.docx");
            Console.WriteLine("coverting document...");
            Test(doc, new WordUtil());

            string excel = Path.Combine(dir, "excel.xlsx");
            Console.WriteLine("coverting excel...");
            Test(excel, new ExcelUtil());

            string ppt = Path.Combine(dir, "ppt.pptx");
            Console.WriteLine("coverting ppt...");
            Test(ppt, new PPTUtil());
        }

        public string Test(string file, IPDFConversion conversion)
        {
            string pdf = Path.Combine(dir, Path.GetFileNameWithoutExtension(file) + ".pdf");
            conversion.ConvertToPDF(file, pdf);
            return pdf;
        }
    }
}
