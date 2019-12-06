using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaNA.Classes
{
    public class DataTableFactory: DataTable
    {
        public DataTableFactory(string name)
        {
            this.TableName = name;
            DataColumn idColumn = new DataColumn("Id", Type.GetType("System.Int32"));
            idColumn.Unique = true; // столбец будет иметь уникальное значение
            idColumn.AllowDBNull = false; // не может принимать null
            idColumn.AutoIncrement = true; // будет автоинкрементироваться
            idColumn.AutoIncrementSeed = 1; // начальное значение
            idColumn.AutoIncrementStep = 1; // приращении при добавлении новой строки

            DataColumn contextColumn = new DataColumn("Context", Type.GetType("System.String"));
            DataColumn linkColumn = new DataColumn("Link", Type.GetType("System.String"));
            //DataColumn typeColumn = new DataColumn("TypeAndNumber", Type.GetType("System.String"));
            DataColumn datum = new DataColumn("Datum", Type.GetType("System.DateTime"));
            DataColumn include = new DataColumn("Include", Type.GetType("System.Boolean"));
            include.DefaultValue = true;

            this.Columns.Add(idColumn);         //id
            this.Columns.Add(contextColumn);    //текст (наименование) закона
            this.Columns.Add(linkColumn);       //гиперссылка
            //this.Columns.Add(typeColumn);       //проект, акт, закон
            this.Columns.Add(datum);            //дата
            this.Columns.Add(include);          //фильтр включения

            // определяем первичный ключ таблицы books
            this.PrimaryKey = new DataColumn[] { this.Columns["Id"] };
        }
        
    }
}
