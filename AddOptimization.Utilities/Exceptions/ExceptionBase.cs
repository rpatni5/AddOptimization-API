using AddOptimization.Utilities.Interface;
using System;
using System.Collections.Generic;

namespace AddOptimization.Utilities.Exceptions
{
    public abstract class ExceptionBase : Exception, IException
    {
        protected ExceptionBase(string type, IEnumerable<string> codes)
        {
            Type = type;
            Code = string.Join(',', codes);
        }

        protected ExceptionBase(string type, string code)
        {
            Type = type;
            Code = code;
        }

        protected ExceptionBase(string type, string code, Exception ex)
            : base("see inner exception", ex)
        {
            Type = type;
            Code = code;
        }

        protected ExceptionBase(string type, string code, string details)
        {
            Type = type;
            Code = code;
            Details = details;
        }

        public string Code { get; private set; }

        public abstract int ViolationCode { get; }
        public abstract object UserMessage { get; }

        public string Details { get; private set; }

        public string Type { get; private set; }
    }
}