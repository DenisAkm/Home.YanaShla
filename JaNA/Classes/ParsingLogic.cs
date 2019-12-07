using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace JaNA.Classes
{
    public static class ParsingLogic
    {
        
        static public DataTableFactory data;
        static private int Days { get; set; } = 1;

        static IWebDriver GetNewDriver()
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            
            return new ChromeDriver(driverService);
        }
        public static async void RunParseAsync(int days)
        {
            await Task.Run(() =>
            {
                Days = days;

                List<DataTableFactory> dataList = new List<DataTableFactory>();
                List<Task> taskList = new List<Task>();

                if (Form1.Application.checkBoxAsozd.Checked)
                {                    
                    Task task = new Task(() => dataList.Add(ParseAsozd(Days)));
                    taskList.Add(task);
                    task.Start();                    
                }
                if (Form1.Application.checkBoxConsultant.Checked)
                {                 
                    Task task = new Task(() => dataList.Add(ParseConsultant(Days)));                    
                    taskList.Add(task);
                    task.Start();
                }                
                Task.WaitAll(taskList.ToArray());
                data = MergeTables(dataList);                
            });
            WriteToList();
        }

        private static DataTableFactory MergeTables(List<DataTableFactory> datalist)
        {
            DataTableFactory mertgedtable = new DataTableFactory("Main");
            foreach (var table in datalist)
            {
                foreach (DataRow r in table.Rows)
                {
                    mertgedtable.Rows.Add(null, r.ItemArray[1], r.ItemArray[2], r.ItemArray[3], r.ItemArray[4]);
                }                
            }
            return mertgedtable;
        }
        
        private static void WriteToList()
        {
            for (int i = 0; i < ParsingLogic.data.Rows.Count; i++)
            {
                string context = ParsingLogic.data.Rows[i].Field<string>("Context");
                Form1.Application.checkedListBox1.Items.Add($"{context}", true);
            }
        }

        private static void Searching(IWebDriver driver, ref DataTableFactory data, int days, string patch)
        {
            for (int i = 0; i < days / 2 + 1; i++)
            {
                if (i != 0)
                {
                    //Прокликивание вкладок на 2 дня назад
                    driver.FindElement(By.XPath(@".//*[@id='right-col']/table/tbody/tr/td[1]/a/font")).Click();
                }

                for (int j = 2; j > 0; j--)
                {
                    // путь до элементов таблицы
                    var elements = driver.FindElements(By.XPath($@"/ html / body / form / div / div / div[2] / table / tbody / tr / td[2] / p / table[2] / tbody / tr / td[{j}] / table[2] / tbody / tr"));
                    foreach (var element in elements)
                    {
                        if (element.Text.Length > 5)
                        {
                            var text = element.FindElement(By.XPath(@".// td[2] / font ")).Text.Replace("\r", "").Replace("\n", " \"");
                            var link = element.FindElement(By.XPath(@".// td[2] / font / a ")).GetAttribute("href");
                            data.Rows.Add(new object[] { null, $"{patch} {text}\"", link });
                        }
                    }
                }
            }
        }

        public static async Task<DataTableFactory> ParseAsozdAsync(int days)
        {
            DataTableFactory data = new DataTableFactory("Asozd");
            await Task.Run(() =>
            {
                IWebDriver driver = GetNewDriver();
                //Идём на главную
                string urlPath = @"http://asozd.duma.gov.ru";
                driver.Url = urlPath;
                //С главной уходим на Перечень законопроектов, внесённых в Государственную Думу
                driver.FindElement(By.XPath(@"/ html / body / div / div / div[2] / table / tbody / tr / td[2] / div / div / div / ul / li[1] / a")).Click();
                //Перебираем по двум дням и 24 часам в Перечене законопроектов
                Searching(driver, ref data, days, "Проект Федерального закона №");
                //Идём на главную
                driver.Url = urlPath;
                //С главной уходим на Проекты постановлений
                driver.FindElement(By.XPath(@"/ html / body / div / div / div[2] / table / tbody / tr / td[2] / div / div / div / ul / li[2] / a")).Click();
                //Перебираем по двум дням и 24 часам в Перечене Проектов постановлений
                Searching(driver, ref data, days, "Проект постановления №");
                driver?.Quit();
            });
            return data;
        }

        public static async Task<DataTableFactory> ParseConsultantAsync(int days)
        {
            DataTableFactory data = new DataTableFactory("Consultant");
            await Task.Run(() => {
                IWebDriver driver = GetNewDriver();
                //Идём в роздел горячие документы
                string urlPath = @"http://www.consultant.ru/law/hotdocs/";
                driver.Url = urlPath;

                //устанавливаем дату
                driver.FindElement(By.XPath(@".//*[@class='btn-calendar']")).Click();
                DateTime finishTime = DateTime.Now;
                DateTime startTime = finishTime.AddDays(-days);
                driver.FindElement(By.XPath(@".//*[@id='start_date']")).SendKeys(startTime.ToString("d"));
                driver.FindElement(By.XPath(@".//*[@id='end_date']")).SendKeys(finishTime.ToString("d"));
                driver.FindElement(By.XPath(@".//*[@id='interim']/button")).Click();
                //Считаем количество страниц 
                int pages = driver.FindElements(By.XPath(@".//*[@class='pagination']/li")).Count;
                if (pages == 0)
                {
                    pages = 1;
                }
                else
                {
                    pages = pages / 2 - 1;
                }
                for (int i = 0; i < pages; i++)
                {
                    //получаем список законов
                    var list = driver.FindElements(By.XPath(@".//*[@class='hditem']"));

                    //записываем всё найденное
                    for (int j = 0; j < list.Count; j++)
                    {
                        var element = list[j].FindElement(By.XPath(@".//*[@class='link']/a"));
                        string text = element.Text.Replace("\r", "");
                        string link = element.GetAttribute("href");
                        data.Rows.Add(new object[] { null, text, link });
                    }
                    if (i != (pages - 1) && pages > 1)
                    {
                        var item = driver.FindElement(By.XPath($@".//*[@class='pagination']/li[{i + 3}]/a"));
                        driver.Url = item.GetAttribute("href");
                    }
                }
                driver?.Quit();
            });            
            return data;
        }

        public static DataTableFactory ParseAsozd(int days)
        {
            DataTableFactory data = new DataTableFactory("Asozd");
            IWebDriver driver = GetNewDriver();
            //Идём на главную
            string urlPath = @"http://asozd.duma.gov.ru";
            driver.Url = urlPath;
            //С главной уходим на Перечень законопроектов, внесённых в Государственную Думу
            driver.FindElement(By.XPath(@"/ html / body / div / div / div[2] / table / tbody / tr / td[2] / div / div / div / ul / li[1] / a")).Click();
            //Перебираем по двум дням и 24 часам в Перечене законопроектов
            Searching(driver, ref data, days, "Проект Федерального закона №");
            //Идём на главную
            driver.Url = urlPath;
            //С главной уходим на Проекты постановлений
            driver.FindElement(By.XPath(@"/ html / body / div / div / div[2] / table / tbody / tr / td[2] / div / div / div / ul / li[2] / a")).Click();
            //Перебираем по двум дням и 24 часам в Перечене Проектов постановлений
            Searching(driver, ref data, days, "Проект постановления №");
            driver?.Quit();
            return data;
        }
        public static DataTableFactory ParseConsultant(int days)
        {
            DataTableFactory data = new DataTableFactory("Consultant");
            IWebDriver driver = GetNewDriver();
            //Идём в роздел горячие документы
            string urlPath = @"http://www.consultant.ru/law/hotdocs/";
            driver.Url = urlPath;

            //устанавливаем дату
            driver.FindElement(By.XPath(@".//*[@class='btn-calendar']")).Click();
            DateTime finishTime = DateTime.Now;
            DateTime startTime = finishTime.AddDays(-days);
            driver.FindElement(By.XPath(@".//*[@id='start_date']")).SendKeys(startTime.ToString("d"));
            driver.FindElement(By.XPath(@".//*[@id='end_date']")).SendKeys(finishTime.ToString("d"));
            driver.FindElement(By.XPath(@".//*[@id='interim']/button")).Click();
            //Считаем количество страниц 
            int pages = driver.FindElements(By.XPath(@".//*[@class='pagination']/li")).Count;
            if (pages == 0)
            {
                pages = 1;
            }
            else
            {
                pages = pages / 2 - 1;
            }
            for (int i = 0; i < pages; i++)
            {
                //получаем список законов
                var list = driver.FindElements(By.XPath(@".//*[@class='hditem']"));

                //записываем всё найденное
                for (int j = 0; j < list.Count; j++)
                {
                    var element = list[j].FindElement(By.XPath(@".//*[@class='link']/a"));
                    string text = element.Text.Replace("\r", "");
                    string link = element.GetAttribute("href");
                    data.Rows.Add(new object[] { null, text, link });
                }
                if (i != (pages - 1) && pages > 1)
                {
                    var item = driver.FindElement(By.XPath($@".//*[@class='pagination']/li[{i + 3}]/a"));
                    driver.Url = item.GetAttribute("href");
                }
            }
            driver?.Quit();
            return data;
        }
    }
}








//public static void ParseAsozdShort(ref DataTableFactory data)
//{
//    driver.Url = @"http://asozd.duma.gov.ru/";

//    var text = driver.FindElements(By.XPath(@".//div[@class='news-block']"))[4].FindElements(By.XPath(@".//div[@class='news-block-item']"));
//    var links = driver.FindElements(By.XPath(@".//div[@class='news-block']"))[4].FindElements(By.XPath(@".//div[@class='news-block-item']/a"));

//    for (int i = 0; i < links.Count; i++)
//    {
//        string[] arr = text[i].Text.Split('\n');
//        data.Rows.Add(new object[] { null, arr[2], links[i].GetAttribute("href"), $"Проект Федерального закона № {arr[1]}" });  //Form1.Application.listBox1.Items.Add($"{arr[2]} - {links[i].GetAttribute("href")}");
//    }

//    text = driver.FindElements(By.XPath(@".//div[@class='news-block']"))[5].FindElements(By.XPath(@".//div[@class='news-block-item']"));
//    links = driver.FindElements(By.XPath(@".//div[@class='news-block']"))[5].FindElements(By.XPath(@".//div[@class='news-block-item']/a"));

//    for (int i = 0; i < links.Count; i++)
//    {
//        string[] arr = text[i].Text.Split('\n');
//        data.Rows.Add(new object[] { null, arr[2], links[i].GetAttribute("href"), $"Проект постановления № {arr[1]}" });  //Form1.Application.listBox1.Items.Add($"{arr[2]} - {links[i].GetAttribute("href")}");
//    }
//}
