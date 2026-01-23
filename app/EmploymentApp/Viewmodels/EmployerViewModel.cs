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
        private Color eventColor = Color.FromArgb("#9dfca8");

        [ObservableProperty]
        private Color vacancyColor = Color.FromArgb("#e0e0e0");

        public EmployerViewModel()
        {
            UpdateColors();  // Инициализация цветов
        }

        partial void OnIsVacancySelectedChanged(bool value)
        {
            UpdateColors();
        }

        private void UpdateColors()
        {
            EventColor = IsVacancySelected ? Color.FromArgb("#e0e0e0") : Color.FromArgb("#9dfca8");
            VacancyColor = IsVacancySelected ? Color.FromArgb("#9dfca8") : Color.FromArgb("#e0e0e0");
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
