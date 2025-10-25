using System.Numerics;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Kpi
{
    public class KpiRate
    {
        public BigInteger Value;

        public KpiRate() { }
        public KpiRate(BigInteger value)
        {
            Value = value;
        }

        public KpiRate(string value)
        {
            if (BigInteger.TryParse(value, out var result))
            {
                Value = result;
            }
            else
            {
                Value = 0;
            }
        }

        public void Set(int key, bool achieved)
        {
            BigInteger mask = BigInteger.One << (int)key;

            if (achieved)
                Value |= mask;
            else
                Value &= ~mask;
        }

        public bool Get(int key)
        {
            BigInteger mask = BigInteger.One << (int)key;
            return (Value & mask) != 0;
        }

        public void Set(KpiKeys key, bool achieved)
        {
            BigInteger mask = BigInteger.One << (int)key;

            if (achieved)
                Value |= mask;
            else
                Value &= ~mask;
        }

        public bool Get(KpiKeys key)
        {
            BigInteger mask = BigInteger.One << (int)key;
            return (Value & mask) != 0;
        }

        public void Reset() => Value = 0;
    }
}
