using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
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
       
        public TgcSkeletalMesh mesh;
        public Actions actions;
        public Vector3 movementVector;
        public Life life;
        private string playername = "TGC Player";
        
        List<Poder> poder = new List<Poder>();



        FightGameManager fightGameManager = new FightGameManager();
        TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
        TgcKeyFrameLoader keyFrameLoader = new TgcKeyFrameLoader();
        int direccion = -1;
        Device d3dDevice = GuiController.Instance.D3dDevice;
        BoundingMultiSphere spheres = new BoundingMultiSphere();


        //Getters y Setters
        public string Playername
        {
            get { return playername; }
            set { playername = value; }
        }
        public int Direccion
        {
            get { return direccion; }
            set { direccion = value; }
        }
        //end Get & Set
        
        //structs
        public struct Actions
        {
            public bool moving;
            public bool kick;
            public bool punch;
            public bool hit;
            public bool power;
            public bool jumping;
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

        
       public void Init()
       {
           
           mesh = skeletalLoader.loadMeshAndAnimationsFromFile(
               fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
               fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\",
               new string[] { 
                    fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                    fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "CaminandoRev-TgcSkeletalAnim.xml",
                    fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Parado-TgcSkeletalAnim.xml",
                    fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Patear-TgcSkeletalAnim.xml",
                    fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Pegar-TgcSkeletalAnim.xml",
                    fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "Cruz-TgcSkeletalAnim.xml",
                    fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "Lose-TgcSkeletalAnim.xml",
                    fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "Win-TgcSkeletalAnim.xml",
                    fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Arrojar-TgcSkeletalAnim.xml",
                });
           mesh.Scale = new Vector3(0.05f, 0.05f, 0.05f);
           
            
           mesh.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, fightGameManager.TexturePath + "uvw.jpg") });
           spheres.getVerticesForBox(fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml", mesh);
           mesh.AutoUpdateBoundingBox = false;
           spheres.GlobalSphere = new TgcBoundingSphere(mesh.BoundingBox.calculateBoxCenter(),
                                                             mesh.BoundingBox.calculateBoxRadius());
           actions.jump = 0f;
           actions.hittimer = 0;
          // poder.powerhit = false;
           life.healthpoints = 100;
           life.bloodypoints = 100;

           //Configurar animacion inicial
           mesh.playAnimation("Parado", true);







       }

        public void metodoEjemplo(int variable)
        {
        //comment
            
        }

        public void tirarPoder()
        {
            Poder pow = new Poder();
            pow.Init(direccion, mesh.Position + new Vector3(0,3,0), new Vector3(40, 0, 0));
            poder.Add(pow);

        }
        public void setColor (Color color)
        {
        //setea el color del personaje
        mesh.setColor(color);
        }
        public void setPosition(Vector3 vec3)
        {
        //setea la position del personaje
            mesh.Position = vec3;
        }
        public Vector3 getPosition()
        {
            //getea la position del personaje
            return mesh.Position;
        }
        public void setRotation(float radianes)
        {
        //setea la rotacion del pj
         mesh.rotateY(radianes);
         direccion = (radianes == 270) ? 1 : -1; ;
        }



        public void update(float elapsedTime)
        {
            //mesh.move(movementVector * elapsedTime);
            //globalSphere.moveCenter(movementVector * elapsedTime);
            foreach (Poder pow in poder)
            {
                pow.update(elapsedTime);

            }
        }
        public void render(float elapsedTime)
        {
            mesh.animateAndRender();
            foreach (Poder pow in poder)
                {
                    pow.render(elapsedTime);

                }
            
        }
    }
}
