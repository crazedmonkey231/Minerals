using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using Minerals.Core;

namespace Minerals
{

    public class BigMineralTrophy : Building
    {
        // the path to the image printed
        public string printedTexturePath = null;
    
        public override void Print(SectionLayer layer)
        {
            Rand.PushState();
            Rand.Seed = ThingID.GetHashCode();

            // Get location
            Vector3 center = this.TrueCenter();
            if (def.graphicData.drawSize.y > def.size.z)
            {
                center.z += (def.graphicData.drawSize.y - def.size.z) / 2;
            }

            // Print image
            Material matSingle = Graphic.MatSingle;
            Printer_Plane.PrintPlane(layer, center, def.graphicData.drawSize, matSingle, 0, Rand.Bool, null, null, 0.01f, 0f);

            Rand.PopState();
        }

        public override Graphic Graphic
        {
            get
            {
                string printedTexturePath = getTexturePath();
                Graphic printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexturePath, def.graphicData.shaderType.Shader);
                printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexture.path, printedTexture.Shader, printedTexture.drawSize, DrawColor, DrawColorTwo, printedTexture.data);
                return printedTexture;
            }
        }

        public virtual void initTexturePath()
        {
            // Get paths to textures
            string textureName = System.IO.Path.GetFileName(def.graphicData.texPath);
            List<string> texturePaths = new List<string> { };
            List<string> versions = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            foreach (string letter in versions)
            {
                string a_path = def.graphicData.texPath + "/" + textureName + letter;
                if (ContentFinder<Texture2D>.Get(a_path, false) != null)
                {
                    texturePaths.Add(a_path);
                }
            }

            // Pick a random texture
            printedTexturePath = texturePaths.RandomElement();

        }

        public virtual string getTexturePath()
        {
            // initalize the array if it has not already been initalized
            if (printedTexturePath == null)
            {
                initTexturePath();
            }

            return(printedTexturePath);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref printedTexturePath, "printedTexturePath", null);
        }
    }
}
