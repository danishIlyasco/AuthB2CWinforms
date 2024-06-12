using Microsoft.Identity.Client;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using NetFwTypeLib;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace AuthB2C
{
    public partial class Form1 : Form
    {
        private IPublicClientApplication _clientApp;

        private static readonly object _lockObject = new object();

        public Form1()
        {
            InitializeComponent();

            Trace.Write("Form Created" + Environment.NewLine);

            // Initialize the MSAL client application.
            _clientApp = PublicClientApplicationBuilder.Create("Client id here")
            .WithB2CAuthority("https://{Your Tennat name}.b2clogin.com/tfp/{Your Tennat name}.onmicrosoft.com/{Your policy Flow}/oauth2/v2.0/authorize")
            .WithRedirectUri("http://localhost:1234")
            .Build();

            Trace.Write("Before EnableAppThroughFireWall" + Environment.NewLine);

            EnableAppThroughFireWall();

            Trace.Write("After EnableAppThroughFireWall" + Environment.NewLine);
        }

        public async void button1_Click(object sender, EventArgs e)
        {
            string[] scopes = { "Your Scope id here" };

            Trace.Write("Button Clicked" + Environment.NewLine);

            Task.Run(() =>
            {
                Trace.Write("Thread! started to Monitor" + DateTime.Now + Environment.NewLine);

                while (true)
                {

                    Thread.Sleep(2000);
                    if (!IsBusy(1234))
                    {
                        Trace.Write("Thread! port is not listening" + DateTime.Now + Environment.NewLine);
                    }
                    else
                    {
                        Trace.Write("Thread! port is listening" + DateTime.Now + Environment.NewLine);
                    }
                }
            });

            try
            {
                Trace.Write("Token! Before sending Token Request" + DateTime.Now + Environment.NewLine);

                /* var authResult = Task.Run(async () => await _clientApp.AcquireTokenInteractive(scopes).WithUseEmbeddedWebView(false)
                 .ExecuteAsync()).Result;*/

                // Attempt to acquire an access token from the cache, or through an interactive prompt
                AuthenticationResult authResult = await _clientApp.AcquireTokenInteractive(scopes)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync();

                Trace.Write($"Token:{authResult.AccessToken} Authentication Successful" + DateTime.Now + Environment.NewLine);

                MessageBox.Show($"Token:\n{authResult.AccessToken}", "Authentication Successful");
            }
            catch (MsalUiRequiredException ex)
            {
                Trace.Write(DateTime.Now + $"Token:{ex.Message} Authentication Failed" + Environment.NewLine);
                MessageBox.Show("The user needs to sign-in interactively.", "Authentication Required");
            }
            catch (Exception ex)
            {
                Trace.Write(DateTime.Now + $"Token:{ex.Message} Authentication Failed Generic expception" + Environment.NewLine);
                MessageBox.Show(ex.Message, "Authentication Failed");
            }

            Application.Exit();
        }

        public bool IsBusy(int port)
        {
            IPGlobalProperties ipGP = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endpoints = ipGP.GetActiveTcpListeners();
            if (endpoints == null || endpoints.Length == 0) return false;
            for (int i = 0; i < endpoints.Length; i++)
                if (endpoints[i].Port == port)
                    return true;
            return false;
        }

        public void WriteToLogs(string fileName, string text)
        {
            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(text);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("The file is being used by another process.");
                MessageBox.Show(ex.Message, "Authentication Failed" + fileName );
            }

        }

        public void EnableAppThroughFireWall()
        {
            var process = Process.GetCurrentProcess(); // Or whatever method you are using
            string fullPath = process.MainModule.FileName;

            string enableFireWallCommand = $"firewall add allowedprogram {fullPath} AuthB2C ENABLE";

            bool enableResult = RunCommand("netsh", enableFireWallCommand, "Command executed successfully");

            if (enableResult)
            {
                Trace.Write($"Firewall Rule! Executed : Enable Rule using Netsh." + DateTime.Now + Environment.NewLine);
            }


            if (!CheckFirewallRuleExists("MagicTasks"))
            {
                string addInboundPortRuleCommand = $"advfirewall firewall add rule name=\"MagicTasks\" dir=in action=allow protocol=TCP localport=1234";

                bool ruleCreationResult = RunCommand("netsh", addInboundPortRuleCommand, "Ok");

                if (ruleCreationResult)
                {
                    Trace.Write($"Firewall Rule! Executed : Enable Rule using Netsh." + DateTime.Now + Environment.NewLine);
                }
                else
                {
                    CreateFirewallRule();
                }
            }
        }

        public bool RunCommand(string fileName, string command, string expectedOutput)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string output = reader.ReadToEnd();
                        return output.Contains(expectedOutput);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Write(DateTime.Now + $"FireWall Rule!: {ex.Message} Failed to create using netsh. {command}" + Environment.NewLine);
                return false;
            }
        }


        public bool CheckFirewallRuleExists(string ruleName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall show rule name=\"{ruleName}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    return output.Contains(ruleName);
                }
            }
        }

        public void RunNetshCommand(string command)
        {
            try
            {
                System.Diagnostics.Process.Start("netsh.exe", command);
                Trace.Write(DateTime.Now + $"Firewall Rule! Executed : {command}." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Trace.Write(DateTime.Now + $"FireWall Rule!: {ex.Message} Failed to create using netsh. {command}" + Environment.NewLine);
            }
        }

        public void CreateFirewallRule()
        {
            try
            {
                Trace.Write(DateTime.Now + $"FireWall Rule! Adding FireWall Rule" + Environment.NewLine);

                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                var currentProfiles = fwPolicy2.CurrentProfileTypes;

                // Let's create a new rule
                INetFwRule2 inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                inboundRule.Enabled = true;
                //Allow through firewall
                inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                //Using protocol TCP
                inboundRule.Protocol = 6; // TCP
                                          //Port 81
                inboundRule.LocalPorts = "1234";
                //Name of rule
                inboundRule.Name = "MagicTaskRule";
                // ...//
                inboundRule.Profiles = currentProfiles;

                // Now add the rule
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(inboundRule);

                Trace.Write(DateTime.Now + $"FireWall Rule! 'MagicTaskRule' is added successfully!" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Trace.Write(DateTime.Now + $"FireWall Rule! Exception occurred while creating rule using Dll" + Environment.NewLine);
            }
        }


        public static void SafeWriteToLog(string message)
        {
            lock (_lockObject)
            {
                // Write to the log file
                File.AppendAllText("log.txt", message + Environment.NewLine);
            }
        }

        /*private void openport(int port)
        {
            var powershell = Powershell.create();
            var pscommand = $"new-netfirewallrule -displayname \"<rule description>\" -direction inbound -localport {port} -protocol tcp -action allow";
            powershell.commands.addscript(pscommand);
            powershell.invoke();
        }*/

    }
}
