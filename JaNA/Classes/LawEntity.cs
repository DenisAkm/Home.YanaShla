using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaNA.Classes
{
    public class LawEntity: DataTable
    {
        public LawEntity(string name)
        {
            TableName = name;
            /*
             * Создание колонки Id записи.
             */
            DataColumn idColumn = new DataColumn("Id", Type.GetType("System.Int32"));
            idColumn.Unique = true;         // столбец будет иметь уникальное значение
            idColumn.AllowDBNull = false;   // не может принимать null
            idColumn.AutoIncrement = true;  // будет автоинкрементироваться
            idColumn.AutoIncrementSeed = 1; // начальное значение
            idColumn.AutoIncrementStep = 1; // приращении при добавлении новой строки
            Columns.Add(idColumn);
            /*
             * Создание колонки c текстом записи (Наименование закона).
             */
            DataColumn contextColumn = new DataColumn("Context", Type.GetType("System.String"));
            Columns.Add(contextColumn);
            /*
             * Создание колонки c адресом гиперссылки.
             */
            DataColumn linkColumn = new DataColumn("Link", Type.GetType("System.String"));            
            Columns.Add(linkColumn);
            /*
             * Создание колонки с Датой.
             */
            DataColumn datum = new DataColumn("Datum", Type.GetType("System.DateTime"));
            Columns.Add(datum);
            /*
             * Создание колонки-атрибута включения в отфильтрованный список.
             */
            DataColumn include = new DataColumn("Include", Type.GetType("System.Boolean"));
            include.DefaultValue = false;
            Columns.Add(include);
            /*
             * Первичный ключ таблицы.
             */
            PrimaryKey = new DataColumn[] { Columns["Id"] };
        }

        /// <summary>
        /// Метод-обработчик записей по ключевым словам.
        /// </summary>
        public void FindRecodrsByFilter()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "filter.txt");

            List<string> keyWordsList = new List<string>();
            if (File.Exists(path))
            {
                keyWordsList = GetKeyWords(path);
            }           

            foreach (DataRow row in Rows)
            {
                foreach (string item in keyWordsList)
                {
                    if (row["Context"].ToString().Contains(item))
                    {
                        row["Include"] = true;
                    }
                }                
            }
        }

        /// <summary>
        /// Получение списка ключевых слов для фильтрации.
        /// </summary>
        /// <param name="path">Путь к файлу с ключевыми словами.</param>
        /// <returns>Список ключевых слов.</returns>
        private List<string> GetKeyWords(string path)
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
