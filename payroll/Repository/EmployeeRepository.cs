using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using payroll.Data;
using payroll.Dto;
using payroll.Interface;
using payroll.Model;
using payroll.Shared;
using System.Data;

namespace payroll.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public EmployeeRepository(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }
        private string GenerateEmployeeNumber(string lastName, DateOnly dob)
        {

            string prefix = new string(lastName
                .Where(char.IsLetter)
                .Take(3)
                .ToArray())
                .ToUpper();

            string random = new Random().Next(0, 1000).ToString("D5");

            string dobFormatted = dob
                .ToDateTime(TimeOnly.MinValue)
                .ToString("ddMMMyyyy")
                .ToUpper();

            return $"{prefix}-{random}-{dobFormatted}";
        }
        private string FormatEmployeeName(string last, string first, string middle)
        {
            var middleInitial = string.IsNullOrEmpty(middle) ? "" : $"{middle[0]}.";
            return $"{last.ToUpper()}, {first.ToUpper()} {middleInitial.ToUpper()}";
        }
        public async Task<Result> CreateEmployeeAsync(EmployeeDto dto)
        {
            var result = new Result();
            try
            {
                var employeeNumber = GenerateEmployeeNumber(dto.lastName, dto.dateOfBirth);
                var employeeName = FormatEmployeeName(dto.lastName, dto.firstName, dto.middleName);

                bool isUseSp = _configuration.GetValue<bool>("Features:UseStoredProcedure");

                if (isUseSp)
                {
                    try
                    {
                        string connString = _configuration.GetConnectionString("DefaultConnection");
                        using var conn = new SqlConnection(connString);
                        await conn.OpenAsync();

                        var newEmployee = await conn.ExecuteAsync(
                           "sp_create_employee",
                           new
                           {
                               EmployeeNumber = employeeNumber,
                               EmployeeName = employeeName,
                               DateOfBirth = dto.dateOfBirth.ToDateTime(TimeOnly.MinValue),
                               DailyRate = dto.dailyRate,
                               WorkingDays = dto.workingDays
                           },
                           commandType: CommandType.StoredProcedure);

                        return result.Success($"Employee successfully added using Stored Procedure.");
                    }
                    catch (SqlException sqlEx)
                    {
                        if (!sqlEx.Message.Contains("Could not find stored procedure"))
                            throw; 
                    }              
                }

                var employee = new Employee
                {
                    EmployeeNumber = employeeNumber,
                    EmployeeName = employeeName,
                    DateOfBirth = dto.dateOfBirth,
                    DailyRate = dto.dailyRate,
                    WorkingDays = dto.workingDays,
                };

                _appDbContext.Employees.Add(employee);
                await _appDbContext.SaveChangesAsync();

                return result.Success("Employee Succesfully added.");
            }
            catch(Exception ex)
            {
                return result.Exception(ex.Message);
            }   
        }
        public async Task<Result> GetAllEmployeesAsync()
        {
            var result = new Result();
            try
            {
                bool isUseSp = _configuration.GetValue<bool>("Features:UseStoredProcedure");

                if (isUseSp)
                {
                    try
                    {
                        string connString = _configuration.GetConnectionString("DefaultConnection");
                        using var conn = new SqlConnection(connString);
                        await conn.OpenAsync();

                        var employees = (await conn.QueryAsync<dynamic>("sp_get_all_employees", commandType: CommandType.StoredProcedure))
                             .Select(emp => new Employee
                             {
                                 Id = emp.Id,
                                 EmployeeNumber = emp.EmployeeNumber,
                                 EmployeeName = emp.EmployeeName,
                                 DateOfBirth = DateOnly.FromDateTime(emp.DateOfBirth),
                                 DailyRate = emp.DailyRate,
                                 WorkingDays = emp.WorkingDays
                             });

                        return result.Success("Success using Stored Procedure.", employees);
                    }
                    catch (SqlException sqlEx)
                    {
                        if (!sqlEx.Message.Contains("Could not find stored procedure"))
                            throw;
                    }
                }

                var employeeList =  _appDbContext.Employees.ToList();

                if (!employeeList.Any())
                {
                    return result.Exception("No employee found.");
                }

                return result.Success("Success",employeeList);
            }
            catch (Exception ex)
            {
                return result.Exception(ex.Message);
            }
        }
        public async Task<Result> UpdateEmployeeAsync(int id, EmployeeDto dto)
        {
            var result = new Result();
            try
            {
                var employee=  await _appDbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);

                if (employee == null)
                    return result.Exception("Employee does not exist.");

                var newEmployeeNo = "";

                if(dto.dateOfBirth != employee.DateOfBirth)
                {
                    newEmployeeNo = GenerateEmployeeNumber(dto.lastName, dto.dateOfBirth);
                }

                bool isUseSp = _configuration.GetValue<bool>("Features:UseStoredProcedure");

                if (isUseSp)
                {
                    try
                    {
                        string connString = _configuration.GetConnectionString("DefaultConnection");
                        using var conn = new SqlConnection(connString);
                        await conn.OpenAsync();

                        var updateEmployee = await conn.ExecuteAsync(
                            "sp_update_employee",
                            new
                            {
                                Id = id,
                                EmployeeNumber = !newEmployeeNo.IsNullOrEmpty() ? newEmployeeNo : employee.EmployeeNumber,
                                EmployeeName = FormatEmployeeName(dto.lastName, dto.firstName, dto.middleName),
                                DateOfBirth = dto.dateOfBirth.ToDateTime(TimeOnly.MinValue),
                                DailyRate = dto.dailyRate,
                                WorkingDays = dto.workingDays
                            },
                            commandType: CommandType.StoredProcedure
                        );

                        return result.Success("Successfully Updated using Stored Procedure.");
                    }
                    catch (SqlException sqlEx)
                    {
                        if (!sqlEx.Message.Contains("Could not find stored procedure"))
                            throw;
                    }
                }

                employee.EmployeeNumber = !newEmployeeNo.IsNullOrEmpty() ? newEmployeeNo : employee.EmployeeNumber;
                employee.EmployeeName = FormatEmployeeName(dto.lastName, dto.firstName, dto.middleName);
                employee.DateOfBirth = dto.dateOfBirth;
                employee.DailyRate = dto.dailyRate;
                employee.WorkingDays = dto.workingDays;

                _appDbContext.SaveChanges();

                return result.Success("Successfully Updated.");
            }
            catch (Exception ex)
            {
                return result.Exception(ex.Message);
            }
        }

        public async Task<Result> DeleteEmployeeAsync(IEnumerable<int> id)
        {
            var result = new Result();
            try
            {
                bool isUseSp = _configuration.GetValue<bool>("Features:UseStoredProcedure");

                if (isUseSp)
                {
                    try
                    {
                        string connString = _configuration.GetConnectionString("DefaultConnection");
                        using var conn = new SqlConnection(connString);
                        await conn.OpenAsync();

                        foreach (var employeeId in id)
                        {
                            await conn.ExecuteAsync("sp_DeleteEmployee", new { EmployeeId = employeeId },commandType: CommandType.StoredProcedure);
                        }

                        return result.Success("Employee(s) successfully deleted using Stored Procedure.");
                    }
                    catch (SqlException sqlEx)
                    {
                        if (!sqlEx.Message.Contains("Could not find stored procedure"))
                            throw;
                    }               
                }

                foreach (var employeeId in id)
                {
                    var employee = await _appDbContext.Employees.FirstOrDefaultAsync(x => x.Id == employeeId);
                    if (employee == null)
                        return result.Exception("Emplooyee does not found.");

                    _appDbContext.Remove(employee);
                }
                
                _appDbContext.SaveChanges();

                return result.Success("Employee(s) Successfully deleted.");
            }
            catch (Exception ex)
            {
                return result.Exception(ex.Message);
            }
        }

        public async Task<Result> GetTakeHomePayAsync(string employeeNumber, DateTime startDate, DateTime endDate)
        {
            var result = new Result();
            try
            {
                var employee = await _appDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeNumber == employeeNumber);

                if(employee == null)
                {
                    return result.Exception("Employee does not found.");
                }

                var takehomePay = ComputeTakeHomePay(employee, startDate, endDate);

                return result.Success($"Total amount of Take Home Pay is PHP {takehomePay} ");
            }
            catch (Exception ex)
            {
                return result.Exception(ex.Message);
            }
        }

        public decimal ComputeTakeHomePay(Employee employee, DateTime startDate, DateTime endDate)
        {
            decimal totalPay = 0;

            var workingDays = employee.WorkingDays.ToUpper();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                bool isWorkday = false;

                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        isWorkday = workingDays.Contains("M");
                        break;
                    case DayOfWeek.Tuesday:
                        isWorkday = workingDays.Contains("T");
                        break;
                    case DayOfWeek.Wednesday:
                        isWorkday = workingDays.Contains("W");
                        break;
                    case DayOfWeek.Thursday:
                        isWorkday = workingDays.Contains("TH");
                        break;
                    case DayOfWeek.Friday:
                        isWorkday = workingDays.Contains("F");
                        break;
                    case DayOfWeek.Saturday:
                        isWorkday = workingDays.Contains("S");
                        break;
                }

                if (isWorkday)
                {
                    totalPay += employee.DailyRate * 2;
                }

                if (date.Month == employee.DateOfBirth.Month &&
                    date.Day == employee.DateOfBirth.Day)
                {
                    totalPay += employee.DailyRate; 
                }
            }
            return totalPay;
        }
    }
}
