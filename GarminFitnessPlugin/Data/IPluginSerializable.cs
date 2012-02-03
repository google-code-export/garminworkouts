using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace GarminFitnessPlugin.Data
{
    abstract class IPluginSerializable
    {
        public void Deserialize(Stream stream, DataVersion version)
        {
            MethodInfo deserializeMethod = GetLatestDeserializeMethod(this.GetType(), version);

            if (deserializeMethod != null)
            {
                deserializeMethod.Invoke(this, new object[] { stream, version });
            }
        }

        protected void Deserialize(Type forcedType, Stream stream, DataVersion version)
        {
            MethodInfo deserializeMethod = GetLatestDeserializeMethod(forcedType, version);
            Debug.Assert(deserializeMethod != null);

            deserializeMethod.Invoke(this, new object[] { stream, version });
        }

        private MethodInfo GetLatestDeserializeMethod(Type forcedType, DataVersion version)
        {
            int bestVersionNumber = -1;
            MethodInfo deserializeMethod = null;
            MethodInfo[] methods = forcedType.GetMethods();

            for (int i = 0; i < methods.Length; ++i)
            {
                MethodInfo currentMethod = methods[i];

                if (currentMethod.Name.StartsWith(Constants.DeserializeMethodNamePrefix) &&
                    currentMethod.DeclaringType.FullName == forcedType.FullName)
                {
                    Byte currentMethodVersionNumber = 0;

                    try
                    {
                        currentMethodVersionNumber = Byte.Parse(currentMethod.Name.Substring(Constants.DeserializeMethodNamePrefix.Length));
                    }
                    catch
                    {
                        // Too big version for Byte?????
                        Debug.Assert(false);
                    }

                    if (currentMethodVersionNumber > bestVersionNumber && currentMethodVersionNumber <= version.VersionNumber)
                    {
                        deserializeMethod = currentMethod;
                        bestVersionNumber = currentMethodVersionNumber;
                    }
                }
            }

            return deserializeMethod;
        }

        public abstract void Serialize(Stream stream);
    }
}
