using System;

namespace Serialization
{
    public static class RegisterSerializableTypes
    {
        static Type[] registeredSerializableTypes = new Type[0];

        static RegisterSerializableTypes()
        {
            registeredSerializableTypes = new Type[] {
                // ...
                typeof(Wrapper<int>),
            };
        }

        public static Type[] Retrieve()
        {
            return registeredSerializableTypes;
        }
    }
}
