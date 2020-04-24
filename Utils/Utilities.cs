
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Utils
{
    public class Utilities
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Utilities));

        public static string ReadFile(string pathFile)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathFile);
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return "";
            }
        }

        public static string ReadTemplateFile(string fileName)
        {
            try
            {
                string path = Path.Combine("EmailTemplates", fileName);
                return ReadFile(path);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return "";
            }
        }
    }
}
