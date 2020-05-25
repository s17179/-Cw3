using WebApplication.DTOs;
using WebApplication.Models;

namespace WebApplication.Services
{
    public interface IStudentsDbService
    {
        public StudentEnrollment EnrollStudent(StudentEnrollmentRequest request);
        public StudentEnrollment PromoteStudents(PromoteStudentRequest request);
        public Student Login(LoginRequestDto request);
        public void SaveRefreshToken(string refreshToken, string indexNumber);

        public Student LoginByRefreshToken(string refreshToken);
    }
}