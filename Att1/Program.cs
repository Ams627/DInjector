using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dinject.Nac;
using Dinject.NaC;

namespace Dinject
{
    class Mapper
    {
        public MethodInfo Commmand { get; set; }
        public MethodInfo PreExecute { get; set; } 
        public MethodInfo GetHelp { get; set; }
    }
    class CommandManager
    {
        private readonly Dictionary<string, Mapper> _commandToType = new Dictionary<string, Mapper>();
        private readonly HashSet<Type> _naTypes = new HashSet<Type> { typeof(NaCommandAttribute), typeof(NaCommandHelpAttribute), typeof(NaPreExecuteAttribute) };
        public CommandManager(Assembly asm = null)
        {
            var commandClasses = from type in asm.GetTypes()
                                 let att = Attribute.GetCustomAttribute(type, typeof(NaClassAttribute))
                                 where att != null
                                 select type;

            var methods = (from commandClass in commandClasses
                           from method in commandClass.GetMethods()
                           from attribute in method.GetCustomAttributes()
                           where _naTypes.Contains(attribute.GetType())
                           select new { attribute, method });

            var names = from method in methods
                        where method.attribute is NaCommandAttribute
                        select new { cmd = ((NaCommandAttribute)method.attribute).Name, method.method };


            // can't have more than one command with a given name:
            var dupCommands = names.ToLookup(x => x.cmd).Where(y => y.Count() > 1);
            if (dupCommands.Any())
            {
                foreach (var cmd in dupCommands)
                {

                }
            }

            // get attributes by type where there is a NaCommand attribute:
            var byType =
                methods.ToLookup(x => x.method.DeclaringType).Where(y => y.Any(z => z.attribute.GetType() == typeof(NaCommandAttribute)));

            foreach (var entry in byType)
            {
                string cmd = "";
                
                MethodInfo command = null;
                MethodInfo commandHelp = null;
                MethodInfo preExecute = null;

                foreach (var e2 in entry)
                {
                    if (e2.attribute is NaCommandAttribute nac)
                    {
                        cmd = nac.Name;
                        command = e2.method;
                    }
                    else if (e2.attribute is NaCommandHelpAttribute help)
                    {
                        commandHelp = e2.method;
                    }
                    else if (e2.attribute is NaPreExecuteAttribute pre)
                    {
                        commandHelp = e2.method;
                    }
                }

                if (!string.IsNullOrEmpty(cmd))
                {
                    _commandToType.Add(cmd, new Mapper
                    {
                        Commmand = command,
                        GetHelp = commandHelp,
                        PreExecute = preExecute
                    });
                }
            }
        }

        private void ReportMultiError(ILookup<Attribute, MethodInfo> methods)
        {
            throw new NotImplementedException();
        }

        public string[] Commands => _commandToType.Keys.ToArray();

        public string Help(string command)
        {
            if (!_commandToType.TryGetValue(command, out var cmdFuncs))
            {
                throw new Exception($"unknown command: {command}");
            }

            var declType = cmdFuncs.GetHelp.DeclaringType;
            var n = Activator.CreateInstance(declType);
            cmdFuncs.PreExecute.Invoke(n, null);
            return (string)cmdFuncs.GetHelp.Invoke(n, null);
        }
    }

    [NaClass]
    class X
    {
        [NaCommand("hello")]
        public void Command()
        {

        }

        [NaCommandHelp]
        public string Help()
        {
            return "12345";
        }
    }

    interface I2
    {

    }

    interface I1
    {

    }

    class C1 : I1
    {

    }

    class C1a : I1
    {

    }

    class C2 : I2
    { 
    }


    class Program
    {
        private static void Main(string[] args)
        {
            //var asm = Assembly.GetExecutingAssembly();
            //var cmdManager = new CommandManager(asm);
            //var r = cmdManager.Help("hello");
            //Console.WriteLine("Finished");


            var c1a = new C1a();

            var dm = new DependencyManager();
            dm.AddMapping<I1>(c1a);

            var res = dm.GetMapping<I1>();
            Console.WriteLine($"{res.GetType()}");
        }
    }
}
