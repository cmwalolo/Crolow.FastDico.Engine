using Kalow.Apps.Common.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace Kalow.Apps.Common.DataTypes
{
    /// <summary>
    /// Represent a 12-bytes BSON type used in document Id
    /// </summary>
    /// 
    [JsonConverter(typeof(KalowIdConverter))]
    public class KalowId : IComparable<KalowId>, IEquatable<KalowId>
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// A zero 12-bytes KalowId
        /// </summary>
        public static KalowId Empty => new KalowId();

        #region Properties

        /// <summary>
        /// Get timestamp
        /// </summary>
        public int Timestamp { get; }

        /// <summary>
        /// Get machine number
        /// </summary>
        public int Machine { get; }

        /// <summary>
        /// Get pid number
        /// </summary>
        public short Pid { get; }

        /// <summary>
        /// Get increment
        /// </summary>
        public int Increment { get; }

        /// <summary>
        /// Get creation time
        /// </summary>
        public DateTime CreationTime
        {
            get { return UnixEpoch.AddSeconds(Timestamp); }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new empty instance of the KalowId class.
        /// </summary>
        public KalowId()
        {
            Timestamp = 0;
            Machine = 0;
            Pid = 0;
            Increment = 0;
        }

        /// <summary>
        /// Initializes a new instance of the KalowId class from KalowId vars.
        /// </summary>
        public KalowId(int timestamp, int machine, short pid, int increment)
        {
            Timestamp = timestamp;
            Machine = machine;
            Pid = pid;
            Increment = increment;
        }

        /// <summary>
        /// Initializes a new instance of KalowId class from another KalowId.
        /// </summary>
        public KalowId(KalowId from)
        {
            Timestamp = from.Timestamp;
            Machine = from.Machine;
            Pid = from.Pid;
            Increment = from.Increment;
        }

        /// <summary>
        /// Initializes a new instance of the KalowId class from hex string.
        /// </summary>
        public KalowId(string value)
            : this(FromHex(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the KalowId class from byte array.
        /// </summary>
        public KalowId(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            Timestamp =
                (bytes[startIndex + 0] << 24) +
                (bytes[startIndex + 1] << 16) +
                (bytes[startIndex + 2] << 8) +
                bytes[startIndex + 3];

            Machine =
                (bytes[startIndex + 4] << 16) +
                (bytes[startIndex + 5] << 8) +
                bytes[startIndex + 6];

            Pid = (short)
                ((bytes[startIndex + 7] << 8) +
                bytes[startIndex + 8]);

            Increment =
                (bytes[startIndex + 9] << 16) +
                (bytes[startIndex + 10] << 8) +
                bytes[startIndex + 11];
        }

        /// <summary>
        /// Convert hex value string in byte array
        /// </summary>
        private static byte[] FromHex(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            if (value.Length != 24) throw new ArgumentException(string.Format("KalowId strings should be 24 hex characters, got {0} : \"{1}\"", value.Length, value));

            var bytes = new byte[12];

            for (var i = 0; i < 24; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }

            return bytes;
        }

        #endregion

        #region Equals/CompareTo/ToString

        /// <summary>
        /// Checks if this KalowId is equal to the given object. Returns true
        /// if the given object is equal to the value of this instance. 
        /// Returns false otherwise.
        /// </summary>
        public bool Equals(KalowId other)
        {
            return other != null &&
                Timestamp == other.Timestamp &&
                Machine == other.Machine &&
                Pid == other.Pid &&
                Increment == other.Increment;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as KalowId);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + Timestamp.GetHashCode();
            hash = 37 * hash + Machine.GetHashCode();
            hash = 37 * hash + Pid.GetHashCode();
            hash = 37 * hash + Increment.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Compares two instances of KalowId
        /// </summary>
        public int CompareTo(KalowId other)
        {
            var r = Timestamp.CompareTo(other.Timestamp);
            if (r != 0) return r;

            r = Machine.CompareTo(other.Machine);
            if (r != 0) return r;

            r = Pid.CompareTo(other.Pid);
            if (r != 0) return r < 0 ? -1 : 1;

            return Increment.CompareTo(other.Increment);
        }

        /// <summary>
        /// Represent KalowId as 12 bytes array
        /// </summary>
        public void ToByteArray(byte[] bytes, int startIndex)
        {
            bytes[startIndex + 0] = (byte)(Timestamp >> 24);
            bytes[startIndex + 1] = (byte)(Timestamp >> 16);
            bytes[startIndex + 2] = (byte)(Timestamp >> 8);
            bytes[startIndex + 3] = (byte)Timestamp;
            bytes[startIndex + 4] = (byte)(Machine >> 16);
            bytes[startIndex + 5] = (byte)(Machine >> 8);
            bytes[startIndex + 6] = (byte)Machine;
            bytes[startIndex + 7] = (byte)(Pid >> 8);
            bytes[startIndex + 8] = (byte)Pid;
            bytes[startIndex + 9] = (byte)(Increment >> 16);
            bytes[startIndex + 10] = (byte)(Increment >> 8);
            bytes[startIndex + 11] = (byte)Increment;
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[12];
            ToByteArray(bytes, 0);
            return bytes;
        }

        public override string ToString()
        {
            return BitConverter.ToString(ToByteArray()).Replace("-", "").ToLower();
        }

        #endregion

        #region Operators

        public static bool operator ==(KalowId lhs, KalowId rhs)
        {
            if (lhs is null) return rhs is null;
            if (rhs is null) return false; // don't check type because sometimes different types can be ==

            return lhs.Equals(rhs);
        }

        public static bool operator !=(KalowId lhs, KalowId rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >=(KalowId lhs, KalowId rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        public static bool operator >(KalowId lhs, KalowId rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator <(KalowId lhs, KalowId rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(KalowId lhs, KalowId rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        #endregion

        #region Static methods

        private static readonly int _machine;
        private static readonly short _pid;
        private static int _increment;

        // static constructor
        static KalowId()
        {
            _machine = GetMachineHash() +
#if HAVE_APP_DOMAIN
                AppDomain.CurrentDomain.Id
#else
                10000 // Magic number
#endif   
                 & 0x00ffffff;
            _increment = new Random().Next();

            try
            {
                _pid = (short)GetCurrentProcessId();
            }
            catch (SecurityException)
            {
                _pid = 0;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
#if HAVE_PROCESS
            return Process.GetCurrentProcess().Id;
#else
            return new Random().Next(0, 5000); // Any same number for this process
#endif
        }

        private static int GetMachineHash()
        {
            var hostName =
#if HAVE_ENVIRONMENT
                Environment.MachineName; // use instead of Dns.HostName so it will work offline
#else
                "SOMENAME";
#endif
            return 0x00ffffff & hostName.GetHashCode(); // use first 3 bytes of hash
        }

        /// <summary>
        /// Creates a new KalowId.
        /// </summary>
        public static KalowId NewObjectId()
        {
            var timestamp = (long)Math.Floor((DateTime.UtcNow - UnixEpoch).TotalSeconds);
            var inc = Interlocked.Increment(ref _increment) & 0x00ffffff;

            return new KalowId((int)timestamp, _machine, _pid, inc);
        }

        #endregion
    }
}