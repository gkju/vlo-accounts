using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VLO_BOARDS.Models.DataModels.Abstracts
{
    public abstract class Scope : IEquatable<Scope>
    {

        public virtual String Name { get; set; } = "DefaultScopeName";
        
        public virtual String SubName { get; set; } = "";

        public List<Scope> ParentScopes { get; set; } = new List<Scope>();

        public List<Scope> GetAllParentScopes()
        {
            List<Scope> scopes = new List<Scope>();

            foreach (var scope in ParentScopes)
            {
                scopes.Add(scope);
                scopes = scopes.Concat(scope.GetAllParentScopes()).ToList();
            }

            return scopes;
        }

        public List<Scope> GetAllParentScopesIncludingSelf()
        {
            List<Scope> scopes = GetAllParentScopes();
            scopes.Add(this);
            return scopes;
        }

        public static bool operator ==(Scope a, Scope b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Name == b.Name && a.SubName == b.SubName;
        }

        public static bool operator !=(Scope a, Scope b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            Type t = obj.GetType();

            try
            {
                Scope otherScope = (Scope) obj;

                return otherScope == this;
            }
            catch
            {
                return false;
            }
            
        }

        public bool Equals(Scope obj)
        {
            return Equals((object) obj);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Name, SubName).GetHashCode();
        }
        
    }
}