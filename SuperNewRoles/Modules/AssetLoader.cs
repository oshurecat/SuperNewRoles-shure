// From https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Modules/AssetLoader.cs
using System.IO;
using System.Reflection;
using UnityEngine;
using SuperNewRoles.CustomObject;

using Il2CppType = UnhollowerRuntimeLib.Il2CppType;

namespace SuperNewRoles.Modules
{
    public static class AssetLoader
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        private static bool flag = false;
        public static void LoadAssets()
        {
            if (flag) return;
            flag = true;
            var resourceWaveCannonAssetBundleStream = assembly.GetManifestResourceStream("SuperNewRoles.Resources.AssetBundle.wave_cannon_pack");
            var waveCannonBundle = AssetBundle.LoadFromMemory(resourceWaveCannonAssetBundleStream.ReadFully());
            #region Load Assets
            // example: LoadSpriteFromAssets(assetBundleBundle.LoadAsset<Texture2D>("SoothSayerButton.png").DontUnload(),115f);
            for (int i = 1; i <= 12; i++)
            {
                WaveCannonObject.AssetSprite.Add(LoadSpriteFromAssets(waveCannonBundle.LoadAsset<Texture2D>($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png").DontUnload(), 115f));
            }
            #endregion LoadAssets
        }
        public static byte[] ReadFully(this Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
#nullable enable
        public static T? LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }
#nullable disable
        public static T DontUnload<T>(this T obj) where T : Object
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return obj;
        }

        /// <summary>
        /// Texture2Dを拡大・縮小しSpriteで返す
        /// </summary>
        public static Sprite LoadSpriteFromAssets(Texture2D texture, float pixelsPerUnit)
            => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}