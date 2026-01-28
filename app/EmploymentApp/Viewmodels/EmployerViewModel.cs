using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EmploymentApp.Models;

namespace EmploymentApp.Viewmodels
{
    public partial class EmployerViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isVacancySelected;

        [ObservableProperty]
        private ObservableCollection<Event> eventCollection;

        [ObservableProperty]
        private ObservableCollection<Vacancy> vacancyCollection;

        [ObservableProperty]
        private ObservableCollection<Event> displayedEvents;

        [ObservableProperty]
        private ObservableCollection<Vacancy> displayedVacancies;



        [ObservableProperty]
        private Color eventColor = Color.FromArgb("#9dfca8");

        [ObservableProperty]
        private Color vacancyColor = Color.FromArgb("#e0e0e0");

        public EmployerViewModel()
        {
            UpdateColors();  // Инициализация цветов

            // Инициализация коллекции событий
            EventCollection = new ObservableCollection<Event>
            {
            new Event(1, "IT Конференция 2026", "Крупная конференция для разработчиков",
                "Владивосток", false, new DateTime(2026, 2, 15, 10, 0, 0), 1, true),

            new Event(2, "WebDeveloper Meetup", "Встреча веб-разработчиков",
                "Онлайн", true, new DateTime(2026, 2, 20, 18, 0, 0), 1, true),

            new Event(3, "C# Workshop", "Практический воркшоп по C#",
                "Санкт-Петербург", false, new DateTime(2026, 3, 10, 14, 0, 0), 1, true)
            };

            // Инициализация коллекции вакансий
            VacancyCollection = new ObservableCollection<Vacancy>
            {
            new Vacancy(1, "Senior C# Developer", "Требуется опыт работы с ASP.NET Core и SignalR",
                150000, 250000, "Владивосток", true,
                1, new List<string> { "C#", "ASP.NET Core", "SQL" }, "RUB", true),

            new Vacancy(2, "Junior React Developer", "Начинающий разработчик для веб-приложений",
                80000, 120000, "Москва", false,
                1, new List<string> { "React", "JavaScript", "CSS" }, "RUB", true),

            new Vacancy(3, "Python Backend Developer", "Разработчик на Django и DRF",
                120000, 180000, "Санкт-Петербург", true,
                1, new List<string> { "Python", "Django", "PostgreSQL" }, "RUB", true)


            };

            // Инициализация отображаемых коллекций
            DisplayedEvents = new ObservableCollection<Event>(EventCollection);
            DisplayedVacancies = new ObservableCollection<Vacancy>();
        }

        partial void OnIsVacancySelectedChanged(bool value)
        {
            UpdateColors();
            UpdateDisplayedCollections();
        }

        private void UpdateColors()
        {
            EventColor = IsVacancySelected ? Color.FromArgb("#e0e0e0") : Color.FromArgb("#9dfca8");
            VacancyColor = IsVacancySelected ? Color.FromArgb("#9dfca8") : Color.FromArgb("#e0e0e0");
        }

        private void UpdateDisplayedCollections()
        {
            if (IsVacancySelected)
            {
                DisplayedEvents.Clear();
                DisplayedVacancies = new ObservableCollection<Vacancy>(VacancyCollection);
            }
            else
            {
                DisplayedVacancies.Clear();
                DisplayedEvents = new ObservableCollection<Event>(EventCollection);
            }
        }

        [RelayCommand]
        private void EventTapped()
        {
            IsVacancySelected = false;
        }

        [RelayCommand]
        private void VacancyTapped()
        {
            IsVacancySelected = true;
        }
    }
}
