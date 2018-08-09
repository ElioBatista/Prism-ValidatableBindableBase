using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace Apollo.Infrastructure.Presentation
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        
        public EnumBindingSourceExtension() { }
        public EnumBindingSourceExtension(Type enumType)
        {
            this.EnumType = enumType;            
        }
        public EnumBindingSourceExtension(Type enumType,bool orderByName)
        {
            this.EnumType = enumType;
            this.OrderByName = orderByName;
        }

        public bool OrderByName { get; set; } = false;
        public Type EnumType
        {
            get { return this._enumType; }
            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }
                    this._enumType = value;
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;            
            List<Enum> enumValues = new List<Enum>(Enum.GetValues(actualEnumType).Cast<Enum>());
            enumValues.RemoveAll( (item) => {
                FieldInfo fi = item.GetType().GetField(item.ToString());
                if (fi != null)
                {
                    var attributes = (BindableAttribute[])fi.GetCustomAttributes(typeof(BindableAttribute), false);
                    return ((attributes.Length > 0) && (!attributes[0].Bindable));
                }
                return false;
            });

            if (actualEnumType == this._enumType)
                return OrderByName ? enumValues.OrderBy(x=> x.ToString()).ToList() : enumValues;
            
            Enum[] tempArray = (Enum[])Array.CreateInstance(actualEnumType, enumValues.Count + 1).Cast<Enum>();
            enumValues.CopyTo(tempArray, 1);
            return OrderByName ? tempArray.OrderBy(x => x.ToString()).ToList() : tempArray.ToList();
        }
    }
}