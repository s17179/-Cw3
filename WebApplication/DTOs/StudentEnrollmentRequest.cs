using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.DTOs
{
    public class StudentEnrollmentRequest
    {
        [Required]
        public string IndexNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public String Studies { get; set; }
    }
}