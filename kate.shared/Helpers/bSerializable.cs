using System;
using System.Collections.Generic;
using System.Text;

namespace kate.shared.Helpers
{
#if NET8_0_OR_GREATER == false
    public interface bSerializable
    {
        void ReadFromStream(SerializationReader sr);
        void WriteToStream(SerializationWriter sw);
    }

    public interface iSerializable
    {
        void WriteToStreamIrc(SerializationWriter sw);
    }
#endif
}
