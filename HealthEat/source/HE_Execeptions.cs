using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat.Exceptions
{
    /// <summary>
    /// Enum type used to identify the exception kind
    /// </summary>
    enum HE_ExceptionType : uint
    {
        Soft = 0u,
        Critical = 1u
    }


    /// <summary>
    /// Base class for HealthEat exceptions. Extends standard Exception class with exception kind
    /// </summary>
    internal class HE_Exception : Exception
    {
        /// <summary>
        /// Default constructor. Specifies kind as Soft
        /// </summary>
        /// <param name="msg">Error message to display</param>
        public HE_Exception(string msg) : this(msg, HE_ExceptionType.Soft) { }

        /// <summary>
        /// Initializes new exception with specified exception type
        /// </summary>
        /// <param name="msg">Error message to display</param>
        /// <param name="type">Exception type</param>
        public HE_Exception(string msg, HE_ExceptionType type) : base(msg)
        {
            m_Type = type;
        }

        public static string FillMessage(string msg, params object[] args)
        {
            string output = String.Format(msg, args);
            return output;
        }

        /// <summary>
        /// Type of exception
        /// </summary>
        public HE_ExceptionType Type => m_Type;
        private readonly HE_ExceptionType m_Type = HE_ExceptionType.Soft;
    }


    /// <summary>
    /// Exception thrown when error while loading asset occurs.
    /// Exception of kind Critical.
    /// </summary>
    internal class HE_AssetLoadException : HE_Exception
    {
        /// <summary>
        /// Initializes new exceptions with specified message and optionally name of asset that failed to load
        /// </summary>
        /// <param name="msg">Error message to be displayed</param>
        /// <param name="asssetName">Name of asset</param>
        public HE_AssetLoadException(string msg, string? asssetName) : this(msg + asssetName) { }
        public HE_AssetLoadException(string msg) : base(msg, HE_ExceptionType.Critical) { }
    }


    /// <summary>
    /// Exception thrown when application tries to access missing asset
    /// </summary>
    internal class HE_MissingAssetException : HE_Exception
    {
        /// <summary>
        /// Initializes new exception with specified message and optionally name of asset that is missing
        /// </summary>
        /// <param name="msg">Error message to be displayed</param>
        /// <param name="asssetName">Name of asset</param>
        public HE_MissingAssetException(string msg, string? asssetName) : this(msg + asssetName) { }
        public HE_MissingAssetException(string msg) : base(msg, HE_ExceptionType.Critical) { }
    }


    internal class HE_InvalidArgumentValueException : HE_Exception
    {
        public HE_InvalidArgumentValueException(string msg) : base(msg, HE_ExceptionType.Critical) { }

    }


    internal class HE_LogicException : HE_Exception
    {
        public HE_LogicException(string msg, HE_ExceptionType type) : base(msg, type) { }
    }
}
