using System;
using VLO_BOARDS.Models.DataModels.Abstracts;

namespace VLO_BOARDS.Models.DataModels.Helpers
{
    
    public class SimpleIntegerProperty : Property, IEquatable<SimpleIntegerProperty>
    {
        protected int Data;

        public SimpleIntegerProperty()
        {
            Data = 0;
        }
        public SimpleIntegerProperty(Property property )
        {
            try
            {
                Data = ((SimpleIntegerProperty) property).Data;
            }
            catch
            {
                Data = 0;
            }
            
        }

        public SimpleIntegerProperty(SimpleIntegerProperty property)
        {
            Data = property.Data;
        }
        
        public override void SetDefaultBannedValue()
        {
            Data = 0;
        }

        public override Property MakeACombination(Property _parentProperty)
        {
            SimpleIntegerProperty parentProperty = new SimpleIntegerProperty((SimpleIntegerProperty) _parentProperty);
            //DO NOT CHANGE TO A COPY CONSTRUCTOR TO AVOID CHANGING TYPE
            var copyOfSelf = (SimpleIntegerProperty) this.Clone();
            
            if (parentProperty.Data > copyOfSelf.Data)
            {
                copyOfSelf.Data = parentProperty.Data;
            }

            return copyOfSelf;
        }

        public static implicit operator int(SimpleIntegerProperty property)
        {
            return property.Data;
        }

        public static bool operator ==(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            if (CheckReferenceEquality(a, b) == ReferenceEquality.SameObjects)
            {
                return true;
            } else if (CheckReferenceEquality(a, b) == ReferenceEquality.OneIsNull)
            {
                return false;
            }
            
            return a.Data == b.Data && a.GetType() == b.GetType();
        }
        
        public static bool operator !=(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            return !(a == b);
        }

        public static bool operator <=(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            if (CheckReferenceEquality(a, b) == ReferenceEquality.SameObjects)
            {
                return true;
            } else if (CheckReferenceEquality(a, b) == ReferenceEquality.OneIsNull)
            {
                return false;
            }
            
            if (a.GetType() != b.GetType())
            {
                throw new InvalidOperationException("Invalid properties for operator <=");
            }

            return a.Data <= b.Data;
        }
        
        public static bool operator >=(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            if (CheckReferenceEquality(a, b) == ReferenceEquality.SameObjects)
            {
                return true;
            } else if (CheckReferenceEquality(a, b) == ReferenceEquality.OneIsNull)
            {
                return false;
            }

            if (a.GetType() != b.GetType())
            {
                throw new InvalidOperationException("Invalid properties for operator >=");
            }

            return a.Data >= b.Data;
        }
        
        public static bool operator >(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            return !(a <= b);
        }
        
        public static bool operator <(SimpleIntegerProperty a, SimpleIntegerProperty b)
        {
            return !(a >= b);
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
                return Data == ((SimpleIntegerProperty) obj).Data  && obj.GetType() == GetType();
            }
            catch
            {
                return false;
            }
        }

        public bool Equals(SimpleIntegerProperty obj)
        {
            return Equals((object) obj);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Name, Data).GetHashCode();
        }
    }
}