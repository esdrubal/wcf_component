using System.Collections.Generic;

// Populated from external/wcf/src/System.Private.ServiceModel/tests/Common/Infrastructure/src/testproperties.props
namespace Infrastructure.Common
{
    public static partial class TestProperties
    {
        public static readonly string BridgeResourceFolder_PropertyName = "BridgeResourceFolder";
        public static readonly string BridgeHost_PropertyName = "BridgeHost";
        public static readonly string BridgePort_PropertyName = "BridgePort";
        public static readonly string BridgeHttpPort_PropertyName = "BridgeHttpPort";
        public static readonly string BridgeHttpsPort_PropertyName = "BridgeHttpsPort";
        public static readonly string BridgeTcpPort_PropertyName = "BridgeTcpPort";
        public static readonly string BridgeWebSocketPort_PropertyName = "BridgeWebSocketPort";
        public static readonly string BridgeRemoteEnabled_PropertyName = "BridgeRemoteEnabled";
        public static readonly string BridgeCertificatePassword_PropertyName = "BridgeCertificatePassword";
        public static readonly string BridgeCertificateValidityPeriod_PropertyName = "BridgeCertificateValidityPeriod";
        public static readonly string UseFiddlerUrl_PropertyName = "UseFiddlerUrl";
        public static readonly string BridgeMaxIdleTimeSpan_PropertyName = "BridgeMaxIdleTimeSpan";
        public static readonly string MaxTestTimeSpan_PropertyName = "MaxTestTimeSpan";

        static partial void Initialize(Dictionary<string, string> properties)
        {
            properties["BridgeResourceFolder"] = ".";
            properties["BridgeHost"] = "localhost";
            properties["BridgePort"] = "44283";
            properties["BridgeHttpPort"] = "8081";
            properties["BridgeHttpsPort"] = "44285";
            properties["BridgeTcpPort"] = "809";
            properties["BridgeWebSocketPort"] = "8083";
            properties["BridgeRemoteEnabled"] = "false";
            properties["BridgeCertificatePassword"] = "test";
            properties["BridgeCertificateValidityPeriod"] = "1.00:00:00";
            properties["UseFiddlerUrl"] = "false";
            properties["BridgeMaxIdleTimeSpan"] = "24:00:00";
            properties["MaxTestTimeSpan"] = "00:20";
        }
    }
}
