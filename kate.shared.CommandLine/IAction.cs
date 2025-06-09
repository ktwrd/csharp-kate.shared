using System;
using System.Collections.Generic;
using System.Text;
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
