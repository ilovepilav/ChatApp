using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Bussiness.Helpers
{
    public static class ConnectedUsers
    {
        //ConcurrentDictionary kullanılabilir.
        public static Dictionary<string, HashSet<string>> UserList = new Dictionary<string, HashSet<string>>();
    }
}
