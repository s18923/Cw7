using Cw5.DTOs;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {

        public EnrollStudentResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResult result = new EnrollStudentResult();

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.IndexNumber) ||
                string.IsNullOrWhiteSpace(request.BirthDate) ||
                string.IsNullOrWhiteSpace(request.Studies) ||
                !DateTime.TryParse(request.BirthDate, out DateTime birthDate))
            {
                result.ResultCode = ResultCodes.NieWpisanoWszystkichDanychStudenta;

                return result;
            }

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18923;Integrated Security=True"))
            using (var com = new SqlCommand())
            {

                com.Connection = con;
                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;

                com.CommandText = "select IdStudy from Studies where Name = @name";
                com.Parameters.AddWithValue("name", request.Studies);

                var dr = com.ExecuteReader();

                if (!dr.Read())
                {

                    dr.Close();
                    tran.Rollback();
                    result.ResultCode = ResultCodes.NieIstniejaStudia;
                    return result;

                }

                int idStudy = (int)dr["IdStudy"];

                dr.Close();
                com.Parameters.Clear();

                com.CommandText = "select e.StartDate, e.IdEnrollment from Enrollment e join Student s on e.IdEnrollment = s.IdEnrollment " +
                    "where e.Semester = 1 and s.IndexNumber = @IndexNumber " +
                    "order by StartDate desc";

                com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);

                dr = com.ExecuteReader();
                if (dr.Read())
                {

                    dr.Close();
                    tran.Rollback();
                    result.ResultCode = ResultCodes.StudentJestJuzZapisanyNaSemest;
                    return result;
                }
                dr.Close();
                com.Parameters.Clear();

                com.CommandText = "select max(IdEnrollment) from Enrollment";
                int maxId = (int)com.ExecuteScalar() + 1;
                DateTime startDate = DateTime.Now;

                com.CommandText = "Insert into Enrollment (IdEnrollment, Semester, IdStudy, StartDate) values (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                com.Parameters.AddWithValue("IdEnrollment", maxId);
                com.Parameters.AddWithValue("Semester", 1);
                com.Parameters.AddWithValue("IdStudy", idStudy);
                com.Parameters.AddWithValue("StartDate", startDate);

                com.ExecuteNonQuery();
                com.Parameters.Clear();

                //----------------------//

                com.CommandText = "select FirstName from Student where IndexNumber = @IndexNumber";
                com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);

                dr = com.ExecuteReader();
                if (dr.Read())
                {

                    dr.Close();
                    tran.Rollback();
                    result.ResultCode = ResultCodes.StudentJuzIstnieje;
                    return result;
                }
                dr.Close();
                com.Parameters.Clear();

                com.CommandText = "Insert into Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) values (@Index, @Fname, @LName, @Date, @IdEnroll)";

                com.Parameters.AddWithValue("Index", request.IndexNumber);
                com.Parameters.AddWithValue("Fname", request.FirstName);
                com.Parameters.AddWithValue("Lname", request.LastName);
                com.Parameters.AddWithValue("Date", request.BirthDate);
                com.Parameters.AddWithValue("IdEnroll", maxId);

                com.ExecuteNonQuery();

                tran.Commit();

                var response = new EnrollStudentResponse
                {
                    IdEnrollment = maxId,
                    IdStudy = idStudy,
                    Semester = 1,
                    StartDate = startDate
                };

                result.ResultCode = ResultCodes.StudentDodany;
                result.Response = response;
                return result;
            }
        }

        public EnrollStudentResponse PromoteStudents(PromoteStudentRequest request)
        {
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18923;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.CommandType = CommandType.StoredProcedure;

                com.Connection = con;
                con.Open();

                com.CommandText = "PromoteStudents";
                com.Parameters.AddWithValue("@Semester", SqlDbType.Int).Value = request.Semester;
                com.Parameters.AddWithValue("@Name", SqlDbType.NVarChar).Value = request.Studies;

                com.Parameters.Add("@IdEnrollment", SqlDbType.Int).Direction = ParameterDirection.Output;
                com.Parameters.Add("@IdStudies", SqlDbType.Int).Direction = ParameterDirection.Output;
                com.Parameters.Add("@StartDate", SqlDbType.DateTime).Direction = ParameterDirection.Output;
                //com.Parameters.Add()

                com.ExecuteNonQuery();

                var response = new EnrollStudentResponse
                {
                    IdEnrollment = (int)com.Parameters["@IdEnrollment"].Value,
                    IdStudy = (int)com.Parameters["@IdStudies"].Value,
                    Semester = request.Semester + 1,
                    StartDate = (DateTime)com.Parameters["@StartDate"].Value
                };

                return response;
            }
        }


        
        public Boolean IsThereStudent(string index)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18923;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                try
                {
                    command.CommandText = "Select 1 from student where IndexNumber = @index";
                    command.Parameters.AddWithValue("index", index);
                    var reader = command.ExecuteReader();
                    return reader.Read();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }

        public bool CheckCredential(string uzytkownik, string haslo)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18923;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                try
                {
                    command.CommandText = "Select 1 from student where IndexNumber = @uzytkownik and haslo = @haslo; ";
                    command.Parameters.AddWithValue("uzytkownik", uzytkownik);
                    command.Parameters.AddWithValue("haslo", haslo);
                    return command.ExecuteReader().Read();

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }
    }
}
