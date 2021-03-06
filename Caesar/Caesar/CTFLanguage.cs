﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Caesar
{
    public class CTFLanguage
    {
        public string Qualifier;
        public int LanguageIndex;
        public int StringPoolSize;
        public int MaybeOffsetFromStringPoolBase;
        public int StringCount;
        public List<string> StringEntries;

        public long BaseAddress;
        public CTFLanguage(BinaryReader reader, long baseAddress, int headerSize) 
        {
            BaseAddress = baseAddress;
            reader.BaseStream.Seek(BaseAddress, SeekOrigin.Begin);

            ulong languageEntryBitflags = reader.ReadUInt16();

            Qualifier = CaesarReader.ReadBitflagStringWithReader(ref languageEntryBitflags, reader, BaseAddress);
            LanguageIndex = CaesarReader.ReadBitflagInt16(ref languageEntryBitflags, reader);
            StringPoolSize = CaesarReader.ReadBitflagInt32(ref languageEntryBitflags, reader);
            MaybeOffsetFromStringPoolBase = CaesarReader.ReadBitflagInt32(ref languageEntryBitflags, reader);
            StringCount = CaesarReader.ReadBitflagInt32(ref languageEntryBitflags, reader);

            // I have no idea if encoding data is included, using ascii as a default for now. Some german character data will be lost
            LoadStrings(reader, headerSize, CaesarReader.DefaultEncoding);

            //PrintDebug();
        }

        public void LoadStrings(BinaryReader reader, int headerSize, Encoding encoding) 
        {
            StringEntries = new List<string>();
            int caesarStringTableOffset = headerSize + 0x410 + 4; // header.CffHeaderSize
            for (int i = 0; i < StringCount; i++) 
            {
                reader.BaseStream.Seek(caesarStringTableOffset + (i * 4), SeekOrigin.Begin);
                int stringOffset = reader.ReadInt32();
                reader.BaseStream.Seek(caesarStringTableOffset + stringOffset, SeekOrigin.Begin);
                string result = CaesarReader.ReadStringFromBinaryReader(reader, encoding);
                StringEntries.Add(result);
            }
        }

        public string GetString(int stringId) 
        {
            return GetString(StringEntries, stringId);
        }

        public static string GetString(List<string> language, int stringId) 
        {
            if (stringId < 0) 
            {
                return "";
            }
            if (stringId > language.Count) 
            {
                return "";
            }
            return language[stringId];
        }

        public void PrintDebug() 
        {
            Console.WriteLine($"Language: {Qualifier} stringCount: {StringCount} stringPoolSize 0x{StringPoolSize:X}, unknowns: {LanguageIndex} {MaybeOffsetFromStringPoolBase}, base: {BaseAddress:X} ");
        }
    }
}
