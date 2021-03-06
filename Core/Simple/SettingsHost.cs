﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OpenWealth.Simple
{
    /// <summary>
    /// Реализация ISettingsHost для храннения настроек в xml файле OpenWealth.xml 
    /// </summary>
    public class SettingsHost : ISettingsHost, IPlugin
    {
        private readonly static ILog l = Core.GetLogger(typeof(SettingsHost).FullName);

        public SettingsHost()
        {
            l.Debug("SettingsHost()");
            Core.SetGlobal("SettingsHost", this);
            filename = System.IO.Path.Combine(Core.GetGlobal("AppPath") as string, "OpenWealth.xml");

            Load();
        }

        public void Init()
        {
            //Load();
        }
        public void Stop()
        {
            //Load();
        }

        static System.Threading.ReaderWriterLock Lock = new System.Threading.ReaderWriterLock();
        static SerializableDictionary<string, string> settings;
        static string filename;

        void Save()
        {
            try
            {
                l.Debug("Save " + filename);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                TextWriter writer = new StreamWriter(filename);
                serializer.Serialize(writer, settings);
                writer.Close();
            }
            catch (Exception e)
            {
                l.Info("Save() Exception " + e.ToString());
            }
        }

        void Load()
        {
            l.Debug("Load " + filename);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                FileStream fs = new FileStream(filename, FileMode.Open);
                settings = (SerializableDictionary<string, string>)serializer.Deserialize(fs);
            }
            catch (Exception e)
            {
                l.Info("Load() Exception " + e.ToString());
                settings = new SerializableDictionary<string, string>();
            }
        }


        public bool ContainsKey(string key)
        {
            Lock.AcquireReaderLock(1000);
            try
            {
                return settings.ContainsKey(key);
            }
            finally
            {
                Lock.ReleaseReaderLock();
            }
        }

        public string Get(string key, string defaultValue)
        {
            l.Debug("Get(" + key);
            Lock.AcquireReaderLock(1000);
            try
            {
                if (settings.ContainsKey(key))
                {
                    return settings[key];
                }
                else
                    return defaultValue;
            }
            finally
            {
                Lock.ReleaseReaderLock();
            }
        }

        public void Set(string key, string value)
        {
            l.Debug("Set(" + key + " ," + value + ");");
            Lock.AcquireWriterLock(1000);
            try
            {
                if (settings.ContainsKey(key))
                {
                    settings[key] = value;
                }
                else
                    settings.Add(key, value);
                Save();
            }
            finally
            {
                Lock.ReleaseWriterLock();
            }
        }

        public void DeleteKey(string key)
        {
            Lock.AcquireWriterLock(1000);
            try
            {
                settings.Remove(key);
                Save();
            }
            finally
            {
                Lock.ReleaseWriterLock();
            }
        }

        public bool Get(string key, bool defaultValue)
        {
            return bool.Parse(Get(key, defaultValue.ToString()));
        }

        public void Set(string key, bool value)
        {
            Set(key, value.ToString());
        }

        public int Get(string key, int defaultValue)
        {
            return int.Parse(Get(key, defaultValue.ToString()));
        }

        public void Set(string key, int value)
        {
            Set(key, value.ToString());
        }

        public double Get(string key, double defaultValue)
        {
            return double.Parse(Get(key, defaultValue.ToString()));
        }

        public void Set(string key, double value)
        {
            Set(key, value.ToString());
        }

        public DateTime Get(string key, DateTime defaultValue)
        {
            return DateTime.ParseExact(Get(key, defaultValue.ToString("yyyy.MM.dd hh:mm:ss.zzz")), "yyyy.MM.dd hh:mm:ss.zzz", null);
        }

        public void Set(string key, DateTime value)
        {
            Set(key, value.ToString("yyyy.MM.dd hh:mm:ss.zzz"));
        }
    }
}