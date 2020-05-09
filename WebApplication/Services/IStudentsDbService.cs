using WebApplication.DTOs;
using WebApplication.Models;

namespace WebApplication.Services
{
    public interface IStudentsDbService
    {
        public StudentEnrollment EnrollStudent(StudentEnrollmentRequest request);
        public StudentEnrollment PromoteStudents(PromoteStudentRequest request);
    }
}