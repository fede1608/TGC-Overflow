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
        Vector3 realmove;
        
        public Personaje Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        Device d3dDevice = GuiController.Instance.D3dDevice;
        BoundingMultiSphere spheres = new BoundingMultiSphere();
        public void Init(int direccion, Vector3 pos, Vector3 mov)
        {
            mesh = keyFrameLoader.loadMeshAndAnimationsFromFile(
                   fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameMesh.xml",
                   fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\",
                   new string[] { fightGameManager.MediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameAnim.xml", });
            mesh.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            mesh.Position = pos;
            movementVector = mov*direccion;
            mesh.rotateY(Geometry.DegreeToRadian(direccion==1? 0f : -180f));
            globalSphere = new TgcElipsoid(mesh.BoundingBox.calculateBoxCenter(), new Vector3(mesh.BoundingBox.calculateAxisRadius().Y,mesh.BoundingBox.calculateAxisRadius().Y,mesh.BoundingBox.calculateAxisRadius().Y));
        }
        public void update(float elapsedTime)
        {
            //owner.Sem.WaitOne();
            realmove = collisionManager.moveCharacter(globalSphere, (movementVector * elapsedTime), owner.ObjCol);
            
            //owner.Sem.Release();
            mesh.move(movementVector * elapsedTime);
            //globalSphere.moveCenter(realmove);
            if (collisionManager.Result.collisionFound)
            {
                owner.Enemigo.restarVida(4);
                owner.sacarPoder(this);
            }
        }


        public void render(float elapsedTime)
    {
        mesh.animateAndRender();
        mesh.BoundingBox.render();
        globalSphere.render();
    }
        public void dispose()
        {
            mesh.dispose();
            globalSphere.dispose();
        }
    }
}
