/*
 * System.String test = Asset.Dir("Textures").Dir2("Gras").File2("Leaf.png");
 * test = Asset.File("a.fx");
 * System.String test = Asset.Dir("one").file("two.f");
 * test = Asset.File("one.f");
 * test = Asset.Dir("one").dir("two").file("three.f");
 */


using System;
using System.Collections.Generic;
using System.Text;

namespace Uncut
{
    class Asset
    {
        public Asset()
        {
            path = "Resources\\";
        }

        public Asset(String aPath)
        {
            path = aPath;
        }

        public Asset dir(String directory)
        {
            path += directory;
            path += "\\";

            return new Asset(path);
        }

        static public Asset Dir(String directory)
        {
            Asset newAsset = new Asset();
            newAsset.dir(directory);
            return newAsset;
        }

        public String file(String filename)
        {
            path += filename;
            return path;
        }

        static public String File(String filename)
        {
            Asset local = new Asset();
            String fileLocation = local.file(filename);
            return fileLocation;
        }

        private String path;
    }
}
