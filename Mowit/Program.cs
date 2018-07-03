using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MowPlanning;

namespace Mowit
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER to start the Mowit service.");
            Console.ReadLine();

            string path = System.IO.Directory.GetCurrentDirectory();
            var serializer = new XmlSerializer(typeof(MowitConfig));
            //TextWriter textWriter = new StreamWriter(Path.Combine(path, "MowSettings.xml"));
            //serializer.Serialize(textWriter, mowitConfig);

            TextReader textReader = new StreamReader(Path.Combine(path, "MowitSettings.xml"));
            var mowitConfig = (MowitConfig)serializer.Deserialize(textReader);

            EmailSender.Init(mowitConfig.EmailConfig);

            var systemTime = new SystemTime();
            var homeSensor = new TimeBasedHomeSensor(mowitConfig.MowPlannerConfig, systemTime);
            var powerSwitch = new UrlPowerSwitch(mowitConfig.MowPlannerConfig.PowerOnUrl, mowitConfig.MowPlannerConfig.PowerOffUrl);
            var weatherForecast = new WeatherForecast(mowitConfig.MowPlannerConfig.MaxHourlyThunderPercent, mowitConfig.MowPlannerConfig.MaxHourlyPrecipitaionMillimeter);
            var logger = new MowLogger();

            logger.LogItemWritten += Logger_LogItemWritten;

            var mowController = new MowController(mowitConfig.MowPlannerConfig, powerSwitch, weatherForecast, systemTime, homeSensor, logger);
            var task = mowController.StartAsync();
        }

        private static void Logger_LogItemWritten(object sender, EventArgs e)
        {
            var logger = sender as IMowLogger;
            int index = logger.LogItems.Count - 1;

            Console.WriteLine(logger.LogItems[index].Time.ToString("yyyy-MM-dd HH:mm") + " - " + logger.LogItems[index].Message);

            try
            {
                EmailSender.SendMail(logger.LogItems[index].Message, logger.LogItems[index].Time.ToString("yyyy-MM-dd HH:mm") + " - " + logger.LogItems[index].Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email exception: " + ex.Message);
            }
        }
    }
}
