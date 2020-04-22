using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw5.DTOs;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{

    [Route("api/enrollments")]
    [Authorize(Roles = "employee")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {

        private IStudentDbService service;
     
        public EnrollmentsController(IStudentDbService service)
        {
            this.service = service;
        }

        [HttpPost]
        public IActionResult EntrollStudent(EnrollStudentRequest request)
        {
            var result = service.EnrollStudent(request);

            switch (result.ResultCode)
            {
                case ResultCodes.NieWpisanoWszystkichDanychStudenta:
                    return BadRequest("Nie wpisano poprawnie wszystkich danych studenta");
                    
                case ResultCodes.NieIstniejaStudia:
                    return BadRequest("Studia nie istnieją");       
                    
                case ResultCodes.StudentJestJuzZapisanyNaSemest:
                    return BadRequest("Student już jest zapisany na semestr 1!");
                   
                case ResultCodes.StudentJuzIstnieje:
                    return BadRequest("Student już istnieje");                    
            }

            return Created("", result.Response);


        }

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        { 


            var result = service.PromoteStudents(request);

            return Created("",result);
        }

    }
}
