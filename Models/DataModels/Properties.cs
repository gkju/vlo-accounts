using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Helpers;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;
using VLO_BOARDS.Models.DataModels.Implementations.Roles;

namespace VLO_BOARDS.Models.DataModels
{
    public class Properties : IEnumerable<Property>
    {
        //never write directly, always use objectInstance[string] = 
        protected Dictionary<string, Property> _propertyDictionary
        {
            get;
            set;
        } = new Dictionary<string, Property>();

        public Properties(params Property[] parameters)
        {
            foreach (var parameter in parameters)
            {
                InsertOrMerge(parameter);
            }
        }
        
        public Properties(List<Role> roles)
        {
            PopulateProperties(roles);
        }

        public Properties(Properties properties)
        {
            PopulateProperties(properties);
        }
        
        public IEnumerator<Property> GetEnumerator()
        {
            foreach (var property in _propertyDictionary)
            {
                yield return property.Value;
            }
        }

        public Property this[string propertyName]
        {
            get { return _propertyDictionary[propertyName]; }
            set
            {
                value = (Property) value.Clone();
                //for safety reasons overrides the name
                propertyName = GetNameFromInstance(value);
                
                if (_propertyDictionary.ContainsKey(BannedProperty.Name) && _propertyDictionary.ContainsValue(new BannedProperty()))
                {
                    value.SetDefaultBannedValue();
                } else if (value.GetType() == typeof(BannedProperty))
                {
                    foreach (var property in _propertyDictionary)
                    {
                        property.Value.SetDefaultBannedValue();
                    }
                }
                _propertyDictionary[propertyName] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void InsertOrMerge(Property property)
        {
            property = (Property) property.Clone();
            
            string propertyName = GetNameFromInstance(property);

            if (ReferenceEquals(propertyName, null))
            {
                return;
            }

            if (_propertyDictionary.ContainsKey(propertyName))
            {
                this[propertyName] += property;
            }
            else
            {
                this[propertyName] = property;
            }
        }

        public void Merge(Property property)
        {
            property = (Property) property.Clone();

            string propertyName = GetNameFromInstance(property);

            if (ReferenceEquals(propertyName, null))
            {
                return;
            }

            if (_propertyDictionary.ContainsKey(propertyName))
            {
                this[propertyName] += property;
            }
        }

        public static string GetNameFromInstance(Property property)
        {
            Type type = property.GetType();
            FieldInfo nameField = type.GetField("Name", BindingFlags.Public | BindingFlags.Static);
            return (string) nameField?.GetValue(null);
        }

        public void PopulateProperties(List<Role> roles)
        {
            foreach(var role in roles)
            {
                foreach (Property property in role.properties)
                {
                    InsertOrMerge(property);
                }
            }
        }

        public void PopulateProperties(Properties properties)
        {
            foreach (Property property in properties)
            {
                InsertOrMerge(property);
            }
        }

        public List<string> GetKeys()
        {
            return new List<string>(_propertyDictionary.Keys);
        }
    }
}