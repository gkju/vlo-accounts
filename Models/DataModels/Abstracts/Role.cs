using System;

namespace VLO_BOARDS.Models.DataModels.Abstracts
{
    public abstract class Role
    {
        public Role()
        {
            Id = Guid.NewGuid();
        }
        
        protected Role(Scope scope, Properties properties, string Name = "DefaultRoleName")
        {
            this.scope = scope;
            this.Name = Name;
            Id = Guid.NewGuid();
        }

        public virtual string Name { get; set; } = "DefaultRoleName";

        public Guid Id { get; set; }
        
        public Properties properties { get; set; }
        public Scope scope { get; set; }

        public virtual bool UserManageable { get; } = true;
        public static bool operator ==(Role a, Role b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Name == b.Name && a.Id == b.Id && a.scope == b.scope;
        }

        public static bool operator !=(Role a, Role b)
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
                Role otherRole = (Role) obj;

                return otherRole == this;
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
            return Tuple.Create(Name, Id, scope).GetHashCode();
        }
    }
}