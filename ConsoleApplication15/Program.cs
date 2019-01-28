using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Text;

namespace ConsoleApplication15
{
    class DatabaseInterface
    {
        private SqlCeConnection Connection
        {
            get; set;
        }
        private string Filename
        {
            get; set;
        }

        private string ConnectionString
        {
            get; set;
        }

        public DatabaseInterface(string filename, string password)
        {
            this.Filename = filename;
            this.ConnectionString = string.Format("DataSource =\"{0}\"; Password = '{1}'", filename, password);
        }

        public bool ConnectDB()
        {
            bool connected = false;

            if (!File.Exists(this.Filename))
            {
                CreateDB();
            }
            try
            {
                this.Connection = new SqlCeConnection(this.ConnectionString);
                connected = true;
            } 
            catch
            {
                Console.WriteLine("ConnectDB() failed.");
            }

            return connected;
        }

        public bool CreateDB()
        {
            bool created = false;
            try
            {
                SqlCeEngine en = new SqlCeEngine(this.ConnectionString);
                en.CreateDatabase();
                created = true;
            }
            catch
            {
                Console.WriteLine("CreateDB() failed.");
            }

            return created;
        }
        
        public bool CreateTables()
        {
            bool created = false;

            try
            {
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                }

                SqlCeCommand cmd;

                string sql = "create table CoolPeople(" +
                    "LastName nvarchar(40) not null, " +
                    "FirstName nvarchar(40), " +
                    "URL nvarchar(256))";
                cmd = new SqlCeCommand(sql, this.Connection);
                cmd.ExecuteNonQuery();
                created = true;
            }
            catch (SqlCeException sqlexception)
            {
                Console.WriteLine("SqlException");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure");
            }

            return created;
        }

        public bool PopulateTables()
        {
            bool populated = false;
            string sql = "insert into CoolPeople " +
            "(LastName, FirstName, URL) " +
            "values(@lastname, @firstname, @url)";
            SqlCeCommand cmd = null;

            try
            {
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.Connection.Open();
                }
                cmd = new SqlCeCommand(sql, this.Connection);
                cmd.Parameters.AddWithValue("@lastname", "Test");
                cmd.Parameters.AddWithValue("@firstname", "Tester");
                cmd.Parameters.AddWithValue("@url", "the_tester_web");
                cmd.ExecuteNonQuery();
                populated = true;
            }
            catch (SqlCeException sqlexception)
            {
                if (cmd != null)
                {
                    Console.WriteLine("SqlException writing row: " + cmd.CommandText);
                }
                else
                {
                    Console.WriteLine("SqlException - Unable to create cmd");
                }
                Console.WriteLine(sqlexception.Message);
                Console.WriteLine(sqlexception.StackTrace);
            }
            catch (Exception ex)
            {
                if (cmd != null)
                {
                    Console.WriteLine("Exception writing row: " + cmd.CommandText);
                }
                else
                {
                    Console.WriteLine("Exception - Unable to create cmd");
                }
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return populated;
        }

        public bool CloseConnection()
        {
            bool closed = false;

            try
            {
                if (this.Connection.State == ConnectionState.Open)
                {
                    this.Connection.Close();
                }
                closed = true;
            } 
            catch
            {

            }

            return closed;
        }

        static void Main(string[] args)
        {
            string fileName = "db.sdf";
            string password = "db";

            DatabaseInterface dbi = new DatabaseInterface(fileName, password);

            dbi.CreateDB();

            bool connectDb = dbi.ConnectDB();
            if (!connectDb)
                Console.WriteLine("Unable to connect DB");
            
            Console.ReadKey();
        }

        public void Populate(DataTable myDataTable, string tableName)
        {
            // If the datatable has no rows, we’re wasting
            // our time, get outta dodge.
            if (myDataTable.Rows.Count == 0)
            {
                return;
            }

            // Make sure database is open for business
            this.ConnectDB();
            
            // Use a string builder to hold our insert clause
            StringBuilder sql = new StringBuilder();
            sql.Append("insert into " +tableName + " (");
            
            // Two more, one for the list of field names,
            // the other for the list of parameters
            StringBuilder fields = new StringBuilder();
            StringBuilder parameters = new StringBuilder();
            
            // This cycles thru each column in the datatable,
            // and gets it’s name. It then uses the column name
            // for the list of fields, and the column name in
            // all lower case for the parameters
            foreach (DataColumn col in myDataTable.Columns)
            {
                fields.Append(col.ColumnName);
                parameters.Append("@" +col.ColumnName.ToLower());
                
                if (col.ColumnName != myDataTable.Columns[myDataTable.Columns.Count - 1].ColumnName)
                {
                    fields.Append(", ");
                    parameters.Append(", ");
                }
            }
            sql.Append(fields.ToString() + ") ");
            sql.Append("values(");
            sql.Append(parameters.ToString() + ") ");
            
            // We now have our Insert statement generated.
            // At this point we are ready to go through
            // each row and add it to our SSCE table.
            int rowCnt = 0;
            string totalRows = myDataTable.Rows.Count.ToString();
            
            foreach (DataRow row in myDataTable.Rows)
            {
                SqlCeCommand cmd = new SqlCeCommand(sql.ToString(), this.Connection);
                
                foreach (DataColumn col in myDataTable.Columns)
                {
                    cmd.Parameters.AddWithValue("@" +
                        col.ColumnName.ToLower(), row[col.ColumnName]);
                }
                // Optional: I created a delegate called message delegate,
                // and assign it to the class level variable Cachemessage
                // It’s a simple method that takes one string and displays
                // the results in a status bar. If you want to simplify
                // things, just remove this entire section (down to
                // the try).

                rowCnt++;

                if ((rowCnt % 100) == 0)
                    Console.WriteLine("Loading " + tableName + 
                        " Row " + rowCnt.ToString() + 
                        " of " + totalRows);

                try
                {
                    // Here’s where all the action is
                    // sports racers! This is what sends
                    // our insert statement to our local table.
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // You’ll probably want to be a bit more
                    // elegant here, but for an example it’ll do.
                    throw ex;
                }
            }
        }
    }
}
