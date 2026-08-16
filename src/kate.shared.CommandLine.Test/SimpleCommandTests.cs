/*
   Copyright 2022-2026 Kate Ward <kate@dariox.club>

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
using System.CommandLine;
using JetBrains.Annotations;

namespace kate.shared.CommandLine.Test;

public class Tests
{
    [Test]
    public void SimpleCommandImplicit()
    {
        var c = CommandLineHelper.GenerateCommand<TestAction1.Options, TestAction1>();
        var r = new RootCommand()
        {
            c
        };
        var e = r.Parse(["test"]).Invoke();
        Assert.That(e, Is.EqualTo(0));
    }
    
    [Test]
    public void SimpleCommandExplicit()
    {
        var c = CommandLineHelper.GenerateCommandExplicit<TestAction2.Options, TestAction2>();

        var r = new RootCommand()
        {
            c
        };
        var e = r.Parse(["test"]).Invoke();
        Assert.That(e, Is.EqualTo(0));
    }
    
    [Test]
    public void SimpleCommandExplicitMultiToken()
    {
        var c = CommandLineHelper.GenerateCommandExplicit<TestActionMulti3.Options, TestActionMulti3>();

        var r = new RootCommand()
        {
            c
        };
        var e = r.Parse([
            "test",
            "--array", "bweh1",
            "--array", "test2",
            "--array", "ur mom",
            "--array", "HAHAHA X3!!!"]).Invoke();
        Assert.That(e, Is.EqualTo(0));
    }

    [UsedImplicitly]
    [CommandAction("test", typeof(Options))]
    public class TestAction1 : IAction
    {
        [UsedImplicitly]
        public Task RunAsync(object options)
        {
            if (options is not Options)
                throw new InvalidOperationException("Test failure - invalid type: " + options?.GetType());
            Console.WriteLine("Log from inside TestAction1");
            return Task.CompletedTask;
        }

        [UsedImplicitly]
        public class Options
        {
        }
    }
    
    [UsedImplicitly]
    [CommandAction("test", typeof(Options))]
    public class TestAction2 : IAction<TestAction2.Options>
    {
        [UsedImplicitly]
        public Task RunAsync(Options options)
        {
            Console.WriteLine("Log from inside TestAction2");
            return Task.CompletedTask;
        }

        [UsedImplicitly]
        public class Options
        {
        }
    }
    
    [UsedImplicitly]
    [CommandAction("test", typeof(Options))]
    public class TestActionMulti3 : IAction<TestActionMulti3.Options>
    {
        [UsedImplicitly]
        public Task RunAsync(Options options)
        {
            Console.WriteLine("Log from inside TestActionMulti3");
            Assert.That(options, Is.Not.Null);
            Assert.That(options.ArrayValue, Is.Not.Null);
            Assert.That(options.ArrayValue, Has.Length.EqualTo(4));
            Assert.That(options.ArrayValue[0], Is.EqualTo("bweh1"));
            Assert.That(options.ArrayValue[1], Is.EqualTo("test2"));
            Assert.That(options.ArrayValue[2], Is.EqualTo("ur mom"));
            Assert.That(options.ArrayValue[3], Is.EqualTo("HAHAHA X3!!!"));
            return Task.CompletedTask;
        }

        [UsedImplicitly]
        public class Options
        {
            [ActionParameter("array", "Array", AllowMultipleArgumentsPerToken = true)]
            public string[] ArrayValue { get; set; } = Array.Empty<string>();
        }
    }
}