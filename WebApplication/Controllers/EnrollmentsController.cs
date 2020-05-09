using System;
using System.Data.SqlClient;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.DTOs;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        [HttpPost]
        public IActionResult EnrollStudent(StudentEnrollmentRequest studentEnrollmentRequest)
        {
            // Końcówka powinna najpierw sprawdzić czy przekazane zostały wszystkie dane.
            // W przeciwnym razie zwracamy błąd 400
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            String studiesName = studentEnrollmentRequest.Studies;
            
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
                    
                    // Następnie sprawdzamy czy istnieją studia w tabeli Studies zgodne z wartością przesłaną przez klienta.
                    // W przeciwnym wypadku zwracamy błąd 400
                    command.CommandText = "SELECT IdStudy FROM Studies WHERE Name = @name";
                    command.Parameters.AddWithValue("name", studiesName);

                    var reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return BadRequest("Studies " + studiesName + " not found");
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
                    command.Parameters.AddWithValue("indexNumber", studentEnrollmentRequest.IndexNumber);
                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Close();
                        transaction.Rollback();
                        return BadRequest($"Student with index {studentEnrollmentRequest.IndexNumber} already exists");
                    }
                    reader.Close();

                    command.CommandText =
                        "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("indexNumber", studentEnrollmentRequest.IndexNumber);
                    command.Parameters.AddWithValue("firstName", studentEnrollmentRequest.FirstName);
                    command.Parameters.AddWithValue("lastName", studentEnrollmentRequest.LastName);
                    command.Parameters.AddWithValue("birthDate", studentEnrollmentRequest.BirthDate);
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
                    return Created("", studentEnrollment);
                }
                catch (SqlException exception)
                {
                    // Jeśli zaszedł jakikolwiek błąd chcemy wycofać (rollback) wszystkie zmiany
                    transaction.Rollback();
                    return BadRequest(exception.Message);
                }
            }
        }
    }
}