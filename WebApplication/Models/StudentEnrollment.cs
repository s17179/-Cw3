using System;

namespace WebApplication.Models
{
    public class StudentEnrollment
    {
        public int IdEnrollment { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { set; get; }
        public Study Study { get; set; }
    }
}