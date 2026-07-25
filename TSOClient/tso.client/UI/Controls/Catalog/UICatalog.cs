using System;
using System.Collections.Generic;
using System.Linq;
using FSO.Client.UI.Framework;
using FSO.Content;
using FSO.LotView.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Files.Formats.IFF.Chunks;
using FSO.Client.UI.Panels.LotControls;
using FSO.SimAntics;
using FSO.SimAntics.Model;
using FSO.Content.Interfaces;
using FSO.Client.UI.Panels;
using System.Text.RegularExpressions;

namespace FSO.Client.UI.Controls.Catalog
{
    public class UICatalog : UIContainer
    {
        private int _Page;
        public int Page { get => _Page; }
        private int _Budget;
        public UILotControl LotControl;
        public VM ActiveVM { get
            {
                return LotControl?.vm;
            }
        }
        public int Budget
        {
            get { return _Budget; }
            set {
                if (value != _Budget)
                {
                    if (CatalogItems != null)
                    {
                        for (int i = 0; i < CatalogItems.Length; i++)
                        {
                            CatalogItems[i].SetDisabled(CatalogItems[i].Info.Item.Price > value);
                        }
                    }
                    _Budget = value;
                }
            }
        }
        private static List<UICatalogElement>[] _Catalog;
        public event CatalogSelectionChangeDelegate OnSelectionChange;

        public static List<UICatalogElement>[] Catalog {
            get
            {
                if (_Catalog != null) return _Catalog;
                else
                {
                    //load and build catalog
                    _Catalog = new List<UICatalogElement>[30];
                    for (int i = 0; i < 30; i++) _Catalog[i] = new List<UICatalogElement>();

                    foreach (var obj in Content.Content.Get().WorldCatalog.All())
                    {
                        _Catalog[obj.Category].Add(new UICatalogElement()
                        {
                            Item = obj
                        });
                    }

                    AddWallpapers();
                    AddFloors();

                    for (int i = 0; i < 30; i++) _Catalog[i].Sort(new CatalogSorter());

                    AddWallStyles();
                    AddRoofs();
                    AddTerrainTools();

                    return _Catalog;
                }
            }
        }

        private static void AddWallpapers()
        {
            var res = new UICatalogWallpaperResProvider();

            var walls = Content.Content.Get().WorldWalls.List();

            for (int i = 0; i < walls.Count; i++)
            {
                var wall = (WallReference)walls[i];
                _Catalog[8].Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = wall.Name,
                        Category = 8,
                        Price = (uint)wall.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPainter),
                        ResID = wall.ID,
                        Res = res,
                        Parameters = new List<int> { (int)wall.ID } //pattern
                    }
                });
            }
        }

        private static void AddFloors()
        {
            var res = new UICatalogFloorResProvider();

            var floors = Content.Content.Get().WorldFloors.List();

            for (int i = 0; i < floors.Count; i++)
            {
                var floor = (FloorReference)floors[i];
                sbyte category = (sbyte)((floor.ID >= 65534) ? 5 : 9);
                _Catalog[category].Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = floor.Name,
                        Category = category,
                        Price = (uint)floor.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIFloorPainter),
                        ResID = floor.ID,
                        Res = res,
                        Parameters = new List<int> { (int)floor.ID } //pattern
                    }
                });
            }
        }

        private static void AddRoofs()
        {
            var res = new UICatalogRoofResProvider();

            var total = Content.Content.Get().WorldRoofs.Count;

            for (int i = 0; i < total; i++)
            {
                sbyte category = 6;
                _Catalog[category].Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = "",
                        Category = category,
                        Price = 0,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIRoofer),
                        ResID = (uint)i,
                        Res = res,
                        Parameters = new List<int> { i } //pattern
                    }
                });
            }
        }

        private static void AddWallStyles()
        {
            var res = new UICatalogWallResProvider();

            for (int i = 0; i < WallStyleIDs.Length; i++)
            {
                var walls = Content.Content.Get().WorldWalls;
                var style = walls.GetWallStyle((ulong)WallStyleIDs[i]);
                _Catalog[7].Insert(0, new UICatalogElement
                {
                    Item = new ObjectCatalogItem()
                    {
                        Name = style.Name,
                        Category = 7,
                        Price = (uint)style.Price,
                    },
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPlacer),
                        ResID = (ulong)WallStyleIDs[i],
                        Res = res,
                        Parameters = new List<int> { WallStylePatterns[i], WallStyleIDs[i] } //pattern, style
                    }
                });
            }
        }

        private static void AddTerrainTools()
        {
            var res = new UICatalogWallResProvider();

            _Catalog[10].Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Raise/Lower Terrain",
                    Category = 7,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UITerrainRaiser),
                    ResID = 0,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });

            _Catalog[10].Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Flatten Terrain",
                    Category = 7,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UITerrainFlatten),
                    ResID = 1,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });

            _Catalog[10].Insert(0, new UICatalogElement
            {
                Item = new ObjectCatalogItem()
                {
                    Name = "Grass Tool",
                    Category = 7,
                    Price = 1,
                },
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UIGrassPaint),
                    ResID = 2,
                    Res = new UICatalogTerrainResProvider(),
                    Parameters = new List<int> { }
                }
            });
        }

        public static short[] WallStyleIDs =
        {
            0x1, //wall
            0x2, //picket fence
            0xD, //iron fence
            0xC, //privacy fence
            0xE //banisters
        };

        public static short[] WallStylePatterns =
        {
            0, //wall
            248, //picket fence
            250, //iron fence
            249, //privacy fence
            251, //banisters
        };

        public int PageSize { get; set; }
        public List<UICatalogElement> Selected;
        public List<UICatalogElement> Filtered;
        private UICatalogItem[] CatalogItems;
        private Dictionary<uint, Texture2D> IconCache;

        private string SearchTerm;

        public UICatalog(int pageSize)
        {
            IconCache = new Dictionary<uint, Texture2D>();
            PageSize = pageSize;
        }

        public void SetActive(int selection, bool active) {
            int index = selection - _Page * PageSize;
            if (index >= 0 && index < CatalogItems.Length) CatalogItems[index].SetActive(active);
        }

        public void SetCategory(List<UICatalogElement> select) {
            Selected = select;
            FilterSelected();
            SetPage(0);
        }

        private void AddMatchScore(string name, Regex search, ref int score)
        {
            if (name == null) return;

            var allMatches = search.Matches(name);

            foreach (Match match in allMatches)
            {
                int matchScore = 4;

                if (match.Index == 0 || char.IsWhiteSpace(name[match.Index - 1]))
                {
                    matchScore *= 2;
                }

                if (match.Index + match.Value.Length == name.Length || char.IsWhiteSpace(name[match.Index + match.Value.Length]))
                {
                    matchScore *= 3;
                }

                score += matchScore;
            }
        }

        private int GetScore(UICatalogElement elem)
        {
            ref var item = ref elem.Item;

            string name = item.Name.ToLowerInvariant();
            string catalogName = item.CatalogName?.ToLowerInvariant();
            string tags = item.Tags?.ToLowerInvariant();

            string[] termWords = SearchTerm.ToLowerInvariant().Split(' ');

            int score = 0;

            foreach (string word in termWords)
            {
                var search = new Regex(".*" + Regex.Escape(word) + ".*");

                AddMatchScore(name, search, ref score);
                AddMatchScore(catalogName, search, ref score);
                AddMatchScore(tags, search, ref score);
            }

            return score;
        }

        public void FilterSelected()
        {
            if (SearchTerm != null && Selected != null)
            {
                Filtered = Selected
                    .Select(elem => new Tuple<UICatalogElement, int>(elem, GetScore(elem)))
                    .Where(tuple => tuple.Item2 > 0)
                    .OrderByDescending(tuple => tuple.Item2)
                    .Select(tuple => tuple.Item1)
                    .ToList();
            }
            else
            {
                Filtered = Selected;
            }
        }

        public void SetSearchTerm(string term)
        {
            if (term == "") term = null;

            if (SearchTerm != term)
            {
                SearchTerm = term;
                FilterSelected();
                SetPage(0);
            }
        }

        public int TotalPages()
        {
            if (Filtered == null) return 0;
            return ((Filtered.Count - 1) / PageSize) + 1;
        }

        public int GetPage()
        {
            return _Page;
        }

        public void SetPage(int page) {
            if (CatalogItems != null)
            {
                for (int i = 0; i < CatalogItems.Length; i++)
                {
                    this.Remove(CatalogItems[i]);
                }
            }

            int index = page*PageSize;
            if (Filtered == null) return;
            CatalogItems = new UICatalogItem[Math.Min(PageSize, Math.Max(Filtered.Count - index, 0))];
            int halfPage = PageSize / 2;
            
            for (int i=0; i<CatalogItems.Length; i++)
            {
                var sel = Filtered[index++];
                var elem = new UICatalogItem(false);
                if (sel.Item.GUID == uint.MaxValue) elem.Visible = false;
                elem.Index = index-1;
                elem.Info = sel;
                elem.Info.CalcPrice = (int)elem.Info.Item.Price;

                if (elem.Info.Item.GUID != 0)
                {
                    var price = (int)elem.Info.Item.Price;
                    var dcPercent = VMBuildableAreaInfo.GetDiscountFor(elem.Info.Item, ActiveVM);
                    var finalPrice = (price * (100 - dcPercent)) / 100;
                    if (LotControl.ObjectHolder.DonateMode) finalPrice -= (finalPrice * 2) / 3;
                    elem.Info.CalcPrice = finalPrice;
                }

                elem.Icon = (elem.Info.Special?.Res != null)?elem.Info.Special.Res.GetIcon(elem.Info.Special.ResID):GetObjIcon(elem.Info.Item.GUID);
                elem.Tooltip = (elem.Info.CalcPrice > 0)?("$"+elem.Info.CalcPrice.ToString()):null;
                elem.X = (i % halfPage) * 45 + 2;
                elem.Y = (i / halfPage) * 45 + 2;
                elem.OnMouseEvent += new ButtonClickDelegate(InnerSelect);
                elem.SetDisabled(elem.Info.CalcPrice > Budget);
                CatalogItems[i] = elem;
                this.Add(elem);
            }
            _Page = page;
        }

        void InnerSelect(UIElement button)
        {
            if (OnSelectionChange != null) OnSelectionChange(((UICatalogItem)button).Index);
        }

        public Texture2D GetObjIcon(uint GUID)
        {
            if (!IconCache.ContainsKey(GUID)) {
                var obj = Content.Content.Get().WorldObjects.Get(GUID);
                if (obj == null)
                {
                    IconCache[GUID] = null;
                    return null;
                }
                var bmp = obj.Resource.Get<BMP>(obj.OBJ.CatalogStringsID);
                if (bmp != null) IconCache[GUID] = bmp.GetTexture(GameFacade.GraphicsDevice);
                else
                {
                    var drawn = RenderObjIcon(obj);
                    if (drawn == null) return null; //no world yet - retry on a later page build
                    IconCache[GUID] = drawn;
                }
            }
            return IconCache[GUID];
        }

        //a shipped catalog icon is two 37x37 snapshots of the object side by side - the first on
        //a dark background, the second on a light one for hover. Matching that layout exactly
        //lets the drawn icons take the same path through UICatalogItem as the real ones.
        private const int ICON_FRAME = 37;
        private static readonly Color IconBackNormal = new Color(56, 88, 120);
        private static readonly Color IconBackHover = new Color(184, 212, 240);

        /// <summary>
        /// Builds a catalog icon from the object's own sprites, for objects the game shipped
        /// without a catalog BMP (the Hygeia-O-Matic Toilet and friends). Snapshots the object
        /// at the same angle and zoom the shipped icons use, then lays it out as the usual
        /// normal + hover frame pair.
        /// </summary>
        private Texture2D RenderObjIcon(GameObject obj)
        {
            var world = LotControl?.World;
            if (world == null || obj.OBJ.BaseGraphicID == 0) return null;
            Texture2D thumb = null;
            try
            {
                var comp = new ObjectComponent(obj);
                thumb = world.GetObjectThumb(new ObjectComponent[] { comp }, new Vector3[] { Vector3.Zero }, GameFacade.GraphicsDevice);
                if (thumb == null || thumb.Width == 0 || thumb.Height == 0) return null;
                var src = new Color[thumb.Width * thumb.Height];
                thumb.GetData(src);

                var scale = Math.Min(ICON_FRAME / (float)thumb.Width, ICON_FRAME / (float)thumb.Height);
                var dw = Math.Max(1, Math.Min(ICON_FRAME, (int)(thumb.Width * scale)));
                var dh = Math.Max(1, Math.Min(ICON_FRAME, (int)(thumb.Height * scale)));
                var ox = (ICON_FRAME - dw) / 2;
                var oy = (ICON_FRAME - dh) / 2;

                var width = ICON_FRAME * 2;
                var dest = new Color[width * ICON_FRAME];
                for (int y = 0; y < ICON_FRAME; y++)
                {
                    for (int x = 0; x < ICON_FRAME; x++)
                    {
                        dest[y * width + x] = IconBackNormal;
                        dest[y * width + ICON_FRAME + x] = IconBackHover;
                    }
                }

                //box filter down to icon size. world sprites are premultiplied, so the channels
                //average independently and composite as a plain over.
                for (int y = 0; y < dh; y++)
                {
                    var sy0 = y * thumb.Height / dh;
                    var sy1 = Math.Max(sy0 + 1, (y + 1) * thumb.Height / dh);
                    for (int x = 0; x < dw; x++)
                    {
                        var sx0 = x * thumb.Width / dw;
                        var sx1 = Math.Max(sx0 + 1, (x + 1) * thumb.Width / dw);
                        int r = 0, g = 0, b = 0, a = 0, n = 0;
                        for (int sy = sy0; sy < sy1; sy++)
                        {
                            for (int sx = sx0; sx < sx1; sx++)
                            {
                                var p = src[sy * thumb.Width + sx];
                                r += p.R; g += p.G; b += p.B; a += p.A; n++;
                            }
                        }
                        if (n == 0 || a == 0) continue;
                        r /= n; g /= n; b /= n; a /= n;
                        var i = (y + oy) * width + (x + ox);
                        dest[i] = Over(r, g, b, a, IconBackNormal);
                        dest[i + ICON_FRAME] = Over(r, g, b, a, IconBackHover);
                    }
                }

                var tex = new Texture2D(GameFacade.GraphicsDevice, width, ICON_FRAME);
                tex.SetData(dest);
                return tex;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (thumb != null && !thumb.IsDisposed) thumb.Dispose();
            }
        }

        private static Color Over(int r, int g, int b, int a, Color back)
        {
            var inv = 255 - a;
            return new Color(
                Math.Min(255, r + back.R * inv / 255),
                Math.Min(255, g + back.G * inv / 255),
                Math.Min(255, b + back.B * inv / 255));
        }

        private class CatalogSorter : IComparer<UICatalogElement>
        {
            #region IComparer<UICatalogElement> Members

            public int Compare(UICatalogElement x, UICatalogElement y)
            {
                if (x.Item.Price > y.Item.Price) return 1;
                else if (x.Item.Price < y.Item.Price) return -1;
                else return 0;
            }

            #endregion
        }
    }

    public delegate void CatalogSelectionChangeDelegate(int selection);

    public struct UICatalogElement {
        public ObjectCatalogItem Item;
        public int CalcPrice;
        public UISpecialCatalogElement Special;
        public int? Count;
        public List<int> Attributes;
        public object Tag;
    }

    public class UISpecialCatalogElement
    {
        public Type Control;
        public ulong ResID;
        public UICatalogResProvider Res;
        public List<int> Parameters;
    }
}
