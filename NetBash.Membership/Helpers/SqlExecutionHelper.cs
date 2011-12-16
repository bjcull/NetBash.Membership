using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace NetBash.Membership.Helpers
{
    public class SqlExecutionHelper
    {
        public void ExecuteBatchNonQuery(string sql, SqlCommand command)
        {
            sql += "\nGO";   // make sure last batch is executed.
            string sqlBatch = string.Empty;

            try
            {
                foreach (string line in sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (!string.IsNullOrEmpty(sqlBatch))
                        {
                            command.CommandText = sqlBatch;
                            command.ExecuteNonQuery();
                            sqlBatch = string.Empty;
                        }
                    }
                    else
                    {
                        sqlBatch += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                using (var message = new StringWriter())
                {
                    message.WriteLine("An error occured executing the following sql:");
                    message.WriteLine(sql);
                    message.WriteLine("The error was {0}", ex.Message);

                    throw new Exception(message.ToString(), ex);
                }
            }
        }
    }
}
