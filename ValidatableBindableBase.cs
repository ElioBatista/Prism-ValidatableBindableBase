using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Apollo.Core
{
    public class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
    {
        #region Bindable

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            if (!string.IsNullOrEmpty(propertyName))
            {
                ValidateProperty(propertyName);
            }

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected override bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            if (!string.IsNullOrEmpty(propertyName))
            {
                ValidateProperty(propertyName);
            }

            return true;
        }

        #endregion //Bindable

        #region Validation

        private ErrorsContainer<string> _errorsContainer;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return ErrorsContainer.GetErrors(propertyName);
        }

        public void SetErrors<TProperty>(Expression<Func<TProperty>> propertyExpression, IEnumerable<string> errors)
        {
            ErrorsContainer.SetErrors(propertyExpression, errors);
        }
        
        public virtual bool HasErrors => ErrorsContainer.HasErrors;

        protected ErrorsContainer<string> ErrorsContainer
        {
            get
            {
                if (_errorsContainer == null)
                    _errorsContainer = new ErrorsContainer<string>(propertyName => this.RaiseErrorsChanged(propertyName));

                return _errorsContainer;
            }
        }

        protected void RaiseErrorsChanged(string propertyName)
        {
            OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));            
        }

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
        }

        public bool ValidateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            var propertyInfo = this.GetType().GetRuntimeProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            var propertyErrors = new List<string>();
            bool isValid = TryValidateProperty(propertyInfo, propertyErrors);
            ErrorsContainer.SetErrors(propertyInfo.Name, propertyErrors);

            return isValid;
        }

        public virtual bool ValidateProperties()
        {
            var propertiesWithChangedErrors = new List<string>();

            // Get all the properties decorated with the ValidationAttribute attribute.
            Type thisType = this.GetType();

            var propertiesToValidate = thisType
                .GetRuntimeProperties()
                .Where(c => c.GetCustomAttributes(typeof(ValidationAttribute)).Any());

            foreach (PropertyInfo propertyInfo in propertiesToValidate)
            {
                var propertyErrors = new List<string>();
                TryValidateProperty(propertyInfo, propertyErrors);

                // If the errors have changed, save the property name to notify the update at the end of this method.
                ErrorsContainer.SetErrors(propertyInfo.Name, propertyErrors);
            }

            return ErrorsContainer.HasErrors;
        }

        private bool TryValidateProperty(PropertyInfo propertyInfo, List<string> propertyErrors)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyInfo.Name };
            var propertyValue = propertyInfo.GetValue(this);

            // Validate the property
            bool isValid = Validator.TryValidateProperty(propertyValue, context, results);

            if (results.Any())
            {
                propertyErrors.AddRange(results.Select(c => c.ErrorMessage));
            }

            return isValid;
        }

        #endregion
    }
}
