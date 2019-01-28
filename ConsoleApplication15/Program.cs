using System.Data.SqlServerCe;
using System.IO;

namespace ConsoleApplication15
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString;
            string fileName = "db.sdf";
            string password = "db";
            
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            connectionString = string.Format("DataSource =\"{0}\"; Password = '{1}'", fileName, password);

            SqlCeEngine en = new SqlCeEngine(connectionString);
            en.CreateDatabase();
        }
    }
}
