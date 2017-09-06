using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Security.Cryptography;

//is method what call for create account in zimbra
public class test
{
    public static void CreateNewZimbraAccount(string username, string domainName, string firstName, string lastName)
    {
        SoapParameters myParam = new SoapParameters()
    {
    
    accountUsername = string.Format("{0}@{1}", username, domainName),
    accountFirstName = firstName,
    accountLastName = lastName,
    };
    AccountService.Create(myParam);
    }

    // class AccountService

    public static void Create(SoapParameters param)
    {
    GetAdministratorAuthenticationToken(param);
    CreateAccountRequest(param);
    }

    private static void GetAdministratorAuthenticationToken(SoapParameters param)
    {
    XmlDocument doc = GetResponseDoc(param, SoapMessages.AuthenticationToken);
    param.authToken = doc.GetElementsByTagName("authToken").Item(0).InnerXml;
    }

    private static XmlDocument GetResponseDoc(SoapParameters param, SoapMessages message)
    {
    string soapMessage = SoapMessageBuilder(message, param);
    XmlDocument requestDoc = new XmlDocument();
    requestDoc.LoadXml(soapMessage);
    HttpWebRequest request = HttpWebRequestBuilder(param.serviceAdminSoapUri);
    Stream stm = request.GetRequestStream();
    requestDoc.Save(stm);
    stm.Close();
    WebResponse resp = request.GetResponse(); // here occurs error ---// Unable to //establish a relationship of trust secure channel for SSL / TLS, .
    stm = resp.GetResponseStream();
    StreamReader r = new StreamReader(stm);
    XmlDocument responseDoc = new XmlDocument();
    responseDoc.LoadXml(r.ReadToEnd());
    return responseDoc;
    }

    private static HttpWebRequest HttpWebRequestBuilder(string serviceAdminSoapUri)
    {
    HttpWebRequest retVal = (HttpWebRequest)WebRequest.Create(serviceAdminSoapUri);
    retVal.Headers.Add("SOAPAction", "''");
    retVal.ContentType = "text/xml;charset='utf - 8 '";
    retVal.Accept = "text/xml";
    retVal.Method = "POST";
    return retVal;
    }

    private static string SoapMessageBuilder(SoapMessages message, SoapParameters param){
    string retVal = "";
    switch(message)
    {
    case SoapMessages.AuthenticationToken :
        
        retVal = "<?xml version=\"1.0\" encoding=\"utf-8\" ?> ";
        retVal += "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\>";
        retVal += " <soap:Header> ";
        retVal += "<context xmlns=\"urn:zimbra\"> ";
        retVal += "<format type=\"xml\"/> ";
        retVal += "</context> ";
        retVal += "</soap:Header> ";
        retVal += "<soap:Body> ";
        retVal += "<AuthRequest xmlns=\"urn:zimbraAdmin\"> ";
        retVal += "<account by=\"name\">cuenta@</account> ";
        retVal += "<password>Clave segura</password> ";
        retVal += "</AuthRequest> ";
        retVal += "</soap:Body> ";
        retVal += "</soap:Envelope>" ;
    break;
    return retVal;
    }
    }
    public  SoapParameters()
    {
    string impersonateDomain = System.Configuration.ConfigurationSettings.AppSettings["Domain.Settings.ImpersonateDomain"];
    string impersonateUsername = System.Configuration.ConfigurationSettings.AppSettings["Domain.Settings.ImpersonateUsername"];
    string impersonatePassword = System.Configuration.ConfigurationSettings.AppSettings["Domain.Settings.ImpersonatePassword"];
    string zimbraServiceAdminSoapUri = System.Configuration.ConfigurationSettings.AppSettings["Domain.Settings.ZimbraServiceAdminSoapUri"];
    adminUsername = string.Format("{0}@{1}", impersonateUsername, impersonateDomain);
    adminPassword = impersonatePassword;
    serviceAdminSoapUri = zimbraServiceAdminSoapUri;
    }

    private static void CreateAccountRequest(SoapParameters param)
    {
    XmlDocument doc = GetResponseDoc(param, SoapMessages.CreateAccount);
    try
    {
    string value = doc.GetElementsByTagName("CreateAccountResponse").Item(0).ChildNodes[0].Attributes["id"].Value;
    Guid guid = new Guid(value);
    param.accountId = guid.ToString();
    }
    catch
    {
    throw new Exception("Account Did Not Appear To Be Created.");
    }
    }
    private enum SoapMessages
    {
    AuthenticationToken,
    CreateAccount,
    RemoveAccount,
    Account
    }

}