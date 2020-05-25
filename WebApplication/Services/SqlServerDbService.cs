using System;
using System.Data;
using System.Data.SqlClient;
using WebApplication.DTOs;
using WebApplication.Models;

namespace WebApplication.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        public Student Login(LoginRequestDto request)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                
                command.CommandText = "SELECT FirstName, LastName FROM Student WHERE IndexNumber = @IndexNumber AND Password = @Password";
                command.Parameters.AddWithValue("IndexNumber", request.Login);
                command.Parameters.AddWithValue("Password", request.Haslo);
                
                var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    throw new Exception("User not found");
                }
                
                return new Student
                {
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString()
                };
            }
        }
        
        public StudentEnrollment EnrollStudent(StudentEnrollmentRequest request)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                // Wszystkie opisane czynności powinni się odbyć w ramach pojedynczej transakcja
                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                try
                {
                    var now = DateTime.Now;
                    var studiesName = request.Studies;
                    
                    // Następnie sprawdzamy czy istnieją studia w tabeli Studies zgodne z wartością przesłaną przez klienta.
                    // W przeciwnym wypadku zwracamy błąd 400
                    command.CommandText = "SELECT IdStudy FROM Studies WHERE Name = @name";
                    command.Parameters.AddWithValue("name", studiesName);

                    var reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        throw new ArgumentException("Studies " + studiesName + " not found");
                    }

                    int idStudy = (int)reader["IdStudy"];
                    reader.Close();

                    // Następnie odnajdujemy najnowszy wpis w tabeli Enrollments zgodny ze studiami studenta i wartością Semester=1
                    command.CommandText =
                        "SELECT TOP 1 IdEnrollment FROM Enrollment WHERE IdStudy = @idStudy AND Semester = 1 ORDER BY StartDate DESC";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("idStudy", idStudy);
                    reader = command.ExecuteReader();

                    // Jeśli tak wpis nie istnieje to dodajemy go do bazy danych (StartDate ustawiamy na aktualną datę)
                    int idEnrollment;
                    if (!reader.Read())
                    {
                        reader.Close();
                        idEnrollment = 1;
                        command.CommandText = "SELECT TOP 1 IdEnrollment FROM Enrollment ORDER BY IdEnrollment DESC";
                        reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            idEnrollment = (int) reader["IdEnrollment"] + 1;
                        }

                        reader.Close();

                        command.CommandText =
                            "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) VALUES (@newIdEnrollment, @semester, @idStudy, @startDate)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("newIdEnrollment", idEnrollment);
                        command.Parameters.AddWithValue("semester", 1);
                        command.Parameters.AddWithValue("idStudy", idStudy);
                        command.Parameters.AddWithValue("startDate", now);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        idEnrollment = (int) reader["IdEnrollment"];
                        reader.Close();
                    }

                    // Na końcu dodajemy wpis w tabeli Students
                    // Pamiętamy o tym, aby sprawdzić czy indeks podany przez studenta jest unikalny.
                    // W przeciwnym wypadku zgłaszamy błąd
                    command.CommandText = "SELECT 1 FROM Student WHERE IndexNumber = @indexNumber";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("indexNumber", request.IndexNumber);
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Close();
                        transaction.Rollback();
                        throw new ArgumentException($"Student with index {request.IndexNumber} already exists");
                    }
                    reader.Close();

                    command.CommandText =
                        "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("indexNumber", request.IndexNumber);
                    command.Parameters.AddWithValue("firstName", request.FirstName);
                    command.Parameters.AddWithValue("lastName", request.LastName);
                    command.Parameters.AddWithValue("birthDate", request.BirthDate);
                    command.Parameters.AddWithValue("idEnrollment", idEnrollment);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    
                    var studentEnrollment = new StudentEnrollment();
                    studentEnrollment.IdEnrollment = idEnrollment;
                    studentEnrollment.Semester = 1;
                    studentEnrollment.StartDate = now;
                    studentEnrollment.Study = new Study{IdStudy = idStudy, Name = studiesName};
                    
                    // Jeśli student został poprawnie zapisany na semestr to zwracamy kod 201.
                    // W ciele żądania zwracamy przypisany do studenta obiekt Enrollment reprezentujący semestr na który został wpisany
                    return studentEnrollment;
                }
                catch (SqlException exception)
                {
                    // Jeśli zaszedł jakikolwiek błąd chcemy wycofać (rollback) wszystkie zmiany
                    transaction.Rollback();
                    throw new ArgumentException(exception.Message);
                }
            }
        }

        public StudentEnrollment PromoteStudents(PromoteStudentRequest request)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                
                // Powinniśmy upewnić się, że w tabeli Enrollment istnieje wpis o podanej wartości Studies i Semester.
                // W przeciwnym razie zwracamy błąd 404 Not Found.
                command.CommandText = "SELECT 1 FROM Enrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE Semester = @semester AND Studies.Name = @studiesName";
                command.Parameters.AddWithValue("semester", request.Semester);
                command.Parameters.AddWithValue("studiesName", request.Studies);

                var reader = command.ExecuteReader();
                command.Parameters.Clear();
                if (!reader.HasRows)
                {
                    throw new DataException();
                }
                reader.Close();
                
                command.CommandText = "EXEC PromoteStudents @Studies, @Semester";
                command.Parameters.AddWithValue("Studies", request.Studies);
                command.Parameters.AddWithValue("Semester", request.Semester);
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                // Na końcu zwracamy kod 201 wraz z zawartością reprezentującą nowy obiekt Enrollment
                command.CommandText = "SELECT Enrollment.IdEnrollment, Enrollment.Semester, Enrollment.StartDate, Studies.IdStudy, Studies.Name FROM Enrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE Studies.Name = @studiesName AND Enrollment.Semester = @semester";
                command.Parameters.AddWithValue("semester", request.Semester + 1);
                command.Parameters.AddWithValue("studiesName", request.Studies);
                reader = command.ExecuteReader();
                command.Parameters.Clear();
                reader.Read();
                
                var studentEnrollment = new StudentEnrollment();
                studentEnrollment.IdEnrollment = (int) reader["IdEnrollment"];
                studentEnrollment.Semester = (int) reader["Semester"];
                studentEnrollment.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                studentEnrollment.Study = new Study{IdStudy = (int)reader["IdStudy"], Name = reader["Name"].ToString()};

                return studentEnrollment;
            }
        }
    }
}