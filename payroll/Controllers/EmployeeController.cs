using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using payroll.Data;
using payroll.Dto;
using payroll.Interface;
using payroll.Model;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace payroll.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeService;

        public EmployeeController(IEmployeeRepository employeeService)
        {
            _employeeService = employeeService; 
        }

        [HttpPost]
        [Route("CreateEmployee")]
        public async Task<IActionResult> CreateEmployeeAsync([FromBody] EmployeeDto dto)
        {
            var result = await _employeeService.CreateEmployeeAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployeesAsync(string? term)
        {
            var result = await _employeeService.GetAllEmployeesAsync();
            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateEmployee")]
        public async Task<IActionResult> UpdateEmployeeAsync(int id, EmployeeDto dto)
        {
            var result = await _employeeService.UpdateEmployeeAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteEmployee")]
        public async Task<IActionResult> DeleteEmployeeAsync(IEnumerable<int> id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            return Ok(result);

        }

        [HttpGet]
        [Route("GetTakeHomePay")]
        public async Task<IActionResult> GetTakeHomePayAsync(string employeeNo, DateTime startDate, DateTime endDate)
        {
            var result = await _employeeService.GetTakeHomePayAsync(employeeNo, startDate, endDate);
            return Ok(result);
        }
    }
}
