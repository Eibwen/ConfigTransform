using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Fclp;
using Microsoft.Web.XmlTransform;

namespace ConfigTransform
{
    public class ApplicationArguments
    {
        public string ConfigFile { get; set; }
        public string TransformFile { get; set; }
        public string ResultFile { get; set; }
        public List<string> AppSetting { get; set; }
    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            var transformArgs = new FluentCommandLineParser<ApplicationArguments>();


            transformArgs
                .Setup<string>(x => x.ConfigFile)
                .As(char.Parse("c"),"ConfigFile")
                .Required();

            transformArgs
                .Setup<string>(x => x.TransformFile)
                .As(char.Parse("t"),"TransformFile")
                .Required();

            transformArgs
                .Setup<string>(x => x.ResultFile)
                .As(char.Parse("r"),"ResultFile")
                .Required();

            IDictionary<string,string> data = new Dictionary<string, string>();
            transformArgs
                .Setup<List<string>>(x => x.AppSetting)
                .As(char.Parse("p"), "AppSetting")
                .Callback(x => x.ForEach(i =>
                {
                    var splitData = i.Split(char.Parse("|"));
                    data.Add(splitData[0],splitData[1]);
                }));

            var result = transformArgs.Parse(args);

            if (result.HasErrors)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error);
                }

                Environment.Exit(1);
            }
            ApplicationArguments final = transformArgs.Object;

//            args = new[]
//            {
//                @"c:\vswip\ConfigTransform\ConfigTransform\test.config",
//                @"c:\vswip\ConfigTransform\ConfigTransform\transform.config",
//                @"c:\vswip\ConfigTransform\ConfigTransform\brandnew.config",
//            };
//
//            if (args.Length != 3)
//            {
//                Console.WriteLine("Wrong number of arguments");
//                Console.WriteLine("WebConfigTransformer ConfigFilename TransformFilename ResultFilename");
//                Environment.Exit(1);
//            }
//            if (!File.Exists(args[0]) && !File.Exists(args[1]))
//            {
//                Console.WriteLine("The config or transform file do not exist!");
//                Environment.Exit(2);
//            }
            Stream parameters = new MemoryStream(GetParams(data));
//            var rootPath = @"C:\inetpub\wwwroot\sif\uship.web";
//            var host = "localhost";
//            Stream parameters = new MemoryStream(GetParams(new Dictionary<string, string>
//            {
//                {
//                    "ApiUserName", "Tom"
//                },
//                {
//                    "RSSDirectory", rootPath + @"\rss\"
//                },
//                {
//                    "SitePath", rootPath + @" \"
//                },
//                {
//                    "UploadPath", rootPath + @" \static"
//                },
//                {
//                    "GeoFile", rootPath + @" \Config\GeoIP.dat"
//                },
//                {
//                    "PathLogFiles", rootPath + @" \WebServices\log\"
//                },
//                {
//                    "PathStaticFiles", rootPath + @" \"
//                },
//                {
//                    "SecureURL", string.Format("https://{0}/" , host)
//                },
//                {
//                    "SiteURL", string.Format("http://{0}/" , host)
//                }
//            }));

            using (var doc = new XmlTransformableDocument())
            {
                doc.Load(final.ConfigFile); //load base doc

                

                using (var tranform = new XmlTransformation(final.TransformFile))
                {
                    tranform.Apply(doc);

                    if (!data.Any())
                    {
                        if (tranform.Apply(doc))
                        {
                            doc.Save(final.ResultFile);
                        }   
                    }
                }

                if(data.Any())
                {
                    using (var tranform = new XmlTransformation(parameters, new BogusLogger()))
                    {
                        if (tranform.Apply(doc))
                        {
                            doc.Save(final.ResultFile);
                        }
                    }
                }
            }

            }

        private static byte[] GetParams(IDictionary<string, string> keyValues)
        {
            //            var data = "<configuration xmlns:xdt=\"http://schemas.microsoft.com/XML-Document-Transform\"><appSettings><add key=\"ApiUserName\" value=\"Tom\" xdt:Transform=\"SetAttributes\" xdt:Locator=\"Match(key)\" /></appSettings></configuration>";
            //
            //            return Encoding.UTF8.GetBytes(data.ToCharArray());

            XNamespace xdt = "http://schemas.microsoft.com/XML-Document-Transform";
            var _customers =
                new XElement("configuration", new XAttribute(XNamespace.Xmlns + "xdt", xdt),
                    new XElement("appSettings",
                        from keyValue in keyValues
                        select new XElement("add",
                            new XAttribute("key", keyValue.Key),
                            new XAttribute("value", keyValue.Value),
                            new XAttribute(xdt + "Transform", "SetAttributes"),
                            new XAttribute(xdt + "Locator", "Match(key)")
                            )));

            return Encoding.UTF8.GetBytes(new XDocument(_customers).ToString().ToCharArray());
        }
    }

    public class BogusLogger : IXmlTransformationLogger
    {
        public void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void LogErrorFromException(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file)
        {
            Console.WriteLine(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            Console.WriteLine(ex.ToString());
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            Console.WriteLine(message);
        }
    }
}