#define CSV
using UnityEngine;
using System;
using System.IO;
namespace TiledJsonUtility
{
    [Serializable]
    public class TiledLayer
    {
        public string draworder;
        public int height;
        public int width;
        public string name;
        public TiledObject[] objects;
        public float opacity;
        public object properties;
        public string type;
        public bool visible;
        public int x;
        public int y;
        public string compression;
        public string encoding;
#if CSV
        public UInt32[] data;
#else
        public string data;
#endif

        public TiledLayerTile[] GetTiles()
        {
            TiledLayerTile[] tiles = null;
#if CSV
            int index = 0;
            tiles = new TiledLayerTile[height * width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    UInt32 global_tile_id = data[x + y * width];
                    tiles[index] = new TiledLayerTile(x, y, global_tile_id);
                    index++;
                }
            }
#else
            if (encoding != null && encoding.Equals(TiledBase64Deflate.BASE64))
            {
                TiledBase64Deflate deflater = new TiledBase64Deflate(encoding, compression, data);
                if (deflater.Result.Length < height * width * sizeof(Int32))
                {
                    Debug.LogError("Tiled Layer is expected to have height * width * sizeof(Int32) bytes");
                    return null;
                }
                BinaryReader reader = new BinaryReader(deflater.Result);
                tiles = new TiledLayerTile[height * width];
                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        UInt32 global_tile_id = reader.ReadUInt32();
                        tiles[index] = new TiledLayerTile(x, y, global_tile_id);
                        index++;
                    }
                }
            }
#endif
            return tiles;
        }
    }
}