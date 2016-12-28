using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core.Utils
{
    public abstract class EquatableBase<TType>
            : IEquatable<TType>, IEquatable<EquatableBase<TType>>
            where TType : EquatableBase<TType>
    {
        protected abstract bool IsEqualTo(TType other);

        bool IEquatable<TType>.Equals(TType other)
        {
            return (object)other != null && this.IsEqualTo(other);
        }

        bool IEquatable<EquatableBase<TType>>.Equals(EquatableBase<TType> other)
        {
            return (object)other != null && this.IsEqualTo((TType)other);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is TType && this.IsEqualTo((TType)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(EquatableBase<TType> left, EquatableBase<TType> right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }
            return (object)right != null && left.IsEqualTo((TType)right);
        }

        public static bool operator !=(EquatableBase<TType> left, EquatableBase<TType> right)
        {
            return !(left == right);
        }
    }
}
