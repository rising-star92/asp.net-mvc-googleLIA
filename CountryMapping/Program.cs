using System;
using System.Data;
using NotVisualBasic.FileIO;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CountryMapping
{
    class Program
    {
        private static void GetDataTableFromCSVFile(string fileName)
        {
            DataTable dt = new DataTable();

            try
            {
                using (var csvReader = new StreamReader(fileName))
                using (var parser = new CsvTextFieldParser(csvReader))
                {
                    string[] colFields = parser.ReadFields();

                    foreach(string col in colFields)
                    {
                        DataColumn dataCol = new DataColumn(col.ToLower().Replace(" ", "_"));
                        dt.Columns.Add(dataCol);
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields;
                        try
                        {
                            fields = parser.ReadFields();
                            for (int i = 0; i < fields.Length; i++)
                            {
                                if (fields[i] == "")
                                {
                                    fields[i] = null;
                                }
                            }

                            dt.Rows.Add(fields);
                        }
                        catch (CsvMalformedLineException ex)
                        {
                            Console.Error.WriteLine($"Failed to parse line {ex.LineNumber}: {parser.ErrorLine}");
                        }
                    }

                    IConfigurationBuilder builder = new ConfigurationBuilder()
                        .SetBasePath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                        .AddJsonFile("appsettings.json");


                    var configuration = builder.Build();
                    string DBconnection = configuration.GetSection("ConnectionString").Value;

                    using (SqlConnection dbConnection = new SqlConnection(DBconnection))
                    {
                        dbConnection.Open();
                        using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                        {
                            s.DestinationTableName = "dbo.Countrylist";
                            foreach (var column in dt.Columns)
                            {
                                s.ColumnMappings.Add(column.ToString(), column.ToString());
                            }
                            s.WriteToServer(dt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }



        static void Main(string[] args)
        {
            GetDataTableFromCSVFile("countrylist.csv");
            Console.WriteLine("Hello World!");
        }
    }
}
