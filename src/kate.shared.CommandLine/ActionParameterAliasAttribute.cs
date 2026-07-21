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

namespace kate.shared.CommandLine
{
    /// <summary>
    /// Define multiple aliases for <see cref="ActionParameterAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ActionParameterAliasAttribute : Attribute
    {
        public ActionParameterAliasAttribute(string alias, ActionParameterAliasKind kind)
        {
            Alias = alias;
            CustomPrefix = null;
            Kind = kind;
        }

        public ActionParameterAliasAttribute(string alias)
            : this(alias, ActionParameterAliasKind.DoubleDash)
        {
        }

        public string Alias { get; set; }
        public string CustomPrefix { get; set; }
        public ActionParameterAliasKind Kind { get; set; }
    }
}