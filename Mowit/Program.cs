using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MowControl;
using SmhiWeather;

// dotnet publish -c Release -r win10-x64

namespace Mowit
{
    class Program
    {
        private static MowitConfig Config { get; set; }

        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            var serializer = new XmlSerializer(typeof(MowitConfig));

            if (args.Length >= 1 && args[0].Contains("/configexample"))
            {
                TextWriter textWriter = new StreamWriter(Path.Combine(path, "MowitSettings.xml.example"));
                serializer.Serialize(textWriter, MowitConfig.GetExampleConfig());
                textWriter.Flush();

                return;
            }

            TextReader textReader = new StreamReader(Path.Combine(path, "MowitSettings.xml"));
            Config = (MowitConfig)serializer.Deserialize(textReader);

            Console.WriteLine("Press ENTER to start the Mowit service.");
            Console.ReadLine();

            EmailSender.Init(Config.EmailConfig);

            var systemTime = new SystemTime();
            var powerSwitch = new UrlPowerSwitch(Config.MowControlConfig.PowerOnUrl, Config.MowControlConfig.PowerOffUrl);
            var homeSensor = new TimeBasedHomeSensor(Config.MowControlConfig, powerSwitch, systemTime);

            Smhi smhi = new Smhi(Config.MowControlConfig.CoordLat, Config.MowControlConfig.CoordLon, new TimeSpan(1, 0, 0));
            var weatherForecast = new WeatherForecast(smhi, Config.MowControlConfig.MaxHourlyThunderPercent, Config.MowControlConfig.MaxHourlyPrecipitaionMillimeter, Config.MowControlConfig.MaxRelativeHumidityPercent);

            var logger = new MowLogger();

            logger.LogItemWritten += Logger_LogItemWritten;

            var mowController = new MowController(Config.MowControlConfig, powerSwitch, weatherForecast, systemTime, homeSensor, logger);
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
