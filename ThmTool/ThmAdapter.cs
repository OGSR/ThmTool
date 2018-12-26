using System;
using System.Collections.Generic;
using System.IO;

namespace ThmTool
{
    public class ThmAdapter
    {
        public int THM_CHUNK_VERSION = 0x0810;
        public int THM_CHUNK_DATA = 0x0811;
        public int THM_CHUNK_TEXTUREPARAM = 0x0812;
        public int THM_CHUNK_TYPE = 0x0813;
        public int THM_CHUNK_TEXTURE_TYPE = 0x0814;
        public int THM_CHUNK_DETAIL_EXT = 0x0815;
        public int THM_CHUNK_MATERIAL = 0x0816;
        public int THM_CHUNK_BUMP = 0x0817;
        public int THM_CHUNK_EXT_NORMALMAP = 0x0818;
        public int THM_CHUNK_FADE_DELAY = 0x0819;

        public enum ETFormat
        {
            tfDXT1 = 0,
            tfADXT1,
            tfDXT3,
            tfDXT5,
            tf4444,
            tf1555,
            tf565,
            tfRGB,
            tfRGBA,
            tfNVHS,
            tfNVHU,
            tfA8,
            tfL8,
            tfA8L8,
            // tfForceU32	= u32(-1)
        };

        public enum ETType
        {
            ttImage = 0,
            ttCubeMap,
            ttBumpMap,
            ttNormalMap,
            ttTerrain,
            // ttForceU32 = u32(-1)
        };

        public enum ETMaterial
        {
            tmOrenNayar_Blin = 0,
            tmBlin_Phong,
            tmPhong_Metal,
            tmMetal_OrenNayar,
            // tmForceU32 = u32(-1)
        };

        public enum ETBumpMode
        {
            tbmResereved = 0,
            tbmNone,
            tbmUse,

            // new
            tbmUseParallax,

            //tbmForceU32 = u32(-1)
        };

        [Flags]
        public enum Flags
        {
            flGenerateMipMaps = (1 << 0),
            flBinaryAlpha = (1 << 1),
            flAlphaBorder = (1 << 4),
            flColorBorder = (1 << 5),
            flFadeToColor = (1 << 6),
            flFadeToAlpha = (1 << 7),
            flDitherColor = (1 << 8),
            flDitherEachMIPLevel = (1 << 9),

            flDiffuseDetail = (1 << 23),
            flImplicitLighted = (1 << 24),
            flHasAlpha = (1 << 25),
            flBumpDetail = (1 << 26),

            //flForceU32 = u32(-1)
        };

        public class ETextureParams
        {
            public Flags flags;

            public ETFormat fmt;
            public ETType type;

            public string detail_name;
            public float detail_scale;

            public ETMaterial material;
            public float material_weight;

            public ETBumpMode bump_mode;
            public string bump_name;
        }

        bool find_chunk(BinaryReader r, int chunkId)
        {
            r.BaseStream.Position = 0;

            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                uint dwType = r.ReadUInt32();
                uint dwSize = r.ReadUInt32();

                if (dwType == chunkId)
                {
                    //return dwSize;
                    return true;
                }
                else
                {
                    r.BaseStream.Position += dwSize;
                }
            }

            return false;
        }

        long chunk_pos = 0;

        void open_chunk(BinaryWriter w, int chunkId)
        {
            w.Write(chunkId);
            chunk_pos = w.BaseStream.Position;
            w.Write(0);     // the place for 'size'
        }

        void close_chunk(BinaryWriter w)
        {
            if (chunk_pos == 0)
            {
                throw new InvalidOperationException("no chunk!");
            }

            long pos = w.BaseStream.Position;
            w.BaseStream.Position = chunk_pos;
            w.Write((int)(pos - chunk_pos - 4));
            w.BaseStream.Position = pos;
            chunk_pos = 0;
        }

        string read_stringZ(BinaryReader r)
        {
            List<char> str = new List<char>();

            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                byte one = r.ReadByte();
                if (one != 0)
                {
                    str.Add((char)one);
                }
                else
                {
                    break;
                }
            }

            return new string(str.ToArray());
        }

        void write_stringZ(BinaryWriter w, string str)
        {
            foreach (char c in str)
            {
                byte b = (byte)c;
                w.Write(b);
            }

            w.Write((byte)0);
        }


        public ETextureParams Load(string file)
        {
            using (var r = new BinaryReader(File.OpenRead(file)))
            {
                if (!find_chunk(r, THM_CHUNK_TYPE))
                {
                    return null;
                }

                r.ReadUInt32(); // skip

                if (!find_chunk(r, THM_CHUNK_TEXTUREPARAM))
                {
                    return null;
                }

                ETextureParams result = new ETextureParams();

                result.fmt = (ETFormat)r.ReadInt32();
                result.flags = (Flags)r.ReadUInt32();

                // not used at all
                uint border_color = r.ReadUInt32();
                uint fade_color = r.ReadUInt32();
                uint fade_amount = r.ReadUInt32();
                uint mip_filter = r.ReadUInt32();
                uint width = r.ReadUInt32();
                uint height = r.ReadUInt32();
                // not used at all

                if (find_chunk(r, THM_CHUNK_TEXTURE_TYPE))
                {
                    result.type = (ETType)r.ReadUInt32();
                }

                if (find_chunk(r, THM_CHUNK_DETAIL_EXT))
                {
                    result.detail_name = read_stringZ(r);
                    result.detail_scale = r.ReadSingle();
                }

                if (find_chunk(r, THM_CHUNK_MATERIAL))
                {
                    result.material = (ETMaterial)r.ReadUInt32();
                    result.material_weight = r.ReadSingle();
                }

                if (find_chunk(r, THM_CHUNK_BUMP))
                {
                    float bump_virtual_height = r.ReadSingle();
                    ETBumpMode bump_mode = (ETBumpMode)r.ReadUInt32();
                    if (bump_mode < ETBumpMode.tbmNone)
                    {
                        bump_mode = ETBumpMode.tbmNone; //.. временно (до полного убирания Autogen)
                    }
                    result.bump_mode = bump_mode;
                    result.bump_name = read_stringZ(r);
                }

                // not used at all
                if (find_chunk(r, THM_CHUNK_EXT_NORMALMAP))
                {
                    string ext_normal_map_name = read_stringZ(r);
                }

                if (find_chunk(r, THM_CHUNK_FADE_DELAY))
                {
                    byte fade_delay = r.ReadByte();
                }
                // not used at all

                return result;
            }
        }

        public void Save(ETextureParams item, string file)
        {
            using (var w = new BinaryWriter(File.OpenWrite(file)))
            {
                open_chunk(w, THM_CHUNK_TYPE);
                w.Write(0); // skip
                close_chunk(w);

                open_chunk(w, THM_CHUNK_TEXTUREPARAM);
                w.Write((int)item.fmt);
                w.Write((int)item.flags);
                w.Write(0);
                w.Write(0);
                w.Write(0);
                w.Write(0);
                w.Write(0);
                w.Write(0);
                close_chunk(w);

                open_chunk(w, THM_CHUNK_TEXTURE_TYPE);
                w.Write((int)item.type);
                close_chunk(w);

                open_chunk(w, THM_CHUNK_DETAIL_EXT);
                write_stringZ(w, item.detail_name);
                w.Write(item.detail_scale);
                close_chunk(w);

                open_chunk(w, THM_CHUNK_MATERIAL);
                w.Write((int)item.material);
                w.Write(item.material_weight);
                close_chunk(w);

                open_chunk(w, THM_CHUNK_BUMP);
                w.Write(0.0f);
                w.Write((int)item.bump_mode);
                write_stringZ(w, item.bump_name);
                close_chunk(w);

                open_chunk(w, THM_CHUNK_EXT_NORMALMAP);
                write_stringZ(w, "");
                close_chunk(w);

                open_chunk(w, THM_CHUNK_FADE_DELAY);
                w.Write((byte)0);
                close_chunk(w);
            }
        }

    }
}