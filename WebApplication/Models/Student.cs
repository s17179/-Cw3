using System;

namespace WebApplication.Models
{
    public class Student
    {
        public string IndexNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime birthDate { get; set; }
        public StudentEnrollment StudentEnrollment { get; set; }
    }
}