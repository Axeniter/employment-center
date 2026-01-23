using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploymentApp.Models
{
    public class Vacancy
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; } = new();

        public int SalaryFrom { get; set; }

        public int SalaryTo { get; set; }

        public string SalaryCurrency { get; set; } = "RUB";

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public bool IsActive { get; set; } = true;

        public int EmployerId { get; set; }

        // Вычисляемое свойство для отображения диапазона зарплаты
        public string SalaryRange =>
            $"{SalaryFrom:N0} — {SalaryTo:N0} {SalaryCurrency}";

        // Вычисляемое свойство для места работы
        public string WorkLocation =>
            IsRemote ? "Удаленно" : Location;

        public Vacancy(int id, string title, string description, 
            int salaryFrom, int salaryTo, 
            string location, 
            bool isRemote,
            int employerId, 
            List<string> tags = null, 
            string salaryCurrency = "RUB", 
            bool isActive = true)
        {
            Id = id;
            Title = title;
            Description = description;
            Tags = tags ?? new List<string>();
            SalaryFrom = salaryFrom;
            SalaryTo = salaryTo;
            SalaryCurrency = salaryCurrency;
            Location = location;
            IsRemote = isRemote;
            IsActive = isActive;
            EmployerId = employerId;
        }
    }
}
