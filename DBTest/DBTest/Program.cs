using System;
using System.Data.Common;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string provider = ConfigurationManager.AppSettings["provider"];
            string connectionString = ConfigurationManager.AppSettings["connectionString"];

            int numberOfProducts = 0;
            

            //Create connection to the Database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //Exception catches//
                if(connection == null)
                {
                    Console.WriteLine("Connection Error");
                    Console.ReadLine();
                    return;
                }

                connection.ConnectionString = connectionString;
                connection.Open();

                SqlCommand command = new SqlCommand();// factory.CreateCommand().ToString());

                if(command == null)
                {
                    Console.WriteLine("Command Error");
                    Console.ReadLine();
                    return;
                }
                //---------------------------//


                command.Connection = connection;

                // Retrieve number of Products as INT32
                command.CommandText =
                    "SELECT COUNT(*) From Products";
                Int32 i = (Int32)command.ExecuteScalar();

                // Turn ON Identity Insert
                command.CommandText =
                    "SET IDENTITY_INSERT Products ON";
                command.ExecuteNonQuery();

                // INSERT new Product into TABLE.
                command.CommandText =
                    "INSERT INTO Products ([ProdID], [Product],[Price],[Code]) VALUES (@ProdID, N'Bowl of Ketchup', CAST(20.5 AS Money), N'BOK')";
                command.Parameters.AddWithValue("@ProdID", System.Data.SqlDbType.Int).Value = i + 1;
                int rows = command.ExecuteNonQuery();

                //Nothing in this DB yet...
                ////Read from Customers Database
                //command.CommandText = "Select * From Customers";

                ////Read from the Database
                //using (DbDataReader dataReader = command.ExecuteReader())
                //{
                //    while (dataReader.Read())
                //    {
                //        Console.WriteLine($"{dataReader["CustID"]} " +
                //            $"{dataReader["Name"]} ");
                //    }
                //}

                // Read Entire Database
                ListAllProducts(command);

                // Display number of rows inserted.
                Console.WriteLine("Inserted {0} rows.\n", rows);

                // Retrieve number of DUPLICATE Products as INT32
                command.CommandText =
                    "SELECT COUNT(*) FROM Products WHERE ProdID NOT IN (SELECT MIN(ProdID) FROM Products GROUP BY Product)";
                Int32 duplicateCount = (Int32)command.ExecuteScalar();

                // Check to see if we have DUPLICATES
                if (duplicateCount > 0)
                {
                    // Display number of DUPLICATES.
                    Console.WriteLine("Found {0} duplicate rows!\n", duplicateCount);

                    // Delete DUPLICATES
                    Console.WriteLine("Deleting duplicates...");
                    command.CommandText =
                        "DELETE FROM Products WHERE ProdID NOT IN (SELECT MIN(ProdID) FROM Products GROUP BY Product)";
                    command.ExecuteScalar();

                    // Read Entire Database - return number of products in DB
                    numberOfProducts = ListAllProducts(command);

                    // Display number of DUPLICATES deleted.
                    Console.WriteLine("Deleted {0} duplicate rows!", duplicateCount);
                }
            }
            
            MessageBox.Show("Finished with the application! \nTotal of " + numberOfProducts + " Products.", "Finished Application", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static int ListAllProducts(SqlCommand command)
        {
            //Read from Products Database
            command.CommandText = "Select * From Products";
            int i = 0;
            using (DbDataReader dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    Console.WriteLine($"{dataReader["ProdID"]} " +
                        $"{dataReader["Product"]}" +
                        $"${dataReader["Price"]} ");
                    i++;
                }
            }
            return i;
        }
    }
}
