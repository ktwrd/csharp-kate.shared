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

namespace kate.shared.Helpers
{
    /// <summary>
    /// Same as <see cref="Action"/>
    /// </summary>
    public delegate void VoidDelegate();
    /// <summary>
    /// Same as <see cref="Action{Exception}"/>
    /// </summary>
    /// <param name="e"></param>
    public delegate void ExceptionDelegate(Exception e);
    /// <summary>
    /// Same as <see cref="Action{string}"/>
    /// </summary>
    public delegate void StringDelegate(string s);
    /// <summary>
    /// Same as <see cref="Action{bool}"/>
    /// </summary>
    public delegate void BoolDelegate(bool b);
    /// <summary>
    /// Same as <see cref="Func{bool, bool}"/>
    /// </summary>
    public delegate bool BoolReturnDelegate(bool b);
    /// <summary>
    /// Return type is the same as the <paramref name="value"/> type.
    /// </summary>
    public delegate T Constraint<T>(T value);
    /// <summary>
    /// Can be used for events where the value or state of something has changed.
    /// </summary>
    public delegate void ComparisonDelegate<T>(T current, T previous);
    /// <summary>
    /// Delegate used for reporting the progress to a BackgroundWorker.
    /// </summary>
    /// <param name="current">Current progress value. Will not be greater than <paramref name="total"/>. Will be a minimum of `0`.</param>
    /// <param name="total">Highest value of <paramref name="current"/></param>
    /// <param name="complete">Is the task complete?</param>
    /// <param name="message"><b>Nullable.</b> . Message for the progress.</param>
    /// <param name="isError">Did an error happen? If this is `true`, then <paramref name="exception"/> will never be null.</param>
    /// <param name="exception"><b>Nullable.</b> Exception relating to error. Never null when <paramref name="isError"/> is `true`.</param>
    public delegate void ProgressDelegate(
        int current,
        int total,
        bool complete,
        string message,
        bool isError,
        Exception exception);
    /// <summary>
    /// Delegate to use when reporting data for a progress bar.
    /// </summary>
    /// <param name="label">Label to display for user.</param>
    /// <param name="current">Current value, should be no less than `0`.</param>
    /// <param name="max">Maximum value of the progress bar</param>
    public delegate void ProgressLabelDelegate(string label, int current, int max);
}
