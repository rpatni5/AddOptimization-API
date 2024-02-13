using System;
using System.Collections.Generic;

namespace AddOptimization.Utilities.Exceptions
{
    public class RuleViolation : ExceptionBase
    {
        public RuleViolation(IEnumerable<string> codes)
            : base("Rule", codes)
        {
        }

        public RuleViolation(string code)
            : base("Rule", code)
        {
        }

        public RuleViolation(string userMessage, Exception ex)
            : base("Rule", userMessage, ex)
        {
        }


        public override object UserMessage
        {
            get { return Code; }
        }

        public override int ViolationCode
        {
            get
            {
                int code;
                return int.TryParse(Details, out code) ? code : 1;
            }
        }
    }
}