using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Aspects
{
    public enum AspectsPriority
    {
        Synchronized = 200,
        SoapExceptionAttribute = 30,
        AutoLog = 20,
        OnErrorVerifyConnection = 15,
        StopExceptions = 10
    }
}
