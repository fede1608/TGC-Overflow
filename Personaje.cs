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
    class Personaje
    {
        //ElipsoidCollisionManager collisionManager;
        List<Collider> objCol = new List<Collider>();
        public TgcSkeletalMesh mesh;
        public Actions actions;
        public Vector3 movementVector=new Vector3 (30,0,0);
        public int life=100;
        public int energia = 100;
        private string playername = "TGC Player";
        Personaje enemigo;
        public List<Poder> poder = new List<Poder>();
        FightGameManager fightGameManager = new FightGameManager();
        TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
        TgcKeyFrameLoader keyFrameLoader = new TgcKeyFrameLoader();
        int direccion = 1;
        Device d3dDevice = GuiController.Instance.D3dDevice;
        BoundingMultiSphere spheres = new BoundingMultiSphere();
        bool luz=true;
        Poder poderLuz;
        bool golpeado = false;
        float timer = 0;
        int cont = 0;
        public Color colorPj = Color.White;
        //Getters y Setters
        public FightGameManager _fightGameManager
        {
            get { return fightGameManager; }
            set { fightGameManager = value; }
        }

        public BoundingMultiSphere Spheres
        {
            get { return spheres; }
            set { spheres = value; }
        }
        public string Playername
        {
            get { return playername; }
            set { playername = value; }
        }
        public bool Luz
        {
            get { return luz; }
            set { luz = value; }
        }
           
        public Personaje Enemigo
        {
            get { return enemigo; }
            set { enemigo = value; }
        }
        public int Direccion
        {
            get { return direccion; }
            set { direccion = value; }
        }
        
        public List<Collider> ObjCol
        {
            get { return objCol; }
            set { objCol = value; }
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
            public bool win;
            public Vector3 ptoGolpe;
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
           spheres.getVerticesForBox(fightGameManager.MediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml", mesh,0.05f);
           
           spheres.GlobalSphere = new TgcElipsoid(mesh.BoundingBox.calculateBoxCenter(),new Vector3(2,mesh.BoundingBox.calculateBoxRadius(),2));
           //spheres.GlobalSphere = new TgcBoundingSphere(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius());
           actions.jump = 0f;
           actions.hittimer = 0;
           actions.hit = false;
           actions.win = false;
           actions.ptoGolpe = mesh.Position;
           spheres.Bones["Bip01 Neck"].bonesphere.setValues(spheres.Bones["Bip01 Neck"].bonesphere.Center,0.6f);
           mesh.AutoUpdateBoundingBox = true;
           //Configurar animacion inicial
           mesh.playAnimation("Parado", true);
          





       }

        public void metodoEjemplo(int variable)
        {
        //comment
            
        }
        public void sacarPoder(Poder pow)
        {
            poder.Remove(pow);
            pow.dispose();

        }
        public void restarVida(int vida)
        {
            move(new Vector3(-direccion*1.5f,0,0));
            golpeado=true;
            if (vida >= 0) life-=vida;
            if (life < 0) life = 0;
        }
        public void setEffect(Effect ef)
        {
            //mesh.Effect = ef;

        }
        public void move(Vector3 movement)
        {
            //if (movement != new Vector3 (0,0,0))
                mesh.move(movement);
                spheres.GlobalSphere.moveCenter(movement);
            //controlar colisión con el Suelo
            if (mesh.Position.Y < 0) 
            { 
                mesh.move(new Vector3(0, -mesh.Position.Y, 0)); 
                spheres.GlobalSphere.moveCenter(new Vector3(0, -mesh.Position.Y, 0)); 
            }
            //Controlar que No se se pase de los limites laterales 
            if (mesh.Position.X > 1993) 
            { 
                mesh.move(new Vector3(1993 - mesh.Position.X, 0, 0)); 
                spheres.GlobalSphere.moveCenter(new Vector3(1993 - mesh.Position.X, 0, 0));
                enemigo.mesh.move(new Vector3(1993 - mesh.Position.X, 0, 0));
                enemigo.Spheres.GlobalSphere.moveCenter(new Vector3(1993 - mesh.Position.X, 0, 0)); 
            }
            if (mesh.Position.X < 1880)
            {
                mesh.move(new Vector3(1880-mesh.Position.X, 0, 0));
                spheres.GlobalSphere.moveCenter(new Vector3(1880-mesh.Position.X, 0, 0));
                enemigo.mesh.move(new Vector3(1880-mesh.Position.X, 0, 0));
                enemigo.Spheres.GlobalSphere.moveCenter(new Vector3(1880-mesh.Position.X, 0, 0));
            }
            
            //GuiController.Instance.UserVars.setValue("camZ", mesh.BoundingBox.PMax.X);   
            //mesh.Position += movement;
            //mesh.createBoundingBox();
            //mesh.BoundingBox.setExtremes(new Vector3(-1, 6.2f, -1) + movement , movement  + new Vector3(1, 0, 1));
        }
        public bool tirarPoder()
        {
            if (energia >= 10)
            {
                Poder pow = new Poder();
                pow.Init(direccion, mesh.Position + new Vector3(0, 3, 0), new Vector3(40, 0, 0));
                pow.Owner = this;
                if (luz & energia >= 50) { luz = false; pow.setLight(0); poderLuz = pow; energia -= 50; }
                else energia -= 10;
                poder.Add(pow);
                //actions.power = false;
                return true;
            }

            return false;
        }

        public Vector3 getPowerPosition()
        {
            if (luz) return new Vector3(0, 0, 0);
            return poderLuz.mesh.Position;
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
            GuiController.Instance.UserVars.setValue("camZ", mesh.BoundingBox.PMax);   
            //mesh.BoundingBox.setExtremes(new Vector3(-1, 6.2f, -1) + vec3, vec3 + new Vector3(1, 0, 1));
            spheres.GlobalSphere = new TgcElipsoid(mesh.BoundingBox.calculateBoxCenter(), new Vector3(2, mesh.BoundingBox.calculateBoxRadius(), 2));
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
         //direccion = (radianes == Geometry.DegreeToRadian(270f)) ? 1 : -1; ;
        }


        public bool verificarColision(Vector3 centro, float radio, Dictionary<string, BoundingMultiSphere.Sphere> huesos)
        {
            if (actions.hit) return false;
            Vector3 distancia;
            //GuiController.Instance.Logger.log("Radio=");
            //GuiController.Instance.Logger.log(radio.ToString());
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in huesos)
            {
                
                distancia = centro - par.Value.bonesphere.Center;
                if ((distancia.Length() <= radio + par.Value.bonesphere.Radius+0.3) && (par.Key != "Bip01 Neck"))
                {
                    enemigo.restarVida(2);
                    actions.hit = true;
                    actions.ptoGolpe = Vector3.Normalize(distancia) * par.Value.bonesphere.Radius;
                    Enemigo.actions.ptoGolpe = actions.ptoGolpe;
                    return true;
                    //if (actions.punch && !mesh.PlayLoop) actions.punch = false;
                    //if (actions.kick && !mesh.PlayLoop) actions.kick = false;
                    
                }
                //GuiController.Instance.Logger.log(par.Key);
                //GuiController.Instance.Logger.log(par.Value.bonesphere.Radius.ToString());
            }
            return false;
        }
        public void update(float elapsedTime)
        {
            
            if (golpeado)
            {
                timer += elapsedTime;
                if (timer > 0.200f)
                {
                    cont++;
                    if (Math.IEEERemainder((double)cont, 2) == 0)
                    {
                        setColor(Color.Red);
                    }else 
                        setColor(colorPj);

                }
                if (cont > 16)
                {
                    cont = 0;
                    golpeado = false;
                    setColor(colorPj);
                }
            }
            //mesh.move(movementVector * elapsedTime);
            //globalSphere.moveCenter(movementVector * elapsedTime);

           // Vector3 realMovement = collisionManager.moveCharacter(spheres.GlobalSphere, movementVector, fightGameManager.ObjetosColisionables);
           // mesh.move(realMovement);
           // mesh.BoundingBox.Position = vec3;
            //spheres.GlobalSphere.moveCenter((mesh.Position.X - spheres.GlobalSphere.Position.X,); //= new TgcElipsoid(mesh.BoundingBox.calculateBoxCenter(), new Vector3(2, mesh.BoundingBox.calculateBoxRadius(), 2));
           
            objCol.Clear();
            objCol.Add(BoundingBoxCollider.fromBoundingBox(enemigo.mesh.BoundingBox));
            
            foreach (Poder pow in poder)
            {
                pow.update(elapsedTime);
                
            }
            //remover poderes fuera del rango del escenario
            poder.RemoveAll(
                delegate(Poder pow)
                {
                    if (pow.globalSphere.Center.X > 2100 || pow.globalSphere.Center.X < 1790)
                    {
                        pow.dispose();
                        return true;
                    }
                    else
                        return false;
                }
                );
            //update multispheres
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in spheres.Bones)
            {
                Vector3 vr = mesh.getBoneByName(par.Key).StartPosition;
                
                //mesh.getBoneByName(par.Key).MatFinal.Scale(new Vector3(0.05f, 0.05f, 0.05f));
                Matrix transf =  par.Value.offset
                              * mesh.getBoneByName(par.Key).MatFinal
                              * Matrix.RotationYawPitchRoll(mesh.Rotation.Y, 0, 0)  
                              * Matrix.Translation(mesh.Position);
                //transf.Scale(new Vector3(0.05f, 0.05f, 0.05f));
                Vector3 center = Vector3.TransformCoordinate(vr, transf);
                //magic happens in your brain while pooping 
                center = center - mesh.Position;
                center = center * 0.05f;
                if(actions.moving&&( par.Key=="Bip01 R Hand"||par.Key=="Bip01 L Hand"||par.Key=="Bip01 R Forearm"||par.Key=="Bip01 L Forearm"||par.Key=="Bip01 R UpperArm"||par.Key=="Bip01 L UpperArm")) center.Z *= -1.0f;
                if ((actions.power || actions.punch) && (par.Key == "Bip01 L Hand")) { center.X *= 0.6f; center.Y *= 1.3f; }
                
                center = center + mesh.Position;
                //here ends magic
                par.Value.bonesphere.setCenter(center);
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
        public void renderbb(float elapsedTime)
        {

            spheres.GlobalSphere.render();
            mesh.BoundingBox.render();
            foreach (Poder pow in poder)
            {
                pow.renderbb(elapsedTime);

            }
            //render multispheres
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in spheres.Bones)
            {
                par.Value.bonesphere.render();
            }

        }
    }
}
