/*
   Copyright 2022-2025 Kate Ward <kate@dariox.club>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
        public static Regex JSON = new Regex(@"{[^}]+}");
    }
}
