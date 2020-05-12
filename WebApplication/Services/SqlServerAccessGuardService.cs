using System.Data.SqlClient;

namespace WebApplication.Services
{
    public class SqlServerAccessGuardService : IAccessGuardService
    {
        public bool CanAccess(string index)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                
                command.CommandText = "SELECT 1 FROM Student WHERE IndexNumber = @IndexNumber";
                command.Parameters.AddWithValue("IndexNumber", index);

                var reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return false;
                } 
            }

            return true;
        }
    }
}