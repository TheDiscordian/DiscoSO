using FSO.Common;
using FSO.Content.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FSO.Content
{
    public class WorldObjectCatalog : IObjectCatalog
    {
        private static List<ObjectCatalogItem>[] ItemsByCategory;
        private static Dictionary<uint, ObjectCatalogItem> ItemsByGUID;
        private List<uint> UntradableGUIDs;

        public void Init(Content content, Dictionary<ulong, GameObjectCatalogEnrich> catalogEnrich)
        {
            //load and build catalog
            ItemsByGUID = new Dictionary<uint, ObjectCatalogItem>();
            ItemsByCategory = new List<ObjectCatalogItem>[30];
            UntradableGUIDs = new List<uint>();

            for (int i = 0; i < 30; i++) ItemsByCategory[i] = new List<ObjectCatalogItem>();

            var packingslip = new XmlDocument();

            packingslip.Load(content.GetPath("packingslips/catalog.xml"));
            var objectInfos = packingslip.GetElementsByTagName("P");

            foreach (XmlNode objectInfo in objectInfos)
            {
                sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                if (Category < 0) continue;
                ushort seasonStart, seasonEnd;
                ReadSeason(objectInfo, out seasonStart, out seasonEnd);
                var item = new ObjectCatalogItem()
                {
                    GUID = guid,
                    Category = Category,
                    Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                    Name = objectInfo.Attributes["n"].Value,
                    DisableLevel = Convert.ToByte(objectInfo.Attributes["r"]?.Value ?? "0"),
                    SeasonStart = seasonStart,
                    SeasonEnd = seasonEnd
                };
                ItemsByCategory[Category].Add(item);
                ItemsByGUID[guid] = item;
            }

            //load and build Content Objects into catalog
            if (File.Exists(Path.Combine(FSOEnvironment.ContentDir, "Objects/catalog_downloads.xml")))
            {
                var dpackingslip = new XmlDocument();

                dpackingslip.Load(Path.Combine(FSOEnvironment.ContentDir, "Objects/catalog_downloads.xml"));
                var downloadInfos = dpackingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in downloadInfos)
                {
                    sbyte dCategory = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint dguid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (dCategory < 0) continue;
                    catalogEnrich.TryGetValue(dguid, out var enrich);

                    ushort dSeasonStart, dSeasonEnd;
                    ReadSeason(objectInfo, out dSeasonStart, out dSeasonEnd);

                    var ditem = new ObjectCatalogItem()
                    {
                        GUID = dguid,
                        Category = dCategory,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value,
                        Tags = objectInfo.Attributes["t"]?.Value,
                        CatalogName = enrich?.CatalogName,
                        DisableLevel = Convert.ToByte(objectInfo.Attributes["r"]?.Value ?? "0"),
                        SeasonStart = dSeasonStart,
                        SeasonEnd = dSeasonEnd
                    };

                    if (ditem.DisableLevel > 1)
                    {
                        UntradableGUIDs.Add(dguid);
                    }

                    ItemsByCategory[dCategory].Add(ditem);
                    ItemsByGUID[dguid] = ditem;

                }
            }

            catalogEnrich.Clear();
        }

        //DiscoSO: everything stays loaded so prices, inventory and resale still resolve for a
        //seasonal object a player already owns. These two are what stocks the catalogue screen,
        //so this is where out-of-season items drop out. Checked per call rather than at load, so
        //a season turning over does not need a restart to take effect.
        public List<ObjectCatalogItem> All()
        {
            var now = DateTime.UtcNow;
            var result = new List<ObjectCatalogItem>();
            foreach (var cat in ItemsByCategory)
            {
                foreach (var item in cat)
                {
                    if (item.InSeason(now)) result.Add(item);
                }
            }
            return result;
        }

        public List<ObjectCatalogItem> GetItemsByCategory(sbyte category)
        {
            var now = DateTime.UtcNow;
            return ItemsByCategory[category].FindAll(item => item.InSeason(now));
        }

        //d="MMDD-MMDD" - the window this item is sold in. Absent or malformed = all year.
        private static void ReadSeason(XmlNode info, out ushort start, out ushort end)
        {
            start = 0; end = 0;
            var d = info.Attributes["d"]?.Value;
            if (string.IsNullOrEmpty(d)) return;
            var parts = d.Split('-');
            if (parts.Length != 2) return;
            if (!ushort.TryParse(parts[0].Trim(), out start) || !ushort.TryParse(parts[1].Trim(), out end))
            {
                start = 0; end = 0;
            }
        }

        public ObjectCatalogItem? GetItemByGUID(uint guid)
        {
            ObjectCatalogItem item;
            if (ItemsByGUID.TryGetValue(guid, out item))
                return item;
            else return null;
        }

        public List<uint> GetUntradableGUIDs()
        {
            return UntradableGUIDs;
        }
    }
}
