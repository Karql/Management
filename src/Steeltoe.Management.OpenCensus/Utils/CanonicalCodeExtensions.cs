﻿using Steeltoe.Management.Census.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Census.Utils
{
    [Obsolete("Use OpenCensus project packages")]
    public static class CanonicalCodeExtensions
    {
        public static Status ToStatus(this CanonicalCode code)
        {
            return new Status(code);
        }
    }
}
