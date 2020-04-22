﻿using Cw5.DTOs;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IStudentDbService
    {
        public EnrollStudentResult EnrollStudent(EnrollStudentRequest request);

        public EnrollStudentResponse PromoteStudents(PromoteStudentRequest request);

        public Boolean IsThereStudent(string index);
        public Boolean CheckCredential(string user, string password);
    }
}
