using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Controls.Catalog;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using FSO.HIT;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Model.TSOPlatform;

namespace FSO.Client.UI.Panels
{
    //TODO: very similar to buy mode... maybe make a them both subclasses of a single abstract "purchase panel" class.

    public class UIBuildMode : UIAbstractCatalogPanel
    {
        public UIButton TerrainButton { get; set; }
        public UIButton WaterButton { get; set; }
        public UIButton WallButton { get; set; }
        public UIButton WallpaperButton { get; set; }
        public UIButton StairButton { get; set; }
        public UIButton FireplaceButton { get; set; }

        public UIButton PlantButton { get; set; }
        public UIButton FloorButton { get; set; }
        public UIButton DoorButton { get; set; }
        public UIButton WindowButton { get; set; }
        public UIButton RoofButton { get; set; }
        public UIButton HandButton { get; set; }

        public Texture2D subtoolsBackground { get; set; }
        public Texture2D dividerImage { get; set; }
        
        public UIImage Divider;

        public UIImage SubToolBg;
        public UISlider SubtoolsSlider { get; set; }
        public UIButton PreviousPageButton { get; set; } 
        public UIButton NextPageButton { get; set; }

        private UISlider RoofSlider;
        private UIButton RoofSteepBtn;
        private UIButton RoofShallowBtn;
        private uint TicksSinceRoof = 0;
        private bool SendRoofValue = false;

        private bool MiddleWasDown;
        private Point MiddleDownAt;

        public UIBuildMode(UILotControl lotController) : base("buildpanel", lotController)
        {
            Divider = new UIImage(dividerImage);
            Divider.Position = new Vector2(337, 14);
            this.AddAt(1, Divider);

            SubToolBg = new UIImage(subtoolsBackground);
            SubToolBg.Position = new Vector2(336, 5);
            this.AddAt(2, SubToolBg);

            Catalog.Position = new Vector2(364, 7);

            PreviousPageButton.OnButtonClick += PreviousPage;
            NextPageButton.OnButtonClick += NextPage;
            SubtoolsSlider.MinValue = 0;
            SubtoolsSlider.OnChange += PageSlider;

            LotController.ObjectHolder.Roommate = true;
            LotController.QueryPanel.Roommate = true;

            RoofSteepBtn = new UIButton(GetTexture(0x4C200000001));
            RoofSteepBtn.X = 46;
            RoofSteepBtn.Y = 6;
            Add(RoofSteepBtn);
            RoofShallowBtn = new UIButton(GetTexture(0x4C700000001));
            RoofShallowBtn.X = 46;
            RoofShallowBtn.Y = 92;
            Add(RoofShallowBtn);
            
            RoofSlider = new UISlider();
            RoofSlider.Orientation = 1;
            RoofSlider.Texture = GetTexture(0x4AB00000001);
            RoofSlider.MinValue = 0f;
            RoofSlider.MaxValue = 1.25f;
            RoofSlider.AllowDecimals = true;
            RoofSlider.AttachButtons(RoofSteepBtn, RoofShallowBtn, 0.25f);
            RoofSlider.X = 48;
            RoofSlider.Y = 24;
            RoofSlider.OnChange += (elem) =>
            {
                if (RoofSlider.Value != (1.25f - LotController.vm.Context.Architecture.RoofPitch))
                {
                    LotController.vm.Context.Blueprint.RoofComp.SetStylePitch(
                        LotController.vm.Context.Architecture.RoofStyle,
                        (1.25f - RoofSlider.Value)
                        );
                    SendRoofValue = true;
                }
            };
            RoofSlider.SetSize(0, 64f);
            Add(RoofSlider);

            RoofSteepBtn.Visible = false;
            RoofShallowBtn.Visible = false;
            RoofSlider.Visible = false;
        }

        public override void InitCategoryMap()
        {
            CategoryMap = new Dictionary<UIButton, int>
            {
                { TerrainButton, 10 },
                { WaterButton, 5 },
                { WallButton, 7 },
                { WallpaperButton, 8 },
                { StairButton, 2 },
                { FireplaceButton, 4 },

                { PlantButton, 3 },
                { FloorButton, 9 },
                { DoorButton, 0 },
                { WindowButton, 1 },
                { RoofButton, 6 },
                { HandButton, 28 },
            };
        }

        public override void SetPage(int page)
        {
            int total = Catalog.TotalPages();

            bool noPrev = (page == 0);
            PreviousPageButton.Disabled = noPrev;

            bool noNext = (page + 1 == total);
            NextPageButton.Disabled = noNext;

            Catalog.SetPage(page);
            if (OldSelection != -1) Catalog.SetActive(OldSelection, true);

            SubtoolsSlider.MaxValue = total - 1;
            SubtoolsSlider.Value = page;
        }

        public override void ChangeCategory(UIElement elem)
        {
            QueryPanel.InInventory = 0;
            foreach (var btn in CategoryMap.Keys)
                btn.Selected = false;

            UIButton button = (UIButton)elem;
            button.Selected = true;
            if (!CategoryMap.ContainsKey(button)) return;
            CurrentCategory = UICatalog.Catalog[CategoryMap[button]];
            Catalog.SetCategory(CurrentCategory);

            var isRoof = CategoryMap[button] == 6;
            RoofShallowBtn.Visible = isRoof;
            RoofSteepBtn.Visible = isRoof;
            RoofSlider.Visible = isRoof;
            RoofSlider.Value = 1.25f - LotController.vm.Context.Architecture.RoofPitch;

            int total = Catalog.TotalPages();
            OldSelection = -1;

            SubtoolsSlider.MaxValue = total - 1;
            SubtoolsSlider.Value = 0;

            NextPageButton.Disabled = (total == 1);

            if (LotController.CustomControl != null)
            {
                LotController.CustomControl.Release();
                LotController.CustomControl = null;
            }

            PreviousPageButton.Disabled = true;

            var showsubtools = CategoryMap[button] != 10;
            SubToolBg.Visible = showsubtools;
            SubtoolsSlider.Visible = showsubtools;
            PreviousPageButton.Visible = showsubtools;
            NextPageButton.Visible = showsubtools;
            
            return;
        }

        public override void Update(UpdateState state)
        {
            CategoryMap[TerrainButton] = (state.ShiftDown && (LotController?.ActiveEntity?.TSOState as VMTSOAvatarState)?.Permissions >= VMTSOAvatarPermissions.Admin) ? 29 : 10;
            var objCount = LotController.vm.Context.ObjectQueries.NumUserObjects;
            if (LastObjCount != objCount || LastDonator != LotController.ObjectHolder.DonateMode)
            {
                if (LastDonator != LotController.ObjectHolder.DonateMode)
                {
                    Catalog.SetPage(Catalog.Page); //update prices
                }
                if (LotController.ObjectHolder.DonateMode) {
                    ObjLimitLabel.Caption = GameFacade.Strings.GetString("f114", "4");
                    ObjLimitLabel.CaptionStyle.Color = new Color(255, 201, 38);
                } else {
                    var limit = LotController.vm.TSOState.ObjectLimit;
                    ObjLimitLabel.Caption = objCount + "/" + limit + " Objects";
                    var lerp = objCount / (float)limit;
                    if (lerp < 0.5)
                        ObjLimitLabel.CaptionStyle.Color = Color.White;
                    if (lerp < 0.75)
                        ObjLimitLabel.CaptionStyle.Color = Color.Lerp(Color.White, new Color(255, 201, 38), lerp * 4 - 2);
                    else
                        ObjLimitLabel.CaptionStyle.Color = Color.Lerp(new Color(255, 201, 38), Color.Red, lerp * 4 - 3);
                }
                LastObjCount = objCount;
                LastDonator = LotController.ObjectHolder.DonateMode;
            }

            if (LotController.ActiveEntity != null) Catalog.Budget = (int)LotController.Budget;
            TicksSinceRoof++;
            if (TicksSinceRoof > 30 && SendRoofValue)
            {
                LotController.vm.SendCommand(new SimAntics.NetPlay.Model.Commands.VMNetSetRoofCmd()
                {
                    Pitch = 1.25f - RoofSlider.Value,
                    Style = LotController.vm.Context.Architecture.RoofStyle
                });
                SendRoofValue = false;
                TicksSinceRoof = 0;
            }
            UpdateEyedropper(state);
            base.Update(state);
        }

        private void UpdateEyedropper(UpdateState state)
        {
            if (state.TouchMode) return;
            var middle = state.MouseState.MiddleButton == ButtonState.Pressed;
            if (middle && !MiddleWasDown) MiddleDownAt = state.MouseState.Position;
            if (!middle && MiddleWasDown && LotController.MouseIsOn)
            {
                //only a stationary click samples - middle drag is camera rotation
                var moved = state.MouseState.Position - MiddleDownAt;
                if (Math.Abs(moved.X) + Math.Abs(moved.Y) < 8) Eyedrop(state);
            }
            MiddleWasDown = middle;
        }

        private void Eyedrop(UpdateState state)
        {
            var arch = LotController.vm.Context.Architecture;
            var world = LotController.World;
            var tilePos = world.EstTileAtPosWithScroll(LotController.GetScaledPoint(state.MouseState.Position).ToVector2());
            if (tilePos.X < 0 || tilePos.Y < 0 || tilePos.X >= arch.Width || tilePos.Y >= arch.Height) return;
            short x = (short)tilePos.X, y = (short)tilePos.Y;
            var level = world.State.Level;

            var floorPat = arch.GetFloor(x, y, level).Pattern;
            var wallPat = SampleWallPattern(arch, tilePos, level);

            //wallpaper category prefers sampling walls, everything else prefers floors
            ushort pattern; int category;
            if (floorPat != 0 && !(CurrentCategory == UICatalog.Catalog[8] && wallPat != 0))
            {
                pattern = floorPat;
                category = (floorPat >= 65534) ? 5 : 9;
            }
            else
            {
                pattern = wallPat;
                category = 8;
            }

            if (pattern == 0 || !SelectCatalogItem(category, pattern))
                HITVM.Get().PlaySoundEvent(UISounds.Error);
        }

        //dir indices match VMArchitectureTools: 0=TopLeft(x-), 1=TopRight(y-), 2=BottomRight(x+), 3=BottomLeft(y+)
        private static readonly Point[] DirOffsets = { new Point(-1, 0), new Point(0, -1), new Point(1, 0), new Point(0, 1) };

        /// <summary>
        /// Samples the wall face the cursor visually hit. The tile estimate projects onto the floor plane,
        /// so a click on a wall face lands PAST the wall's base tile - walking back down-screen finds the
        /// wall whose viewer-facing face was clicked, rather than reading the far side's pattern.
        /// </summary>
        private ushort SampleWallPattern(VMArchitecture arch, Vector2 tilePos, sbyte level)
        {
            int r = (int)LotController.World.State.CutRotation;
            var t = new Point((int)tilePos.X, (int)tilePos.Y);
            var fract = new Vector2(tilePos.X - t.X, tilePos.Y - t.Y);

            int upA = (4 - r) & 3, upB = (5 - r) & 3; //edges facing away from the viewer (walls' visible faces point into t)
            int downA = (6 - r) & 3, downB = (7 - r) & 3; //edges toward the viewer
            var downStep = new Point(DirOffsets[downA].X + DirOffsets[downB].X, DirOffsets[downA].Y + DirOffsets[downB].Y);

            //clicked the floor just in front of a wall base: sample that wall's face into this tile
            var upNear = (EdgeDist(fract, upA) <= EdgeDist(fract, upB)) ? upA : upB;
            if (EdgeDist(fract, upNear) < 0.3f)
            {
                var basePat = FaceIntoTile(arch, t, upNear, level);
                if (basePat != -1) return (ushort)basePat;
            }

            //walk toward the viewer; the clicked face belongs to the first wall under the cursor's screen column
            for (int k = 0; k < 4; k++)
            {
                var tk = new Point(t.X + downStep.X * k, t.Y + downStep.Y * k);
                if (tk.X < 0 || tk.Y < 0 || tk.X >= arch.Width || tk.Y >= arch.Height) break;
                var wall = arch.GetWall((short)tk.X, (short)tk.Y, level);

                if ((wall.Segments & (WallSegments.HorizontalDiag | WallSegments.VerticalDiag)) > 0)
                {
                    if (wall.TopRightStyle != 1) continue;
                    //same face mapping WallPatternDot uses for the painter's preferred direction
                    if ((wall.Segments & WallSegments.HorizontalDiag) > 0)
                        return (upNear < 2) ? wall.BottomRightPattern : wall.BottomLeftPattern;
                    return (upNear > 0 && upNear < 3) ? wall.BottomLeftPattern : wall.BottomRightPattern;
                }

                var first = (EdgeDist(fract, downA) <= EdgeDist(fract, downB)) ? downA : downB;
                var second = (first == downA) ? downB : downA;
                for (int c = 0; c < 2; c++)
                {
                    var d = (c == 0) ? first : second;
                    if ((wall.Segments & (WallSegments)(1 << d)) == 0) continue;
                    //viewer-facing face of this wall = the face into the tile on the viewer's side
                    var pat = FaceIntoTile(arch, new Point(tk.X + DirOffsets[d].X, tk.Y + DirOffsets[d].Y), (d + 2) & 3, level);
                    if (pat != -1) return (ushort)pat;
                }
            }
            return 0;
        }

        private static float EdgeDist(Vector2 fract, int d)
        {
            switch (d)
            {
                case 0: return fract.X;
                case 1: return fract.Y;
                case 2: return 1 - fract.X;
                default: return 1 - fract.Y;
            }
        }

        /// <summary>
        /// Pattern of the wall face on edge d of tile t, as seen from inside t. -1 if no wall there.
        /// </summary>
        private int FaceIntoTile(VMArchitecture arch, Point t, int d, sbyte level)
        {
            if (t.X < 0 || t.Y < 0 || t.X >= arch.Width || t.Y >= arch.Height) return -1;
            var w = arch.GetWall((short)t.X, (short)t.Y, level);
            if ((w.Segments & (WallSegments)(1 << d)) == 0) return -1;
            switch (d)
            {
                case 0: return w.TopLeftThick ? w.TopLeftPattern : -1;
                case 1: return w.TopRightThick ? w.TopRightPattern : -1;
                case 2: return (t.X + 1 < arch.Width && arch.GetWall((short)(t.X + 1), (short)t.Y, level).TopLeftThick) ? w.BottomRightPattern : -1;
                default: return (t.Y + 1 < arch.Height && arch.GetWall((short)t.X, (short)(t.Y + 1), level).TopRightThick) ? w.BottomLeftPattern : -1;
            }
        }

        private bool SelectCatalogItem(int category, ulong resID)
        {
            var target = UICatalog.Catalog[category];
            if (CurrentCategory != target)
            {
                var button = CategoryMap.FirstOrDefault(e => e.Value == category).Key;
                if (button == null) return false;
                ChangeCategory(button);
            }
            var index = Catalog.Filtered.FindIndex(e => e.Special != null && e.Special.ResID == resID);
            if (index == -1 && Catalog.Filtered != Catalog.Selected)
            {
                Catalog.SetSearchTerm("");
                index = Catalog.Filtered.FindIndex(e => e.Special != null && e.Special.ResID == resID);
            }
            if (index == -1) return false;
            SetPage(index / Catalog.PageSize);
            Catalog_OnSelectionChange(index);
            HITVM.Get().PlaySoundEvent(UISounds.Click);
            return true;
        }
    }
}
