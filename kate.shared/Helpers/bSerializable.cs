using System;
using System.Collections.Generic;
using System.Text;

namespace kate.shared.Helpers
{
#if NET8_0_OR_GREATER == false
    [Obsolete("Removed in v1.5")]
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
