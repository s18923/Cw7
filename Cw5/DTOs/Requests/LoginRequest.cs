using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Requests
{
    public class LoginRequest
    {
        public string NazwaUzytkownika { get; set; }

        public string Haslo { get; set; }
    }
}
