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
namespace eFacebook.Mediator.Source
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
    [ServiceContract(Namespace = "http://eFacebook.Mediator.Source")]
    public interface Mediator
    {

        [OperationContract]
        //[WebGet(UriTemplate = "/form?content={stuff}&name={action}", ResponseFormat = WebMessageFormat.Json)]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/form")]
        string receiver(string stuff, string action);

    }



    public class FocalService : Mediator
    {

        public string receiver(string stuff, string action)
        {
            
            string path = @"C:\Users\Shahbaaz\efb\in\";
            path += action;
            path += ".txt";
            Away a = new Away();
            a.content = action;
            ServiceController myService = new ServiceController("eFacebookMediatorService");
            myService.ExecuteCommand((int)MyCustomCommands.FileAccess);
            File.WriteAllText(path, stuff);
            int dwStartTime = System.Environment.TickCount;

            a.statusManager = false;
            while (1 != 0)
            {
                if (a.statusManager == true)
                {
                    a.statusManager = false;
                    return a.sendManager;
                }
                else
                    continue;
               
            }

        }



    }

    public class FocalWindowsService : ServiceBase
    {
        public ServiceHost serviceHost = null;
        public FocalWindowsService()
        {
            // Name the Windows Service
            ServiceName = "eFacebookMediatorService";
        }

        public static void Main()
        {
            ServiceBase.Run(new FocalWindowsService());
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

            serviceHost = new ServiceHost(typeof(FocalService));
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
            service.ServiceName = "eFacebookMediatorService";
            Installers.Add(process);
            Installers.Add(service);

           
            this.AfterInstall += new InstallEventHandler(ServiceInstaller_AfterInstall);
                

        }
        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController("eFacebookMediatorService"))
            {
                sc.Start();
            }
        }
    }
}
