using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class ProfileImporter
    {
        public static bool ImportProfile(Stream importStream)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                byte[] byteContents = new byte[importStream.Length];
                string stringContents;

                importStream.Read(byteContents, 0, (int)importStream.Length);
                stringContents = Encoding.UTF8.GetString(byteContents, 0, (int)importStream.Length);

                // Akward bug fix : Remove last character if it's a non-printing character
                for (int i = 0; i < 32; ++i)
                {
                    char currentCharacter = (char)i;

                    if (stringContents.EndsWith(currentCharacter.ToString()))
                    {
                        stringContents = stringContents.Substring(0, stringContents.Length - 1);
                        break;
                    }
                }

                document.LoadXml(stringContents);

                for (int i = 0; i < document.ChildNodes.Count; ++i)
                {
                    XmlNode database = document.ChildNodes.Item(i);

                    if (database.Name == "TrainingCenterDatabase")
                    {
                        for(int j = 0; j < database.ChildNodes.Count; ++j)
                        {
                            if (database.ChildNodes[j].Name == Constants.ExtensionsTCXString)
                            {
                                XmlNode extensionsNode = database.ChildNodes[j];

                                if (extensionsNode.ChildNodes.Count == 1 && extensionsNode.FirstChild.Name == Constants.ProfileTCXString)
                                {
                                    GarminProfileManager.Instance.Deserialize(extensionsNode.FirstChild);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (GarminFitnesXmlDeserializationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.ErroneousNode.OuterXml);
                return false;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }
    }
}
