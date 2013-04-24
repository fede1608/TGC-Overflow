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
    class Poder
    {

        public TgcKeyFrameMesh mesh;
        public TgcBoundingSphere globalSphere;
        public bool active;
        public bool powerhit;
        public Vector3 movementVector;
        FightGameManager fightGameManager = new FightGameManager();
        TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
        TgcKeyFrameLoader keyFrameLoader = new TgcKeyFrameLoader();
        
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
            movementVector = mov;
            mesh.rotateY(Geometry.DegreeToRadian(mov.X  > 0 ? 0f : -180f));
            globalSphere = new TgcBoundingSphere(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateAxisRadius().Y);
        }
        public void update(float elapsedTime)
        {
            mesh.move(movementVector * elapsedTime);
            globalSphere.moveCenter(movementVector * elapsedTime);
        
        }


        public void render(float elapsedTime)
    {
        mesh.animateAndRender();
    }
    }
}
