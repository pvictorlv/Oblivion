namespace Oblivion.Configuration
{
    internal static class CrossDomainSettings
    {
        internal static byte[] XmlPolicyBytes;

        internal static void Set()
        {
            XmlPolicyBytes =
                Oblivion.GetDefaultEncoding()
                    .GetBytes(
                        "<?xml version=\"1.0\"?>\r\n<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n<cross-domain-policy>\r\n<allow-access-from domain=\"*\" to-ports=\"" +
                        ConfigurationData.Data["game.tcp.port"] + "\" />\r\n</cross-domain-policy>\0");
        }
    }
}