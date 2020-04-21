using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using WebApplication.DAL;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        // [HttpGet]
        // public IActionResult GetStudents(string orderBy)
        // {
        //     ArrayList students = new ArrayList();
        //     
        //     using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
        //     using (var command = new SqlCommand())
        //     {
        //         command.Connection = connection;
        //         command.CommandText = "SELECT * FROM Student";
        //         
        //         connection.Open();
        //         var reader = command.ExecuteReader();
        //         while (reader.Read())
        //         {
        //             var student = new Student();
        //             student.IndexNumber = reader["IndexNumber"].ToString();
        //             student.FirstName = reader["FirstName"].ToString();
        //             student.LastName = reader["LastName"].ToString();
        //             student.birthDate = DateTime.Parse(reader["BirthDate"].ToString());
        //             students.Add(student);
        //         }
        //     }
        //     
        //     return Ok(students);
        // }
        
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Kowalski");
            } else if (id == 2)
            {
                return Ok("Malewski");
            }

            return NotFound("Nie znaleziono studenta");
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";

            return Ok(student);
        }

        [HttpPut]
        public IActionResult UpdateStudent(int id)
        {
            return Ok("Aktualizacja dokończona");
        }
        
        [HttpDelete]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }
        
        [HttpGet]
        public IActionResult GetStudentEnrollments(string studentId)
        {
            ArrayList students = new ArrayList();
            
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17179;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT Student.IndexNumber, Student.FirstName, Student.LastName, Student.BirthDate, Enrollment.IdEnrollment, Enrollment.Semester, Enrollment.StartDate, Studies.IdStudy, Studies.Name FROM Student JOIN Enrollment ON Student.IdEnrollment = Enrollment.IdEnrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE Student.IndexNumber = " + studentId;
                
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var study = new Study();
                    study.IdStudy = Int32.Parse(reader["IdStudy"].ToString());
                    study.Name = reader["Name"].ToString();
                    
                    var studentEnrollment = new StudentEnrollment();
                    studentEnrollment.IdEnrollment = Int32.Parse(reader["IdEnrollment"].ToString());
                    studentEnrollment.Semester = Int32.Parse(reader["Semester"].ToString());
                    studentEnrollment.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                    studentEnrollment.Study = study;
                    
                    var student = new Student();
                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.birthDate = DateTime.Parse(reader["BirthDate"].ToString());
                    student.StudentEnrollment = studentEnrollment;
                    
                    students.Add(student);
                }
            }
            
            return Ok(students);
        }
    }
}