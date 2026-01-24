using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploymentApp.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public DateTime Date { get; set; }

        public bool IsActive { get; set; }

        public int EmployerId { get; set; }

        public string FormattedDate =>
            Date.ToString("dd MMMM yyyy, HH:mm", new System.Globalization.CultureInfo("ru-RU"));

        public string EventLocation =>
            IsRemote ? "Онлайн" : Location;

        public Event(int id, string title, string description, string location, bool isRemote,
            DateTime date, int employerId, bool isActive = true )
        {
            Id = id;
            Title = title;
            Description = description;
            Location = location;
            IsRemote = isRemote;
            Date = date;
            IsActive = isActive;
            EmployerId = employerId;
        }
    }
}

