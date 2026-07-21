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
    /// Required attribute about details for <see cref="IAction"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandActionAttribute : Attribute
    {
        /// <summary>
        /// Name of the action to use when invoking via command-line.
        /// </summary>
        public string ActionName { get; set; }
        /// <summary>
        /// Type of the class where all the action-specific options are.
        /// </summary>
        public Type OptionsType { get; set; }

        /// <summary>
        /// Display name for this action that will get shown to the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandActionAttribute"/> class.
        /// </summary>
        /// <param name="actionName">Name of the action to use when invoking via command-line.</param>
        /// <param name="optionsType">Type of the class where all the action-specific options are.</param>
        /// <param name="displayName">
        /// <para>Display name for this action that will get shown to the user.</para>
        ///
        /// <para>Defaults to <paramref name="actionName"/></para>
        /// </param>
        public CommandActionAttribute(
            string actionName,
            Type optionsType = null,
            string displayName = null)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentNullException(nameof(actionName), "Cannot be null or empty");
            }
            ActionName = actionName;
            DisplayName = string.IsNullOrEmpty(displayName) ? actionName : displayName;
            OptionsType = optionsType;
        }
    }
}
