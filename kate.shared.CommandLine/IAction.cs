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

using System.Threading.Tasks;

namespace kate.shared.CommandLine
{
    /// <summary>
    /// <para>Inherited by a class that can be used for a CLI action.</para>
    ///
    /// Inherited class must have <see cref="CommandActionAttribute"/>
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Run this action.
        /// </summary>
        /// <param name="options">
        /// Parsed command-line options. Will be deserialized into the type provided in <see cref="CommandActionAttribute.OptionsType"/>
        /// </param>
        Task RunAsync(object options);
    }
}