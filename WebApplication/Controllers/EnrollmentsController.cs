using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.DTOs;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsDbService _studentsDbService;

        public EnrollmentsController(IStudentsDbService studentsDbService)
        {
            _studentsDbService = studentsDbService;
        }

        [HttpPost]
        public IActionResult EnrollStudent(StudentEnrollmentRequest request)
        {
            // Końcówka powinna najpierw sprawdzić czy przekazane zostały wszystkie dane.
            // W przeciwnym razie zwracamy błąd 400
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var studentEnrollment = _studentsDbService.EnrollStudent(request);
                
                return Created("", studentEnrollment);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPost]
        [Route("promotions")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        {
            try
            {
                var studentEnrollment = _studentsDbService.PromoteStudents(request);

                return Created("", studentEnrollment);
            }
            catch (DataException exception)
            {
                return NotFound();
            }
        }
    }
}