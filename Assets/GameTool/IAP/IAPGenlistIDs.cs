using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameToolSample.IAP.Editor
{
    public class IAPGenlistIDs : MonoBehaviour
    {
        [SerializeField] List<IAPProduct> iAPProducts = new List<IAPProduct>();
        [SerializeField] string countryID;
        [SerializeField] string jsonTemplate;

        Dictionary<string, string> pricingIDs = new Dictionary<string, string>()
        {
            { "0.99", "4635535399119798933" }, { "1.99", "4634523613135097162" }, { "2.99", "4636939043325652542" },
            { "3.99", "4638402480604359484" }, { "4.99", "4635119002925525769" }, { "5.99", "4634797242452526582" },
            { "6.99", "4637004186805771673" }, { "7.99", "4635841426345637299" }, { "8.99", "4637390031200367292" },
            { "9.99", "4634740126348679829" }, { "10.99", "4637595199372171475" }, { "11.99", "4638115163503485796" },
            { "12.99", "4637818585083793948" }, { "13.99", "4637299582041381126" }, { "14.99", "4637528628245699831" },
            { "15.99", "4638043674625088392" }, { "16.99", "4638678226890916449" }, { "17.99", "4638313887305189415" },
            { "18.99", "4635740650868269222" }, { "19.99", "4636286131341488387" }, { "23.99", "4634207065253711451" },
            { "39.99", "4638458418744564053" }, { "49.99", "4636593012161671070" }
        };

        string fileName;
        private void Start()
        {
            ExportCSVFile();
        }
        [ContextMenu("ExportCSVFile")]
        public void ExportCSVFile()
        {
            fileName = Application.dataPath + "/iap.csv";

            if (iAPProducts.Count > 0)
            {
                TextWriter tw = new StreamWriter(fileName, false);
                tw.WriteLine(
                    "Product ID,Published State,Purchase Type,Auto Translate,Locale; Title; Description,Auto Fill Prices,Price,Pricing Template ID");
                // tw.Close();

                // tw = new StreamWriter(fileName, false);


                for (int i = 0; i < iAPProducts.Count; i++)
                {
                    tw.WriteLine(string.Format("{0},published,managed_by_android,FALSE,en-US;{1};{2},FALSE, ,{3}",
                        iAPProducts[i].id, iAPProducts[i].name, iAPProducts[i].des, pricingIDs[iAPProducts[i].defaultPrice]));

                    for (int j = 0; j < iAPProducts[i].countryIds.Count; j++)
                    {
                        tw.WriteLine(string.Format("{0}.{1},published,managed_by_android,FALSE,en-US; {2} {3};{4},FALSE, ,{5}",
                            iAPProducts[i].countryIds[j], iAPProducts[i].id, iAPProducts[i].countryIds[j].ToUpper(), iAPProducts[i].name,
                            iAPProducts[i].des, pricingIDs[iAPProducts[i].countryPrices[j]]));
                    }

                }
                tw.Close();
            }
        }
#if UNITY_EDITOR
        [ContextMenu("Copy Country ID")]
        public void CopyCountryID()
        {
            string newJson = jsonTemplate.Replace("\":\"", string.Format("\":\"{0}.", countryID));
            UnityEditor.EditorGUIUtility.systemCopyBuffer = newJson;
        }
#endif
    }
    [Serializable]
    public class IAPProduct
    {
        public string name;
        public string des;
        public string id;
        public string defaultPrice;
        public List<string> countryIds = new List<string>();
        public List<string> countryPrices = new List<string>();
    }
}