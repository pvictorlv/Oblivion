using System;
using System.Text;

namespace Oblivion.Connection;

public class CrossDomainSettings
{
    #region Fields

    private byte[] xmlPolicyBytes;

    #endregion Fields

    #region Constructors

    public CrossDomainSettings(string domain, int port)
    {
        string[] lines = new string[]
        {
            "<?xml version=\"1.0\"?>",
            "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">",
            "<cross-domain-policy>",
            "<allow-access-from domain=\"" + domain + "\" to-ports=\"" + port + "\" />",
            "</cross-domain-policy>\0"
        };

        xmlPolicyBytes = Encoding.ASCII.GetBytes(String.Join("\r\n", lines));
    }

    #endregion Constructors

    #region Methods

    public byte[] GetBytes()
    {
        return xmlPolicyBytes;
    }

    #endregion Methods
}