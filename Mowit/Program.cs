using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MowPlanning;
using SmhiWeather;

namespace Mowit
{
    class Program
    {
        private static MowitConfig Config { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER to start the Mowit service.");
            Console.ReadLine();

            string path = System.IO.Directory.GetCurrentDirectory();
            var serializer = new XmlSerializer(typeof(MowitConfig));

            TextReader textReader = new StreamReader(Path.Combine(path, "MowitSettings.xml"));
            Config = (MowitConfig)serializer.Deserialize(textReader);

            // When writing a file
            //TextWriter textWriter = new StreamWriter(Path.Combine(path, "MowitSettingsOut.xml"));
            //serializer.Serialize(textWriter, Config);
            //textWriter.Flush();

            Smhi.Init(Config.MowPlannerConfig.CoordLat, Config.MowPlannerConfig.CoordLon, new TimeSpan(1, 0, 0));
            EmailSender.Init(Config.EmailConfig);

            var systemTime = new SystemTime();
            var homeSensor = new TimeBasedHomeSensor(Config.MowPlannerConfig, systemTime);
            var powerSwitch = new UrlPowerSwitch(Config.MowPlannerConfig.PowerOnUrl, Config.MowPlannerConfig.PowerOffUrl);
            var weatherForecast = new WeatherForecast(Config.MowPlannerConfig.MaxHourlyThunderPercent, Config.MowPlannerConfig.MaxHourlyPrecipitaionMillimeter);
            var logger = new MowLogger();

            logger.LogItemWritten += Logger_LogItemWritten;

            var mowController = new MowController(Config.MowPlannerConfig, powerSwitch, weatherForecast, systemTime, homeSensor, logger);
            var task = mowController.StartAsync();
        }

        private static void Logger_LogItemWritten(object sender, EventArgs e)
        {
            var logger = sender as IMowLogger;
            int index = logger.LogItems.Count - 1;

            Console.WriteLine(logger.LogItems[index].Time.ToString("yyyy-MM-dd HH:mm") + " - " + logger.LogItems[index].Message);

            try
            {
                if (Config.EmailConfig.SendEmails)
                {
                    EmailSender.SendMail(logger.LogItems[index].Message, logger.LogItems[index].Time.ToString("yyyy-MM-dd HH:mm") + " - " + logger.LogItems[index].Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email exception: " + ex.Message);
            }
        }
    }
}
