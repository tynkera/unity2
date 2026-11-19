using System.IO;
using UnityEditor;
using UnityEngine;

namespace ExpeditionCamp.EditorTools
{
    public class ImageImportSettings : AssetPostprocessor
    {
        private const string IconsFolder = "Assets/Resources/Icons/";
        private const string UiFolder = "Assets/Resources/UI/";
        private const string KenneyFolder = "Assets/ThirdParty/Kenney/";
        private const string SpritesFolder = "Assets/Sprites/";
        private const string ArtFolder = "Assets/Art/";

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            bool isIcon = path.StartsWith(IconsFolder);
            bool isUi = path.StartsWith(UiFolder);
            bool isKenney = path.StartsWith(KenneyFolder);
            bool isSprite = path.StartsWith(SpritesFolder) || path.StartsWith(ArtFolder);
            if (!isIcon && !isUi && !isKenney && !isSprite)
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = GetMaxTextureSize(path);

            if (isKenney)
            {
                importer.spriteBorder = GetKenneyBorder(path);
            }
        }

        private static int GetMaxTextureSize(string path)
        {
            int width;
            int height;
            bool found = TryReadPngSize(path, out width, out height);
            if (!found)
            {
                return 2048;
            }

            int longest = Mathf.Max(width, height);
            int size = 32;
            while (size < longest && size < 16384)
            {
                size *= 2;
            }
            return size;
        }

        private static bool TryReadPngSize(string path, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (!File.Exists(path))
            {
                return false;
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < header.Length)
                {
                    return false;
                }
            }

            bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            if (!isPng)
            {
                return false;
            }

            width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
            return true;
        }

        private static Vector4 GetKenneyBorder(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Contains("depth"))
            {
                return new Vector4(12f, 14f, 12f, 12f);
            }
            return new Vector4(12f, 12f, 12f, 12f);
        }
    }
}
