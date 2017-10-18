using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidatableBindableBaseDemo
{
    public class MainWindowViewModel : BindableBase
    {
        private Customer _Customer = new Customer();

        public MainWindowViewModel()
        {
            SaveCommand = new DelegateCommand(OnSave);
            Customer.ErrorsChanged += (s, e) => Errors = FlattenErrors();
            Customer.PropertyChanged += (s, e) => PerformAsyncValidation();
        }

        public DelegateCommand SaveCommand { get; set; }

        public Customer Customer
        {
            get { return _Customer; }
            set { SetProperty(ref _Customer, value); }
        }

        private List<string> _Errors;

        public List<string> Errors
        {
            get { return _Errors; }
            set { SetProperty(ref _Errors, value); }
        }

        private void OnSave()
        {
            Customer.ValidateProperties();
            Errors = FlattenErrors();
            if (!Customer.HasErrors)
            {
                // Save it somewhere
            }
        }
        private List<string> FlattenErrors()
        {
            List<string> errors = new List<string>();
            Dictionary<string, List<string>> allErrors = Customer.GetAllErrors();
            foreach (string propertyName in allErrors.Keys)
            {
                foreach (var errorString in allErrors[propertyName])
                {
                    errors.Add(propertyName + ": " + errorString);
                }
            }
            return errors;
        }
        private async void PerformAsyncValidation()
        {
            SimulatedValidationServiceProxy proxy = new SimulatedValidationServiceProxy();
            var errors = await proxy.ValidatePhone(Customer.Phone);
            if (errors != null) Customer.SetErrors(() => Customer.Phone, errors);
        }

    }
}
