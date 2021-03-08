using System;
using System.Collections.Generic;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;

namespace VLO_BOARDS.Models.DataModels.RoleProperties
{
    public class GenericRoleProperties : Properties
    {
        private void Initialize()
        {
            _propertyDictionary = new Dictionary<string, Property>()
            {
                {AuthorityProperty.Name, new AuthorityProperty(0)},
                {MayManageRolesProperty.Name, new MayManageRolesProperty(false)}
            };  
        } 

        public GenericRoleProperties(Properties properties, int userAuthority = 0)
        {
            Initialize();
            
            List<string> keys = new List<string>(_propertyDictionary.Keys);

            foreach (var key in keys)
            {
                Merge(properties[key]);
            }

            this[AuthorityProperty.Name] = new AuthorityProperty(Math.Min(userAuthority, ((AuthorityProperty) this[AuthorityProperty.Name]).GetValue()));
        }
        
        public GenericRoleProperties(int userAuthority = 0, params Property[] parameters)
        {
            Initialize();

            foreach (var parameter in parameters)
            {
                Merge(parameter);
            }
            
            this[AuthorityProperty.Name] = new AuthorityProperty(Math.Min(userAuthority, ((AuthorityProperty) this[AuthorityProperty.Name]).GetValue()));
        }
    }
}