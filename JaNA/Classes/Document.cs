using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace JaNA.Classes
{
    class Document
    {
        private DocX GetRejectionLetterTemplate()
        {
            // Adjust the path so suit your machine:
            string fileName = @"D:\Users\John\Documents\DocXExample.docx";

            // Set up our paragraph contents:
            string headerText = "Rejection Letter";
            string letterBodyText = DateTime.Now.ToShortDateString();
            string paraTwo = ""
                + "Dear %APPLICANT%" + Environment.NewLine + Environment.NewLine
                + "I am writing to thank you for your resume. Unfortunately, your skills and "
                + "experience do not match our needs at the present time. We will keep your "
                + "resume in our circular file for future reference. Don't call us, "
                + "we'll call you. "

                + Environment.NewLine + Environment.NewLine
                + "Sincerely, "
                + Environment.NewLine + Environment.NewLine
                + "Jim Smith, Corporate Hiring Manager";

            // Title Formatting:
            var titleFormat = new Formatting();
            titleFormat.FontFamily = new Xceed.Document.NET.Font("Arial Black");
            titleFormat.Size = 18D;
            titleFormat.Position = 12;

            // Body Formatting
            var paraFormat = new Formatting();
            paraFormat.FontFamily = new Xceed.Document.NET.Font("Times New Roman");
            paraFormat.Size = 10D;
            titleFormat.Position = 12;

            // Create the document in memory:
            var doc = DocX.Create(fileName);

            // Insert each prargraph, with appropriate spacing and alignment:
            Paragraph title = doc.InsertParagraph(headerText, false, titleFormat);
            title.Alignment = Alignment.center;

            doc.InsertParagraph(Environment.NewLine);
            Paragraph letterBody = doc.InsertParagraph(letterBodyText, false, paraFormat);
            letterBody.Alignment = Alignment.both;


            doc.InsertParagraph(Environment.NewLine);
            doc.InsertParagraph(paraTwo, false, paraFormat);

            return doc;
        }

        internal void CreateNumberdList()
        {
            string name = "DocXExampleList.docx";
            string fileName = Path.Combine(Environment.CurrentDirectory, name);
            // Create the document in memory:
            var doc = DocX.Create(fileName);
            doc.SetDefaultFont(new Xceed.Document.NET.Font("Times New Roman"), 14);
            var paragraph = doc.InsertParagraph();

            var listType = ListItemType.Numbered;
            var list = doc.AddList(listType: ListItemType.Numbered, startNumber: 1);

            doc.AddListItem(list, "Number 1", 0, listType);
            doc.AddListItem(list, "Number 2", 0, listType);

            doc.InsertList(list);
            Paragraph par = doc.InsertParagraph(); //just to get some space between.

            var secondList = doc.AddList(listType: ListItemType.Numbered, startNumber: 1);

            doc.AddListItem(secondList, "Number 1", 0, listType);
            doc.AddListItem(secondList, "Number 2", 0, listType);

            doc.InsertList(secondList);
            
            List l = doc.Lists[0];
            
            Hyperlink hl = doc.AddHyperlink("some hyperlink", new Uri("https://stackoverflow.com"));
            
            l.Items[1].AppendHyperlink(hl);
            par.InsertHyperlink(hl);
            // Save to the output directory:
            doc.Save();

            // Open in Word:
            Process.Start("WINWORD.EXE", name);
        }

        public void CreateSampleDocument()
        {
            string name = "DocXExample.docx";
            string fileName = Path.Combine(Environment.CurrentDirectory, name);
            string headlineText = "Constitution of the United States";
            string paraOne = ""
                + "We the People of the United States, in Order to form a more perfect Union, "
                + "establish Justice, insure domestic Tranquility, provide for the common defence, "
                + "promote the general Welfare, and secure the Blessings of Liberty to ourselves "
                + "and our Posterity, do ordain and establish this Constitution for the United "
                + "States of America.";

            // A formatting object for our headline:
            var headLineFormat = new Formatting();
            headLineFormat.FontFamily = new Xceed.Document.NET.Font("Arial Black");
            headLineFormat.Size = 18D;
            headLineFormat.Position = 12;

            // A formatting object for our normal paragraph text:
            var paraFormat = new Formatting();
            paraFormat.FontFamily = new Xceed.Document.NET.Font("Times New Roman");
            paraFormat.Size = 10D;
            
            // Create the document in memory:
            var doc = DocX.Create(fileName);

            // Insert the now text obejcts;
            doc.InsertParagraph(headlineText, false, headLineFormat);
            doc.InsertParagraph(paraOne, false, paraFormat);

            // Save to the output directory:
            doc.Save();

            // Open in Word:
            Process.Start("WINWORD.EXE", name);
        }
        public void CreateRejectionLetter(string applicantField, string applicantName)
        {
            // Modify to suit your machine:
            string nameTemplate = "Rejection-Letter-{0}-{1}";

            // Let's save the file with a meaningful name, including the 
            // applicant name and the letter date:
            string outputName = string.Format(nameTemplate, applicantName, DateTime.Now.ToString("dd-MM-yyyy"));

            // We will need a file name for our output file (change to suit your machine):            
            string fileName = Path.Combine(Environment.CurrentDirectory, outputName) + ".docx";

            // Grab a reference to our document template:
            DocX letter = this.GetRejectionLetterTemplate();

            // Perform the replace:
            letter.ReplaceText(applicantField, applicantName);
            // Save as New filename:
            letter.SaveAs(fileName);
            // Open in word:         
            Process p = new Process();
            p.StartInfo.FileName = fileName;
            p.Start();

        }
                

        public void CreateReportDocument(DataTableFactory data)
        {
            string name = "DocXExample.docx";
            string fileName = Path.Combine(Environment.CurrentDirectory, name);

            // Create the document in memory:
            var doc = DocX.Create(fileName);
            doc.SetDefaultFont(new Xceed.Document.NET.Font("Times New Roman"), 14);
            
            var listType = ListItemType.Numbered;
            var list = doc.AddList(listType: ListItemType.Numbered, startNumber: 1);
            
            // Основной заголовок;
            Paragraph headparagraph = doc.InsertParagraph("Основные изменения нормативно-правовых актов");
            headparagraph.Bold().SetLineSpacing(LineSpacingType.Line, 2.5f);
            headparagraph.Alignment = Alignment.center;
            
            foreach (DataRow row in data.Rows)
            {
                if (row.Field<bool>("Include"))
                {
                    object[] cells = row.ItemArray;
                    string context = ModifyToText(cells[1]);                    

                    // создаём ссылку
                    Hyperlink hyperlinkBlog = doc.AddHyperlink($"{context}", new Uri((string)cells[2]));
                    
                    doc.AddListItem(list, "", 0, listType);
                    list.Items[list.Items.Count - 1].AppendHyperlink(hyperlinkBlog);
                    list.Items[list.Items.Count - 1].Alignment = Alignment.both;
                    list.Items[list.Items.Count - 1].SetLineSpacing(LineSpacingType.After, 1.5f);
                }
            }
            doc.InsertList(list);
            // Save to the output directory:
            doc.Save();

            // Open in Word:
            Process.Start("WINWORD.EXE", name);
        }
        string ModifyToText(object val)
        {
            string context = (string)val;
            if (context !="")
            {
                string lastChar = context.Substring(context.Length - 1);
                while (lastChar == " " || lastChar == "\r")
                {
                    context = context.Remove(context.Length - 1);
                    lastChar = context.Substring(context.Length - 1);
                }
                context.Replace("\"\"", "\"");
            }
            return context;
        }
    }
    
}
