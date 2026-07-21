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
using System.Reflection;
using System.Text;

namespace kate.shared
{
    /// <summary>
    /// Exception used when there are issues reading or finding an embedded resource.
    /// </summary>
    public class EmbeddedResourceException : Exception
    {
        #region Constructors
        /// <inheritdoc/>
        public EmbeddedResourceException() : this(null, null)
        { }

        /// <inheritdoc/>
        public EmbeddedResourceException(string message) : this(message, null)
        { }

        /// <inheritdoc/>
        public EmbeddedResourceException(string message, Exception innerException) : base(message, innerException)
        {
            Assembly = null;
            SearchedAssemblies = null;
            ResourceName = null;
            ResourceExists = null;
        }
        #endregion

        /// <summary>
        /// Assembly that was being used when reading (or searching) for <see cref="ResourceName"/>
        /// </summary>
        public Assembly Assembly { get; set; }
        /// <summary>
        /// Enumerable of Assemblies that the resource was searched in.
        /// </summary>
        public IList<Assembly> SearchedAssemblies { get; set; }
        /// <summary>
        /// Full name of the resource (asm.namespace.file)
        /// </summary>
        public string ResourceName { get; set; }
        /// <summary>
        /// Does the resource exist? Will be <see langword="null"/> if this doesn't matter.
        /// </summary>
        public bool? ResourceExists { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Assembly != null || ResourceName != null || ResourceExists != null)
            {
                var sb = new StringBuilder(base.ToString());

                sb.AppendLine();
                sb.Append("".PadRight(40, '-'));
                sb.AppendFormat(" {0}", GetType().Name);
                sb.AppendLine();
                sb.AppendLine(string.Format("Assembly: {0}", Assembly));
                if (SearchedAssemblies != null && SearchedAssemblies.Count > 0)
                {
                    for (int i = 0; i < SearchedAssemblies.Count; i++)
                    {
                        var s = string.Format("SearchedAssemblies[{0}]: {1}",
                            i,
                            SearchedAssemblies[i].ToString());
                        sb.AppendLine(s);
                    }
                }
                sb.AppendLine(string.Format("ResourceName: {0}", ResourceName));
                sb.AppendLine(string.Format("ResourceExists: {0}", ResourceExists));
                return sb.ToString();
            }
            return base.ToString();
        }
    }
}
