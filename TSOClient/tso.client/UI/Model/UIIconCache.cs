using FSO.Common.Rendering.Framework;
using FSO.Content;
using FSO.LotView.Components;
using FSO.Common.Rendering.Framework.Camera;
using FSO.SimAntics;
using FSO.Vitaboy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FSO.Client.UI.Model
{
    /// <summary>
    /// Caches icons for objects missing them, eg. heads, some catalog objects..
    /// </summary>
    public static class UIIconCache
    {
        //indexed as mesh:texture
        private static Dictionary<ulong, Texture2D> AvatarHeadCache = new Dictionary<ulong, Texture2D>();

        /*
        public static Texture2D GetObject(VMEntity obj)
        {
            if (obj is VMAvatar)
            {
                var ava = (VMAvatar)obj;
                var headname = ava.HeadOutfit.Name;
                if (headname == "") headname = ava.BodyOutfit.OftData.TS1TextureID;
                var id = headname +":"+ ava.HeadOutfit.OftData.TS1TextureID;

                Texture2D result = null;
                if (!AvatarHeadCache.TryGetValue(id, out result))
                {
                    result = GenHeadTex(ava);
                    AvatarHeadCache[id] = result;
                }
                return result;
            }
            else if (obj is VMGameObject)
            {
                if (obj.Object.OBJ.GUID == 0x000007C4) return Content.Get().CustomUI.Get("int_gohere.png").Get(GameFacade.GraphicsDevice);
                else return obj.GetIcon(GameFacade.GraphicsDevice, 0);
            }
            return null;
        }*/

        public static Texture2D GetObject(VMEntity obj)
        {
            if (obj is VMAvatar)
            {
                var ava = (VMAvatar)obj;
                return GenHeadTex(ava.HeadOutfit?.ID ?? 0, ava.BodyOutfit.ID);
            }
            else if (obj is VMGameObject)
            {
                return GetObjectIcon(obj.Object);
            }
            return null;
        }

        //a shipped catalog icon is two 37x37 snapshots of the object side by side - the first on
        //a dark background, the second on a light one for hover. Matching that layout exactly lets
        //drawn icons take the same path as real ones, in the catalog and the action queue alike
        //(UIInteraction draws the left half of anything wider than 45px).
        private const int ICON_FRAME = 37;
        private static readonly Color IconBackNormal = new Color(56, 88, 120);
        private static readonly Color IconBackHover = new Color(184, 212, 240);
        private static Dictionary<uint, Texture2D> ObjIconCache = new Dictionary<uint, Texture2D>();

        /// <summary>
        /// Builds a catalog icon from the object's own sprites, for objects the game shipped
        /// without a catalog BMP (the Hygeia-O-Matic Toilet and friends). Snapshots the object
        /// at the same angle and zoom the shipped icons use, then lays it out as the usual
        /// normal + hover frame pair. Null until a world exists to render in - callers retry.
        /// </summary>
        public static Texture2D GetObjectIcon(GameObject obj)
        {
            if (obj == null) return null;
            Texture2D cached;
            if (ObjIconCache.TryGetValue(obj.OBJ.GUID, out cached)) return cached;

            var world = (GameFacade.Screens.CurrentUIScreen as FSO.Client.UI.Screens.CoreGameScreen)?.LotControl?.World;
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
                ObjIconCache[obj.OBJ.GUID] = tex;
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

        public static Texture2D GenHeadTex(ulong headOft, ulong bodyOft)
        {
            if (headOft == 0) headOft = bodyOft;

            Texture2D result = null;
            if (!AvatarHeadCache.TryGetValue(headOft, out result))
            {
                var ofts = Content.Content.Get().AvatarOutfits;
                var oft = ofts.Get(headOft);
                if (oft == null) return null;
                else
                {
                    result = GenHeadTex(oft, ofts.GetNameByID(headOft));
                }
                AvatarHeadCache[headOft] = result;
            }
            return result;
        }

        public static Texture2D GenHeadTex(Outfit headOft, string name)
        {
            var skels = Content.Content.Get().AvatarSkeletons;
            Skeleton skel = null;
            bool pet = false;
            if (name.StartsWith("uaa"))
            {
                //pet
                if (name.Contains("cat")) skel = skels.Get("cat.skel");
                else skel = skels.Get("dog.skel");
                pet = true;
            } else
            {
                skel = skels.Get("adult.skel");
            }

            var m_Head = new SimAvatar(skel);
            m_Head.Head = headOft;
            m_Head.ReloadSkeleton();
            m_Head.StripAllButHead();

            var HeadCamera = new BasicCamera(GameFacade.GraphicsDevice, new Vector3(0.0f, 7.0f, -17.0f), Vector3.Zero, Vector3.Up);

            var pos2 = m_Head.Skeleton.GetBone("HEAD").AbsolutePosition;
            pos2.Y += (pet)?((name.Contains("dog"))?0.16f:0.1f):0.12f;
            HeadCamera.Position = new Vector3(0, pos2.Y, 12.5f);
            HeadCamera.FOV = (float)Math.PI / 3f;
            HeadCamera.Target = pos2;
            HeadCamera.ProjectionOrigin = new Vector2(66/2, 66/2);

            var HeadScene = new _3DTargetScene(GameFacade.GraphicsDevice, HeadCamera, new Point(66, 66), 0);// (GlobalSettings.Default.AntiAlias) ? 8 : 0);
            HeadScene.ID = "UIPieMenuHead";
            HeadScene.ClearColor = new Color(49, 65, 88);

            m_Head.Scene = HeadScene;
            m_Head.Scale = new Vector3(1f);

            HeadCamera.Zoom = 19.5f;

            //rotate camera, similar to pie menu

            double xdir = 0;//Math.Atan(0);
            double ydir = 0;//Math.Atan(0);

            Vector3 off = new Vector3(0, 0, 13.5f);
            Matrix mat = Microsoft.Xna.Framework.Matrix.CreateRotationY((float)xdir) * Microsoft.Xna.Framework.Matrix.CreateRotationX((float)ydir);

            HeadCamera.Position = new Vector3(0, pos2.Y, 0) + Vector3.Transform(off, mat);

            if (pet)
            {
                HeadCamera.Zoom *= 1.3f;
            }
            //end rotate camera

            HeadScene.Initialize(GameFacade.Scenes);
            HeadScene.Add(m_Head);

            HeadScene.Draw(GameFacade.GraphicsDevice);
            return Common.Utils.TextureUtils.Decimate(HeadScene.Target, GameFacade.GraphicsDevice, 2, true);
        }
    }
}
