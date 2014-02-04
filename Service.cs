using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;
using System.Configuration.Install;
using System.Web;
using System.ServiceModel.Web;
using System.Threading.Tasks;
namespace Microsoft.ServiceModel.Samples
{
    
    public enum MyCustomCommands { FileAccess = 128, Another = 129 };
    public class DataSender
    {
        public string send { get; set; }
    }
    public class Away
    {
        private static string mycontent;
        private static bool status;
        private static string sendback;
        public string content
        {
            get { return mycontent; }
            set { mycontent = value; }
        }
        public bool statusManager
        {
            get { return status; }
            set { status = value; }
        }
        public string sendManager
        {
            get { return sendback; }
            set { sendback = value; }
        }
    }
    // Define a service contract.
    [ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples")]
    public interface ICalculator
    {

        [OperationContract]
        //[WebGet(UriTemplate = "/form?content={stuff}&name={action}", ResponseFormat = WebMessageFormat.Json)]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/form")]
        string receiver(string stuff, string action);

    }



    public class CalculatorService : ICalculator
    {

        // @"C:\test\write\drive.txt";



        public string receiver(string stuff, string action)
        {
            //WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string path = @"C:\Users\Shahbaaz\efb\in\";
            path += action;
            path += ".txt";
            Away a = new Away();
            a.content = action;
            ServiceController myService = new ServiceController("WCFWindowsServiceSample");
            myService.ExecuteCommand((int)MyCustomCommands.FileAccess);
            File.WriteAllText(path, stuff);
            int dwStartTime = System.Environment.TickCount;

            a.statusManager = false;
            while (1 != 0)
            {
                if (a.statusManager == true)
                {
                    a.statusManager = false;
                    //var result = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(a.sendManager));
                    //return (Encoding.ASCII.GetString(result.ToArray()));

                    return a.sendManager;
                }
                else
                    continue;
                //return("oops");
            }

        }



    }

    public class CalculatorWindowsService : ServiceBase
    {
        public ServiceHost serviceHost = null;
        public CalculatorWindowsService()
        {
            // Name the Windows Service
            ServiceName = "WCFWindowsServiceSample";
        }

        public static void Main()
        {
            ServiceBase.Run(new CalculatorWindowsService());
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }
            Away a = new Away();
            a.statusManager = false; 

            serviceHost = new ServiceHost(typeof(CalculatorService));
            serviceHost.Open();

            FileSystemWatcher Watcher;
            string pathToFolder = @"C:\Users\Shahbaaz\efb\out";
            Watcher = new FileSystemWatcher { Path = pathToFolder, IncludeSubdirectories = false, Filter = "*.*" };
            Watcher.Changed += new FileSystemEventHandler(FileWasChanged);
            Watcher.EnableRaisingEvents = true;



        }
        public void FileWasChanged(object source, FileSystemEventArgs e)
        {
            int dwStartTime = System.Environment.TickCount;

            while (true)
            {

                if (System.Environment.TickCount - dwStartTime > 20) break;

            }



            FileInfo file = new FileInfo(e.FullPath);
            string nom = file.Name;
            int f = nom.IndexOf(".");
            nom = nom.Substring(0, f);
            Away a = new Away();
            try
            {

                if (nom == a.content)
                {
                    a.sendManager = File.ReadAllText(e.FullPath);
                    a.statusManager = true;
                }
            }
            catch (IOException err)
            {
                string path = @"C:\test\errors\log.txt";
                File.AppendAllText(path, err.ToString());

            }
            /*string path = @"C:\test\read\send.txt";

            if (File.Exists(path))
            {
                System.IO.File.WriteAllText(path, "Response from send.txt at " + DateTime.Now.ToString("HH:mm:ss tt"));
                // System.IO.File.AppendAllText(path, "File Created from WCF at " + DateTime.Now.ToString("HH:mm:ss tt") + Environment.NewLine);

            }*/
        }
        protected override void OnCustomCommand(int command)
        {
            switch (command)
            {
                case (int)MyCustomCommands.FileAccess:
                    makeFiles();

                    break;
                case (int)MyCustomCommands.Another:

                    break;
                default:
                    break;
            }
        }

        public void makeFiles()
        {
            Away a = new Away();
            string path = @"C:\test\watched\";
            path += a.content;
            //Random r = new Random();
            // path += r.Next(1,8);
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            path += ts.TotalMilliseconds;
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Created");
                }
            }
        }
        public void writeToDrive()
        {
            Away b = new Away();

            string anotherPath = @"C:\test\read\send.txt";
            File.WriteAllText(anotherPath, "The message was " + b.content + ". Response created at " + DateTime.Now.ToString("HH:mm:ss tt") + Environment.NewLine);
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }

    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "WCFWindowsServiceSample";
            Installers.Add(process);
            Installers.Add(service);

           
            this.AfterInstall += new InstallEventHandler(ServiceInstaller_AfterInstall);
                

        }
        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController("WCFWindowsServiceSample"))
            {
                sc.Start();
            }
        }
    }
}
