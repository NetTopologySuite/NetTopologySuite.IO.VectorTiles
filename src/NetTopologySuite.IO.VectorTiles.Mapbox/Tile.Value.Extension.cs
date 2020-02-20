using System.Globalization;
using System.Text;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    partial class Tile
    {
        partial class Value
        {
            public static bool OverrideGetHashCodeAndEquals { get; set; }

            private int _hashCode = 0;
            public override int GetHashCode()
            {
                if (_hashCode == 0)
                    _hashCode = ComputeHashCode();

                return _hashCode;
            }

            private int ComputeHashCode()
            {
                if (!OverrideGetHashCodeAndEquals)
                    return base.GetHashCode();
                
                int res = 17 ^ GetType().GetHashCode();
                if (HasBoolValue)
                    res ^= _boolValue.GetHashCode();
                else if (HasDoubleValue)
                    res ^= _doubleValue.GetHashCode();
                else if (HasFloatValue)
                    res ^= _floatValue.GetHashCode();
                else if (HasIntValue)
                    res ^= _intValue.GetHashCode();
                else if (HasUIntValue)
                    res ^= _uintValue.GetHashCode();
                else if (HasSIntValue)
                    res ^= _sintValue.GetHashCode();
                else if (HasSIntValue)
                    res ^= _stringValue?.GetHashCode() ?? 0;

                return res;
            }

            public override bool Equals(object obj)
            {
                if (!OverrideGetHashCodeAndEquals)
                    return base.Equals(obj);

                if (!(obj is Value other))
                    return false;

                if (HasBoolValue && other.HasBoolValue)
                    return _boolValue == other._boolValue;
                if (HasDoubleValue && other.HasDoubleValue)
                    return _doubleValue == other._doubleValue;
                if (HasFloatValue && other.HasFloatValue)
                    return _floatValue == other._floatValue;
                if (HasIntValue && other.HasIntValue)
                    return _intValue == other._intValue;
                if (HasUIntValue && other.HasUIntValue)
                    return _uintValue == other._uintValue;
                if (HasSIntValue && other.HasSIntValue)
                    return _sintValue == other._sintValue;
                if (HasStringValue && other.HasStringValue)
                    return _stringValue == other._stringValue;

                return false;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("Tile.Value(");
                if (HasBoolValue)
                    sb.AppendFormat("Bool: {0}", _boolValue);
                else if (HasDoubleValue)
                    sb.AppendFormat(NumberFormatInfo.InvariantInfo, "Double: {0:R}", _doubleValue);
                else if (HasFloatValue)
                    sb.AppendFormat(NumberFormatInfo.InvariantInfo, "Float: {0:R}", _floatValue);
                else if (HasIntValue)
                    sb.AppendFormat("Int: {0}", _intValue);
                else if (HasUIntValue)
                    sb.AppendFormat("Uint: {0}", _uintValue);
                else if (HasSIntValue)
                    sb.AppendFormat("Sint: {0}", _sintValue);
                else if (HasSIntValue)
                    sb.AppendFormat("String: {0}", _stringValue);
                sb.Append(")");

                return sb.ToString();
            }
        }
    }
}
