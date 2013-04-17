using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcKeyFrameLoader;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Collision.ElipsoidCollision;

namespace AlumnoEjemplos.overflowDT
{
    class Personaje
    {
        public struct Actions
        {
            public bool moving;
            public bool kick;
            public bool punch;
            public bool hit;
            public bool power;
            public float hittimer;
            public float jump;
            public float moveForward;
        }

        public struct Life
        {
            public TgcSprite lifebar;
            public TgcSprite bloodybar;
            public decimal healthpoints;
            public decimal bloodypoints;
            public TgcText2d hpText;
        }

        public struct Power
        {
            public TgcKeyFrameMesh mesh;
            public TgcBoundingSphere globalSphere;
            public bool active;
            public bool powerhit;
            public Vector3 movementVector;
            public float[] combo;
        }

        public TgcSkeletalMesh mesh;
        public BoundingMultiSphere spheres;
        public Actions actions;
        public Vector3 movementVector;
        public Life life;
        public TgcText2d playername;
        public Power poder;
       public void Init()
       {
           
       }

    }
}
