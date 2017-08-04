using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Happiness.Models;
using System.Web.Helpers;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
namespace Happiness.Controllers
{
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 30)]
        public ActionResult SummaryReport()
        {
            return View();
        }

        //public ActionResult GetHappyIndexSummary(DateTime? Fromdate ,DateTime? Todate)
        //{
        //    EmojidbEntities db = new EmojidbEntities();

        //    var pietest = db.GetHappyindexSummary(Fromdate, Todate).ToList();
            
        //    return Json(pietest.ToList(), JsonRequestBehavior.AllowGet);
        //}
        private static DataTable ConvertTable(DataTable table)
        {
            var dt = new DataTable();
            foreach (DataColumn column in table.Columns)
            {
                dt.Columns.Add(column.ColumnName.ToString(), typeof(string)).Caption = column.ColumnName;
            }

            for (var i = 0; i < table.Rows.Count; i++)
            {
                object[] ROW = new object[table.Rows[i].ItemArray.Length];
                DataRowCollection rowCollection = dt.Rows;
                for (var item = 0; item < table.Rows[i].ItemArray.Length; item++)
                {
                    if (!(table.Rows[i][item] == DBNull.Value))
                    {
                        ROW[item] = table.Rows[i][item];
                    }
                    else
                    {
                        ROW[item] = null;
                    }
                }
                dt.Rows.Add(ROW);
            }
            return dt;
        }
        public ActionResult GetsummaryLineChartMonth(DateTime? Fromdate, DateTime? Todate)
        {            
            UsersContext db = new UsersContext();
            string ConnectionString = db.Database.Connection.ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.ConnectTimeout = 2500;
            SqlConnection con = new SqlConnection(builder.ConnectionString);
            SqlDataReader sqlReader;
            DataTable dt = new DataTable();
            con.Open();
            using (SqlCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "GetsummaryLineChartMonth";
                cmd.Parameters.AddWithValue("@fromdate", Fromdate);
                cmd.Parameters.AddWithValue("@todate", Todate);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;

                sqlReader = cmd.ExecuteReader();
                dt.Load(sqlReader);
            }
            var Data = new Bortosky.Google.Visualization.GoogleDataTable(dt).GetJson();
           //  var result = ConvertDataTable<object>(dt);
            //string pie = DataTableToJSONWithJSONNet(dt);
            List<object> data = new List<object>();
          // // string dat = DataTableToJSONWithJSONNet(dt);
          ////  data = ConvertDataTable<object>(dt).ToList();
            ////data.Add(new[] { "Day", "Kasse", "Bonds", "Stocks", "Futures", "Options" });
            ////data.Add(new[] { 01.03, 200, 500, 100, 0, 10 });
            ////data.Add(new[] { 01.03, 300, 450, 150, 50, 30 });
            ////data.Add(new[] { 12.15, 350, 200, 180, 80, 40 });
          //  return Json(data);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        public string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }  
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }  
        public ActionResult CreateLine()
        {
            //Create bar chart
            //var db = Database.Open("DBName");
            //var data = db.Query("SELECT Col1, Col2 FROM Table");
            //var myChart = new Chart(width: 600, height: 400)
            //    .AddTitle("Chart Title")
            //    .DataBindTable(dataSource: data, xField: "Col1")
            //    .Write();
            var chart = new Chart(width: 600, height: 200, theme: ChartTheme.Vanilla3D)
            .AddSeries(chartType: "Pie",
                            xValue: new[] { "10 ", "50", "30 ", "70" },
                            yValues: new[] { "50", "70", "90", "110" })
                            .GetBytes("png");
            return File(chart, "image/bytes");
        }

        public ActionResult TestReport()
        {
            return View();
        }
    }
}
