using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using WPLovefilm.Models;

namespace WPLovefilm.Service
{
    public sealed class Format
    {
        private static readonly Format instance = new Format();

        public static Format Instance
        {
            get
            {
                return instance;
            }
        }

        private Format()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists("DiscFormats.xml"))
                    {
                        //first run, setup the defaults
                        CreateFormatList();
                    }
                    else
                    {
                        //There's already a file, load it and deserialize
                        using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("DiscFormats.xml", FileMode.Open, store))
                        {
                            XmlSerializer s = new XmlSerializer(typeof(List<LFFormat>));

                            formats = (List<LFFormat>)s.Deserialize(fs);
                        }
                    }
                }
            }
            //If these occur we won't retrieve user prefs. They should never happen!
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (IsolatedStorageException) { }
        }

        private List<LFFormat> formats;

        public List<LFFormat> GetFormatList()
        {
            return formats;
        }

        /// <summary>
        /// Retrieve the list of formats selected by the user
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetFormatString(LFFormatType type)
        {
            string formatString = string.Empty;

            foreach (LFFormat f in formats)
            {
                if (f.Type == type && f.Active == true)
                {
                    if (string.IsNullOrEmpty(formatString))
                    {
                        formatString += f.Name;
                    }
                    else
                    {
                        formatString += " OR " + f.Name;
                    }
                }
            }

            return formatString;
        }


        /// <summary>
        /// Set/Store format list in IsoStore
        /// </summary>
        /// <param name="formatList"></param>
        public void SetFormats(List<LFFormat> formatList)
        {
            formats = formatList;

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("DiscFormats.xml", FileMode.Create, store))
                    {
                        XmlSerializer s = new XmlSerializer(typeof(List<LFFormat>));

                        s.Serialize(fs, formats);
                    }
                }
            }
            //There really is nothing we can do here, but it's not the end of the world so we'll just swallow it, gulp!
            catch (IsolatedStorageException) { }
        }

        /// <summary>
        /// First run, create the initial format list
        /// </summary>
        private void CreateFormatList()
        {
            List<LFFormat> newFormatList = new List<LFFormat>();

            newFormatList.Add(new LFFormat("DVD", true, LFFormatType.Film));
            newFormatList.Add(new LFFormat("Blu-ray", true, LFFormatType.Film));
            newFormatList.Add(new LFFormat("HD-DVD", true, LFFormatType.Film));

            newFormatList.Add(new LFFormat("DS", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("GameCube", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("PS2", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("PS3", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("PSP", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("Wii", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("Xbox", true, LFFormatType.Game));
            newFormatList.Add(new LFFormat("Xbox 360", true, LFFormatType.Game));

            SetFormats(newFormatList);
        }

    }
}
