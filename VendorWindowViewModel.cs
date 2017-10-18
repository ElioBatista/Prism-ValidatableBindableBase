using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidatableBindableBaseDemo
{
    public class VendorWindowViewModel : BindableBase
    {
        private Vendor _Vendor = new Vendor();

        public VendorWindowViewModel()
        {
            SaveCommand = new DelegateCommand(OnSave);
            Vendor.ErrorsChanged += (s, e) => Errors = FlattenErrors();
            Vendor.PropertyChanged += (s, e) => PerformAsyncValidation();
        }

        public DelegateCommand SaveCommand { get; set; }

        public Vendor Vendor
        {
            get { return _Vendor; }
            set { SetProperty(ref _Vendor, value); }
        }

        private List<string> _Errors;

        public List<string> Errors
        {
            get { return _Errors; }
            set { SetProperty(ref _Errors, value); }
        }

        private void OnSave()
        {
            Vendor.ValidateProperties();
            Errors = FlattenErrors();
            if (!Vendor.HasErrors)
            {
                // Save it somewhere
            }
        }
        private List<string> FlattenErrors()
        {
            List<string> errors = new List<string>();
            Dictionary<string, List<string>> allErrors = Vendor.GetAllErrors();
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
            var errors = await proxy.ValidatePhone(Vendor.Phone);
            if (errors != null) Vendor.SetErrors(() => Vendor.Phone, errors);
        }

    }
}
