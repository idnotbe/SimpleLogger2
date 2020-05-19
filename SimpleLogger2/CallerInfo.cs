using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLogger2
{
    public enum CallerInfo
    {
        SourceLine,
        ClassMethod, // 퍼포먼스 떨어짐
        NoCallerInfo
    }
}