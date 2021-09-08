using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Bussiness.Helpers
{
    public static class ChatRooms
    {
        public static Dictionary<string, HashSet<string>> chatRooms = new Dictionary<string, HashSet<string>>();
    }
}
