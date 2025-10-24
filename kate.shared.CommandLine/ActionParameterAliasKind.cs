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

namespace kate.shared.CommandLine
{
    /// <summary>
    /// Kind of alias for <see cref="ActionParameterAliasAttribute"/>
    /// </summary>
    public enum ActionParameterAliasKind
    {
        /// <summary>
        /// Value for <see cref="ActionParameterAliasAttribute.Alias"/> will have no prefix for the alias, and will be passed to System.CommandLine as-is.
        /// </summary>
        NoPrefix,
        /// <summary>
        /// Value for <see cref="ActionParameterAliasAttribute.Alias"/> will have <c>-</c> as the prefix when added as an alias.
        /// </summary>
        SingleDash,
        /// <summary>
        /// Value for <see cref="ActionParameterAliasAttribute.Alias"/> will have <c>--</c> as the prefix when added as an alias.
        /// </summary>
        DoubleDash,
        /// <summary>
        /// Value for <see cref="ActionParameterAliasAttribute.Alias"/> will have the value at <see cref="ActionParameterAliasAttribute.CustomPrefix"/> as the prefix when added as an alias (only when it's not <see langword="null"/>, otherwise no prefix is used.)
        /// </summary>
        Custom
    }
}