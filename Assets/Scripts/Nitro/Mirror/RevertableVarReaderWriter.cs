using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nitro.Mirror
{

    public static class RevertableVarReaderWriter
    {
        public static void SyncTest<T>(RevertableVar<T> revertableVar, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            //Debug.LogError("Member Name = " + memberName);
            //Debug.LogError("Source File Path = " + sourceFilePath);
            //Debug.LogError("Source Line Number = " + sourceLineNumber);
        }
    }
}
