using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kate.shared.Helpers
{
    public static class RegExStatements
    {
        public static Regex Base64 = new Regex(@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})$");
        public static Regex Username = new Regex(@"^[0-9A-Za-z_\-]{3,}$");
        public static Regex Password = new Regex(@"^[0-9A-Za-z_\-]{8,}$");
    }
}
