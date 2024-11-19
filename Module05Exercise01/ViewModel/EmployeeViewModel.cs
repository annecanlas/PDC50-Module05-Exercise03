using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Module05Exercise01.Model;
using Module05Exercise01.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Module05Exercise01.ViewModel
{
    public class EmployeeViewModel: INotifyPropertyChanged
    {
        private readonly EmployeeService _employeeService;
        public ObservableCollection<Employee> EmployeeList { get; set; }
        public ObservableCollection<Employee> FilteredEmployeeList { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterEmployeeList();
            }
        }
        private void ClearFields()
        {
            NewEmployeeName = string.Empty;
            NewEmployeeAddress = string.Empty;
            NewEmployeeemail = string.Empty;
            NewEmployeeContactNo = string.Empty;
            SelectedEmployee = null;
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                if (_selectedEmployee != null)
                {
                    NewEmployeeName = _selectedEmployee.Name;
                    NewEmployeeAddress = _selectedEmployee.Address;
                    NewEmployeeemail = _selectedEmployee.email;
                    NewEmployeeContactNo = _selectedEmployee.ContactNo;
                    IsEmployeeSelected = true;
                    IsEmployeeSelectedAdd = false;
                }
                else
                {
                    IsEmployeeSelected = false;
                    IsEmployeeSelectedAdd = true;
                }
                OnPropertyChanged();
            }
        }
        private bool _isEmployeeSelected;
        public bool IsEmployeeSelected
        {
            get => _isEmployeeSelected;
            set
            {
                _isEmployeeSelected = value;
                OnPropertyChanged();
            }
        }
        private bool _isEmployeeSelectedAdd;
        public bool IsEmployeeSelectedAdd
        {
            get => _isEmployeeSelectedAdd;
            set
            {
                _isEmployeeSelectedAdd = value;
                OnPropertyChanged();
            }
        }
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // New Personal entry for name, gender, contant no
        public string _newEmployeeName;
        public string NewEmployeeName
        {
            get => _newEmployeeName;
            set
            {
                _newEmployeeName = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeAddress;
        public string NewEmployeeAddress
        {
            get => _newEmployeeAddress;
            set
            {
                _newEmployeeAddress = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeemail;
        public string NewEmployeeemail
        {
            get => _newEmployeeemail;
            set
            {
                _newEmployeeemail = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeContactNo;
        public string NewEmployeeContactNo
        {
            get => _newEmployeeContactNo;
            set
            {
                _newEmployeeContactNo = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDataCommand { get; set; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand UpdateEmployeeCommand { get; }
        public ICommand SelectedEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeViewModel() 
        {
            _employeeService = new EmployeeService();
            EmployeeList = new ObservableCollection<Employee>();
            
            FilteredEmployeeList = new ObservableCollection<Employee>();
            
            LoadDataCommand = new Command(async () => await LoadData());
            
            AddEmployeeCommand = new Command(async () => await AddEmployee());

            UpdateEmployeeCommand = new Command(async () => await UpdateEmployee());
            DeleteEmployeeCommand = new Command(
                async () => await DeleteEmployee(),
                () => IsEmployeeSelected
            );
            
            SelectedEmployeeCommand = new Command<Employee>(employee =>
            {
                SelectedEmployee = employee;
                ((Command)DeleteEmployeeCommand).ChangeCanExecute();
            });

            LoadData();
        }

        public async Task LoadData()
        {
            if (_isBusy) return;
            _isBusy = true;
            StatusMessage = "Loading employee data...";
            try
            {
                var employees = await _employeeService.GetAllEmployeeAsynch();

                foreach (var employee in employees)
                {
                    if (!EmployeeList.Any(e => e.EmployeeID == employee.EmployeeID))
                    {
                        EmployeeList.Add(employee);
                    }
                }

                FilteredEmployeeList = new ObservableCollection<Employee>(EmployeeList);
                StatusMessage = "Data loaded successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task AddEmployee()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(NewEmployeeName) || string.IsNullOrWhiteSpace(NewEmployeeAddress) || string.IsNullOrWhiteSpace(NewEmployeeemail) || string.IsNullOrWhiteSpace(NewEmployeeContactNo))
            {
                StatusMessage = "Please fill in all fields before adding";
                return;
            }
            IsBusy = true;
            StatusMessage = "Adding new employee record.....";

            try
            {
                var newEmployee = new Employee
                {
                    Name = NewEmployeeName,
                    Address = NewEmployeeAddress,
                    email = NewEmployeeemail,
                    ContactNo = NewEmployeeContactNo
                };
                var isSuccess = await _employeeService.AddEmployeeAsync(newEmployee);
                if (isSuccess)
                {
                    NewEmployeeName = string.Empty;
                    NewEmployeeAddress = string.Empty;
                    NewEmployeeemail = string.Empty;
                    NewEmployeeContactNo = string.Empty;
                    StatusMessage = "New employee added successfully";
                }
                else
                {
                    StatusMessage = "Failed to add the new employee";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed adding employee: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task UpdateEmployee()
        {
            if (IsBusy || SelectedEmployee == null)
            {
                StatusMessage = "Select an employee to update";
                return;
            }
            IsBusy = true;

            try
            {
                SelectedEmployee.Name = NewEmployeeName;
                SelectedEmployee.Address = NewEmployeeAddress;
                SelectedEmployee.email = NewEmployeeemail;
                SelectedEmployee.ContactNo = NewEmployeeContactNo;

                var success = await _employeeService.UpdateEmployeeAsync(SelectedEmployee);
                StatusMessage = success ? "Employee updated successfully!" : "Failed to update employee.";

                await LoadData();
                ClearFields();
                
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task DeleteEmployee()
        {
            Console.WriteLine("Delete command executed");

            if (SelectedEmployee == null)
            {
                Console.WriteLine("Delete failed: No person selected.");
                return;
            }

            var answer = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {SelectedEmployee.Name}?",
                "Yes", "No");

            if (!answer)
            {
                Console.WriteLine("Delete cancelled by user.");
                return;
            }

            IsBusy = true;
            StatusMessage = "Deleting employee...";

            try
            {
                var success = await _employeeService.DeleteEmployeeAsync(SelectedEmployee.EmployeeID);
                StatusMessage = success ? "Person Deleted Successfully" : "Failed to delete person";

                if (success)
                {
                    EmployeeList.Remove(SelectedEmployee);
                    SelectedEmployee = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting employee: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterEmployeeList()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredEmployeeList.Clear();
                foreach (var employee in EmployeeList)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
            else
            {
                var filtered = EmployeeList.Where(p =>
                                         (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                         (p.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                         (p.email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                         (p.ContactNo?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                                         .ToList();
                FilteredEmployeeList.Clear();
                foreach (var employee in filtered)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
            OnPropertyChanged(nameof(FilteredEmployeeList));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
