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
using System.Threading;

namespace AlumnoEjemplos.overflowDT
{
    class Poder
    {

        public TgcKeyFrameMesh mesh;
        public TgcElipsoid globalSphere;
        public int powerhit = 5;
        public Vector3 movementVector;
        FightGameManager fightGameManager = new FightGameManager();
        TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
        TgcKeyFrameLoader keyFrameLoader = new TgcKeyFrameLoader();
        Personaje owner;
        ElipsoidCollisionManager collisionManager = new ElipsoidCollisionManager();
        public int dir;
        bool luz=false;
        int numluz=0;
        
        public Personaje Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        Device d3dDevice = GuiController.Instance.D3dDevice;
        BoundingMultiSphere spheres = new BoundingMultiSphere();
        public void Init(int direccion, Vector3 pos, Vector3 mov)
        {
            dir = direccion;
            mesh = keyFrameLoader.loadMeshAndAnimationsFromFile(
                   fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameMesh.xml",
                   fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\",
                   new string[] { fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameAnim.xml", });
            mesh.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            mesh.Position = pos;
            movementVector = mov*direccion;
            mesh.rotateY(Geometry.DegreeToRadian(direccion==1? 0f : -180f));
            
            mesh.playAnimation("Animation", true);
            globalSphere = new TgcElipsoid(mesh.BoundingBox.calculateBoxCenter(), new Vector3(mesh.BoundingBox.calculateAxisRadius().Y,mesh.BoundingBox.calculateAxisRadius().Y,mesh.BoundingBox.calculateAxisRadius().Y));
        }
        public void update(float elapsedTime)
        {
            
            //realmove = collisionManager.moveCharacter(globalSphere, (movementVector * elapsedTime), owner.ObjCol);
            
            
            mesh.move(movementVector * elapsedTime);
            globalSphere.moveCenter(movementVector * elapsedTime);
            if ((globalSphere.Center.X * dir > owner.Enemigo.getPosition().X * dir - 2) & (globalSphere.Center.X * dir < owner.Enemigo.getPosition().X * dir + 2 ))
            {
                if ((globalSphere.Center.Y > owner.Enemigo.getPosition().Y) && (globalSphere.Center.Y < (owner.Enemigo.getPosition().Y+8)))
                {
                    owner.Enemigo.restarVida(4);
                    if (luz) owner.Enemigo.restarVida(6);
                    disappear();
                    //owner.sacarPoder(this);
                }
            }
            //if(luz)  owner._fightGameManager.LightMeshes[numluz].Position = this.mesh.Position;
        }


        public void render(float elapsedTime)
    {
        mesh.animateAndRender();
        
    }
        public void renderbb(float elapsedTime)
        {

            mesh.BoundingBox.render();
            globalSphere.render();
        }
        public void dispose()
        {
            mesh.dispose();
            globalSphere.dispose();
            if (luz) owner.Luz = true;
        }
        public void disappear()
        {
            mesh.move(movementVector * 10000);
            globalSphere.moveCenter(movementVector * 10000);
            if(luz) owner.Luz = true;
           
        }
        public void setLight(int indice)
        {
            luz = true;
            numluz = indice;
            mesh.setColor(Color.Black);
            mesh.Scale = new Vector3(0.2f, 0.4f, 0.4f);
        }

    }
}
