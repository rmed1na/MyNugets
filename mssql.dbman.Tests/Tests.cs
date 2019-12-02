using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using TextLogs;

namespace mssql.dbman.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void SetConnection_Test()
        {
            bool connected = false;
            Log log = new Log();
            MSSQLServer db = new MSSQLServer(log: log);
            db.server = "172.22.1.34";
            db.database = "ISHOPPE";
            db.user = "UserApp";
            db.password = "EsbTxt7b";
            connected = db.SetConnection();
            Assert.IsTrue(connected);
        }

        [TestMethod]
        public void GetData_Test()
        {
            Log log = new Log();
            bool connected = false;
            DataTable dt = new DataTable();
            MSSQLServer db = new MSSQLServer(log: log);
            db.server = "172.22.1.34";
            db.database = "ISHOPPE";
            db.user = "UserApp";
            db.password = "EsbTxt7b";
            db.debugMode = true;
            connected = db.SetConnection();

            dt = db.GetData(query: "SELECT * FROM Sucursal");
            foreach (DataRow row in dt.Rows)
                log.Write($"{row["Nombre"].ToString()}");

            Assert.IsTrue(connected);
            Assert.IsTrue(dt.Rows.Count > 1);
        }

        [TestMethod]
        public void WriteData_Test()
        {
            bool gotData = false;
            Log log = new Log();
            MSSQLServer db = new MSSQLServer(log: log);
            db.server = "172.22.1.34";
            db.database = "ISHOPPE";
            db.user = "UserApp";
            db.password = "EsbTxt7b";
            db.debugMode = true;
            db.SetConnection();
            gotData = db.WriteData(query: "UPDATE Empresa SET UltimoCambio = GETDATE()");
            Assert.IsTrue(gotData);
        }

        [TestMethod]
        public void CheckColumns_Test()
        {
            bool done = false;
            Log log = new Log();
            MSSQLServer db = new MSSQLServer(log: log);
            DataTable dt = new DataTable("Empresa");
            db.server = "172.22.1.34";
            db.database = "ISHOPPE";
            db.user = "UserApp";
            db.password = "EsbTxt7b";
            db.debugMode = false;
            db.SetConnection();

            dt.Columns.Add("Empresa", typeof(string));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Grupo", typeof(string));
            dt.Columns.Add("Direccion", typeof(string));

            //done = db.CheckColumns(dt, dt.TableName); //set to public for tests
            Assert.IsTrue(done);
        }

        [TestMethod]
        public void BulkInsert_Test()
        {
            bool success = false;
            Log log = new Log();
            MSSQLServer db = new MSSQLServer(log: log);
            DataTable dt = new DataTable();
            db.server = "172.22.1.34";
            db.database = "DESARROLLOINTELISIS";
            db.user = "UserApp";
            db.password = "EsbTxt7b";
            db.SetConnection();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Texto", typeof(string));
            dt.Columns.Add("Plata", typeof(decimal));
            dt.Rows.Add(1, "ROLANDO MEDINA", 34.56);
            dt.Rows.Add(2, "NOMBRE APELLIDO", 100.00);
            dt.Rows.Add(3, "DESARROLLO INTELISIS", 0.04);

            success = db.BulkInsert(dt, "PRUEBADOTNET");
            Assert.IsTrue(success);
        }
    }
}
