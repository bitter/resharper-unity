﻿using System.Collections.Generic;

#nullable enable

namespace JetBrains.ReSharper.Plugins.Unity.AsmDef.Psi.Caches
{
    public class AsmDefCacheItemBuilder
    {
        private readonly string myName;
        private readonly int myNameOffset;
        private readonly List<string> myReferences = new();

        public AsmDefCacheItemBuilder(string name, int nameOffset)
        {
            myName = name;
            myNameOffset = nameOffset;
        }

        public void AddReference(string? reference)
        {
            if (reference != null)
                myReferences.Add(reference);
        }

        public AsmDefCacheItem Build() => new(myName, myNameOffset, myReferences.ToArray());
    }
}