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
            bool simulatedHomeSensor = false;

            foreach (string arg in args)
            {
                if (arg == "/configexample")
                {
                    TextWriter textWriter = new StreamWriter(Path.Combine(path, "MowitSettings.xml.example"));
                    serializer.Serialize(textWriter, MowitConfig.GetExampleConfig());
                    textWriter.Flush();

                    return;
                }

                if (arg == "/simulatedcontacthomesensor")
                {
                    simulatedHomeSensor = true;
                }
            }

            TextReader textReader = new StreamReader(Path.Combine(path, "MowitSettings.xml"));
            Config = (MowitConfig)serializer.Deserialize(textReader);

            Console.WriteLine("Press ENTER to start the Mowit service.");
            Console.ReadLine();

            EmailSender.Init(Config.EmailConfig);

            var systemTime = new SystemTime();
            var powerSwitch = new UrlPowerSwitch(Config.MowControlConfig.PowerOnUrl, Config.MowControlConfig.PowerOffUrl);
            IHomeSensor homeSensor;

            if (simulatedHomeSensor)
            {
                homeSensor = new SimulatedContactHomeSensor(systemTime, Config.MowControlConfig.TimeIntervals.ToArray(), powerSwitch);
            }
            else
            {
                homeSensor = new TimeBasedHomeSensor(systemTime.Now, Config.MowControlConfig, powerSwitch, systemTime);
            }

            Smhi smhi = new Smhi(Config.MowControlConfig.CoordLat, Config.MowControlConfig.CoordLon, new TimeSpan(1, 0, 0));
            var weatherForecast = new WeatherForecast(smhi, Config.MowControlConfig.MaxHourlyThunderPercent, Config.MowControlConfig.MaxHourlyPrecipitaionMillimeter, Config.MowControlConfig.MaxRelativeHumidityPercent);

            var logger = new MowLogger();

            logger.LogItemWritten += Logger_LogItemWritten;

            var rainSensor = new SmhiRainSensor(systemTime, smhi);
            var mowController = new MowController(Config.MowControlConfig, powerSwitch, weatherForecast, systemTime, homeSensor, logger, rainSensor);
            var task = mowController.StartAsync();
            task.Wait();
        }

        private static void Logger_LogItemWritten(object sender, MowLoggerEventArgs e)
        {
            Console.WriteLine(e.Item.Time.ToString("yyyy-MM-dd HH:mm") + " - " + e.Item.Message);

            try
            {
                if (Config.EmailConfig.SendEmails)
                {
                    EmailSender.SendMail(e.Item.Message.Replace("\n", " ").Substring(0, 100), e.Item.Time.ToString("yyyy-MM-dd HH:mm") + " - " + e.Item.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email exception: " + ex.Message);
            }
        }
    }
}
