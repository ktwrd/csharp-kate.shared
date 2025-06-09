﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace kate.shared.Helpers
{
#if NET8_0_OR_GREATER == false
    [Obsolete("Removed in v1.5")]
    public class DynamicDeserializer
    {
        private static VersionConfigToNamespaceAssemblyObjectBinder versionBinder;
        private static BinaryFormatter formatter;


        private static void Initialize()
        {
            versionBinder = new VersionConfigToNamespaceAssemblyObjectBinder();
            formatter = new BinaryFormatter();
            formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
            formatter.Binder = versionBinder;
            
        }

        public static Object Deserialize(Stream stream)
        {
            if (formatter == null)
                Initialize();
            return formatter.Deserialize(stream);
        }

        #region Nested type: VersionConfigToNamespaceAssemblyObjectBinder

        public sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
        {
            private readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();

            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize;

                if (cache.TryGetValue(assemblyName + typeName, out typeToDeserialize))
                    return typeToDeserialize;

                List<Type> tmpTypes = new List<Type>();
                Type genType = null;

                try
                {
                    if (typeName.Contains("System.Collections.Generic") && typeName.Contains("[["))
                    {
                        string[] splitTyps = typeName.Split(new char[] { '[' });

                        foreach (string typ in splitTyps)
                        {
                            if (typ.Contains("Version"))
                            {
                                string asmTmp = typ.Substring(typ.IndexOf(',') + 1);
                                string asmName = asmTmp.Remove(asmTmp.IndexOf(']')).Trim();
                                string typName = typ.Remove(typ.IndexOf(','));
                                tmpTypes.Add(BindToType(asmName, typName));
                            }
                            else if (typ.Contains("Generic"))
                            {
                                genType = BindToType(assemblyName, typ);
                            }
                        }
                        if (genType != null && tmpTypes.Count > 0)
                        {
                            return genType.MakeGenericType(tmpTypes.ToArray());
                        }
                    }

                    string ToAssemblyName = assemblyName.Split(',')[0];
                    Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly a in Assemblies)
                    {
                        if (a == null) continue;
                        if (a.FullName == null) continue;
                        if (a.FullName.Split(',')[0] == ToAssemblyName)
                        {
                            typeToDeserialize = a.GetType(typeName);
                            break;
                        }
                    }
                }
                catch
                {
                    throw;
                }

                cache.Add(assemblyName + typeName, typeToDeserialize);

                return typeToDeserialize;
            }
        }

        #endregion
    }
#endif
}
