using Microsoft.AspNetCore.Mvc;
using payroll.Dto;
using payroll.Model;
using payroll.Shared;

namespace payroll.Interface
{
    public interface IEmployeeRepository
    {
        Task<Result> CreateEmployeeAsync(EmployeeDto employeeDto);
        Task<Result> GetAllEmployeesAsync();
        Task<Result> UpdateEmployeeAsync(int id, EmployeeDto dto);
        Task<Result> DeleteEmployeeAsync(IEnumerable<int> id);
        Task<Result> GetTakeHomePayAsync(string employeeNumber, DateTime startDate, DateTime endDate);
    }
}
