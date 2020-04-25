using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaNA.Classes
{
    public class DataTableFactory: DataTable
    {
        public DataTableFactory(string name)
        {
            TableName = name;
            DataColumn idColumn = new DataColumn("Id", Type.GetType("System.Int32"));
            idColumn.Unique = true; // столбец будет иметь уникальное значение
            idColumn.AllowDBNull = false; // не может принимать null
            idColumn.AutoIncrement = true; // будет автоинкрементироваться
            idColumn.AutoIncrementSeed = 1; // начальное значение
            idColumn.AutoIncrementStep = 1; // приращении при добавлении новой строки

            DataColumn contextColumn = new DataColumn("Context", Type.GetType("System.String"));
            DataColumn linkColumn = new DataColumn("Link", Type.GetType("System.String"));            
            DataColumn datum = new DataColumn("Datum", Type.GetType("System.DateTime"));
            DataColumn include = new DataColumn("Include", Type.GetType("System.Boolean"));
            include.DefaultValue = false;

            Columns.Add(idColumn);         //id
            Columns.Add(contextColumn);    //текст (наименование) закона
            Columns.Add(linkColumn);       //гиперссылка
            //this.Columns.Add(typeColumn);       //проект, акт, закон
            Columns.Add(datum);            //дата
            Columns.Add(include);          //фильтр включения

            // определяем первичный ключ таблицы books
            PrimaryKey = new DataColumn[] { Columns["Id"] };
        }

        public void Filter()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "filter.txt");
            List<string> originList = new List<string>();
            if (File.Exists(path))
            {
                originList = GetFilter(path);
            }
            

            foreach (DataRow row in Rows)
            {
                foreach (string item in originList)
                {
                    if (row["Context"].ToString().Contains(item))
                    {
                        row["Include"] = true;
                    }
                }                
            }
        }

        private List<string> GetFilter(string path)
        {
            var answer = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(path);
                while (!sr.EndOfStream)
                {                    
                    answer.Add(sr.ReadLine());
                }
                sr.Close();
                return answer;
            }
            catch (Exception)
            {
                return answer;                
            }            
        }
    }
}
