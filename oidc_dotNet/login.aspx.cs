using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace oidc_dotNet
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string runOnOpen= ConfigurationManager.AppSettings["runOnOpen"];
            string showGoButton = ConfigurationManager.AppSettings["showGoButton"];

            if (showGoButton.ToLower() == "true")
            {
                Button1.Visible = true;
            }


            if (runOnOpen.ToLower() == "true")
            {
                startLogin();
            }

        }


      
        private void startLogin()
        {
            PrincipalContext pc = null;
            UserPrincipal principal = null;
            string domain = "";
            string ignoreSSLError = "";
            string qseok_url = "";
            string path = "";
            string ignoreGroups = "";
            List<string> ignoreGroupsArray = null;
            string onlyGroupsStartingWith = "";
            string localTesting = "false";



            try
            {
                domain = ConfigurationManager.AppSettings["domain"];
                ignoreSSLError = ConfigurationManager.AppSettings["ignoreSSLError"];
                qseok_url = ConfigurationManager.AppSettings["qseok_url"];
                path = ConfigurationManager.AppSettings["path"];
                ignoreGroups = ConfigurationManager.AppSettings["ignoreGroups"];
                onlyGroupsStartingWith = ConfigurationManager.AppSettings["onlyGroupsStartingWith"];
                localTesting = ConfigurationManager.AppSettings["localVS.NETDebug"];


                ignoreGroupsArray = ignoreGroups.Split(',').ToList();


                GenericPrincipal genericPrincipal = GetGenericPrincipal();

                var username = Thread.CurrentPrincipal.Identity.Name;

                var username1 = HttpContext.Current.User.Identity.Name;
                
                

                pc = new PrincipalContext(ContextType.Domain, domain);


                if (pc is null)
                {
                    TextBox1.Text = "PC is null";
                    return;
                }



                if (localTesting.ToLower() == "true")
                {
                    principal = UserPrincipal.FindByIdentity(pc, genericPrincipal.Identity.Name);
                }
                else
                {
                    principal = UserPrincipal.FindByIdentity(pc, username);
                }

                

                var firstName =  principal.GivenName ?? string.Empty;
                var lastName =   principal.Surname ?? string.Empty;
                var groups = principal.GetGroups();

             


                string[] groupArray = { };


                foreach (GroupPrincipal g in groups)
                {
                    if (!ignoreGroupsArray.Contains(g.ToString()))
                    {
                        if (string.IsNullOrEmpty(onlyGroupsStartingWith) || (g.ToString().StartsWith(onlyGroupsStartingWith) && !string.IsNullOrEmpty(onlyGroupsStartingWith)))
                        {
                            
                            Array.Resize(ref groupArray, groupArray.Length + 1);
                            groupArray[groupArray.GetUpperBound(0)] = g.ToString();
                            
                        }
                    }


                }

                string jwt = createJWT(genericPrincipal.Identity.Name, groupArray, firstName + " " + lastName);
                string ticket = getTicket(jwt, ignoreSSLError.ToLower(), qseok_url, path);
                JObject o = JObject.Parse(ticket);
                JObject o1 = JObject.Parse(o.ToString());
                string url = qseok_url + "?qlikticket=" + o1["Ticket"].ToString();
                Response.Redirect(url);

            }
            catch (Exception ex)
            {
                TextBox1.Visible = true;
                TextBox1.Text = ex.Message;
            }
            finally
            {
                if (principal != null) principal.Dispose();
                if (pc != null) pc.Dispose();
            }
        }

        // Create a generic principal based on values from the current
        // WindowsIdentity.
        private static GenericPrincipal GetGenericPrincipal()
        {
            // Use values from the current WindowsIdentity to construct
            // a set of GenericPrincipal roles.
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            string[] roles = new string[10];
            if (windowsIdentity.IsAuthenticated)
            {
                // Add custom NetworkUser role.
                roles[0] = "NetworkUser";
            }

            if (windowsIdentity.IsGuest)
            {
                // Add custom GuestUser role.
                roles[1] = "GuestUser";
            }

            if (windowsIdentity.IsSystem)
            {
                // Add custom SystemUser role.
                roles[2] = "SystemUser";
            }

            // Construct a GenericIdentity object based on the current Windows
            // identity name and authentication type.
            string authenticationType = windowsIdentity.AuthenticationType;
            string userName = windowsIdentity.Name;
            GenericIdentity genericIdentity =
                new GenericIdentity(userName, authenticationType);

            // Construct a GenericPrincipal object based on the generic identity
            // and custom roles for the user.
            GenericPrincipal genericPrincipal =
                new GenericPrincipal(genericIdentity, roles);

            return genericPrincipal;
        }


        private List<GroupPrincipal> GetGroups(string userName)
        {
            List<GroupPrincipal> result = new List<GroupPrincipal>();

            // establish domain context
            PrincipalContext yourDomain = new PrincipalContext(ContextType.Domain);

            // find your user
            UserPrincipal user = UserPrincipal.FindByIdentity(yourDomain, userName);

            // if found - grab its groups
            if (user != null)
            {
                PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

                // iterate over all groups
                foreach (Principal p in groups)
                {
                    // make sure to add only group principals
                    if (p is GroupPrincipal)
                    {
                        result.Add((GroupPrincipal)p);
                    }
                }
            }

            return result;
        }


        private static string createJWT(string username, string[] groups, string name)
        {
            
            var payload = new Dictionary<string, object>
            {
                { "username", username.Replace("\\\\","\\" )},
                 { "id", username },
                 { "name", name },
                { "groups",  groups } 
        
            };

         

             string secret = ConfigurationManager.AppSettings["jwtSecret"];

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            return token;
        }


        private static string getTicket(string jwt, string ignoreSSLError, string url, string path)
        {
            var client = new RestClient(url +path + "/ticket");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + jwt);
            request.AddHeader("content-type", "application/json");

            if (ignoreSSLError != "false")
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            startLogin();
        }
    }
}