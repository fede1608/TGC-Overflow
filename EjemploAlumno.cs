using System;
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

namespace AlumnoEjemplos.Manoja
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
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

        public struct Player
        {
            public TgcSkeletalMesh mesh;
            public BoundingMultiSphere spheres;
            public Actions actions;
            public Vector3 movementVector;
            public Life life;
            public TgcText2d playername;
            public Power poder;
        }

        Player pj1;
        Player pj2;
        TgcScene escenario;
        List<TgcMesh> objectsBehind = new List<TgcMesh>();
        List<TgcMesh> objectsInFront = new List<TgcMesh>(); 
        TgcSkyBox skyBox;
        TgcSprite barras;
        TgcSprite banner;
        TgcMp3Player player = GuiController.Instance.Mp3Player;
        TgcStaticSound sound;
        float delayPJ2 = 0;
        float wallleft;
        float wallright;
        float floor;
        string musicFile;
        string soundFile;
        bool face = true;
        bool playerAI = true;
        int match;

        //Statics
        string mediaMPath = GuiController.Instance.AlumnoEjemplosMediaDir + "OverflowDT\\";
        string textureMPath = GuiController.Instance.AlumnoEjemplosMediaDir + "OverflowDT\\" + "SkeletalAnimations\\Robot\\Textures\\";
        string mediaPath = GuiController.Instance.ExamplesMediaDir;
        string texturePath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Textures\\";
        Size screenSize = GuiController.Instance.Panel3d.Size;
        Vector3 gravity = new Vector3(0f, -10f, 0f);
        float velocidadCaminar = 400f;
        int hitdelay = 1000;
        static int damagepunch = 2;
        static int damagekick = 5;
        static int damagepower = 10;
       
        #region Descripcion
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Grupo MANOJA";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "Trabajo Practico de Manoja - Ejemplo de juego de pelea";
        }
        #endregion

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

        #region Personajes - carga y seteo
            //Cargar escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(mediaMPath + "\\Ambiental\\CuartoLucha-TgcScene.xml");
            
            //Guardo los limites del escenario para el posterior calculo de colisiones
            floor = escenario.BoundingBox.PMin.Y + 5;
            wallleft = escenario.BoundingBox.PMin.X;
            wallright = escenario.BoundingBox.PMax.X; 
            
            //Cargar personajes con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            pj1.mesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                mediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                mediaPath + "SkeletalAnimations\\Robot\\",
                new string[] { 
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "CaminandoRev-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Parado-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Patear-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Pegar-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Cruz-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Lose-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Win-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Arrojar-TgcSkeletalAnim.xml",
                });

            pj2.mesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                mediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                mediaPath + "SkeletalAnimations\\Robot\\",
                new string[] { 
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "CaminandoRev-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Parado-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Patear-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Pegar-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Cruz-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Lose-TgcSkeletalAnim.xml",
                    mediaMPath + "SkeletalAnimations\\Robot\\" + "Win-TgcSkeletalAnim.xml",
                    mediaPath + "SkeletalAnimations\\Robot\\" + "Arrojar-TgcSkeletalAnim.xml",
                });

            //Cargar poderes con animacion
            TgcKeyFrameLoader keyFrameLoader = new TgcKeyFrameLoader();
            pj1.poder.mesh = keyFrameLoader.loadMeshAndAnimationsFromFile(
                mediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameMesh.xml",
                mediaMPath + "SkeletalAnimations\\Robot\\",
                new string[] { mediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameAnim.xml",});
            pj1.poder.mesh.Scale = new Vector3(3f,3f,3f);
            pj1.poder.mesh.Position = pj1.mesh.Position + new Vector3(0, -1000, 500);
            pj1.poder.movementVector = Vector3.Empty;
            pj1.poder.mesh.rotateY(Geometry.DegreeToRadian(((face ? 0f : -180f))));
            pj1.poder.globalSphere = new TgcBoundingSphere(pj1.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                           pj1.poder.mesh.BoundingBox.calculateAxisRadius().Y);

            pj2.poder.mesh = keyFrameLoader.loadMeshAndAnimationsFromFile(
                mediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameMesh.xml",
                mediaMPath + "SkeletalAnimations\\Robot\\",
                new string[] { mediaMPath + "SkeletalAnimations\\Robot\\" + "ball-TgcKeyFrameAnim.xml",});
            pj2.poder.mesh.Scale = new Vector3(3f, 3f, 3f);
            pj2.poder.mesh.Position = pj1.mesh.Position + new Vector3(0, -1000, -500);
            pj2.poder.movementVector = Vector3.Empty;
            pj2.poder.mesh.rotateY(Geometry.DegreeToRadian(((face ? 0f : -180f))));
            pj2.poder.globalSphere = new TgcBoundingSphere(pj2.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                           pj2.poder.mesh.BoundingBox.calculateAxisRadius().Y);

            //Asignamos las texturas y el color al 2do player
            pj1.mesh.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, texturePath + "uvw.jpg") });
            pj2.mesh.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, texturePath + "uvw.jpg") });
            pj2.mesh.setColor(Color.LightGreen);

            //Marcar posiciones iniciales
            pj1.mesh.Position = new Vector3(-300f, 6f, 0f);
            pj2.mesh.Position = new Vector3(300f, 6f, 0f);

            //Rotar de acuerdo a la posicion
            pj1.mesh.rotateY(Geometry.DegreeToRadian(270f));
            pj2.mesh.rotateY(Geometry.DegreeToRadian(90f));

            //Calculo sus BoundingMultiSphere
            pj1.spheres = new BoundingMultiSphere();
            pj2.spheres = new BoundingMultiSphere();
            pj1.spheres.getVerticesForBox(mediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml", pj1.mesh);
            pj2.spheres.getVerticesForBox(mediaPath + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml", pj2.mesh);

            //BoundingSphere global que van a usar los personajes
            pj1.mesh.AutoUpdateBoundingBox = false;
            pj1.spheres.GlobalSphere = new TgcBoundingSphere(pj1.mesh.BoundingBox.calculateBoxCenter(),
                                                             pj1.mesh.BoundingBox.calculateBoxRadius());
            pj2.mesh.AutoUpdateBoundingBox = false;
            pj2.spheres.GlobalSphere = new TgcBoundingSphere(pj2.mesh.BoundingBox.calculateBoxCenter(),
                                                             pj2.mesh.BoundingBox.calculateBoxRadius());
            
            //Seteo inicial de variables
            pj1.actions.jump = 0f;
            pj2.actions.jump = 0f;
            pj1.actions.hittimer = 0;
            pj2.actions.hittimer = 0;
            pj1.poder.powerhit = false;
            pj2.poder.powerhit = false;
            pj1.life.healthpoints = 100;
            pj2.life.healthpoints = 100;
            pj1.life.bloodypoints = 100;
            pj2.life.bloodypoints = 100;
            pj1.poder.combo = new float[3];
            setComboToZero();
        #endregion

        #region Sprites - carga y seteo
            //Carga estructura de barras
            barras = new TgcSprite();
            barras.Texture = TgcTexture.createTexture(mediaMPath + "\\Life\\Barra_de_vida.png");
            barras.Scaling = new Vector2(0.8f*((float)screenSize.Width / 961f), 0.8f*((float)screenSize.Height / 507f));
            barras.Position = new Vector2(screenSize.Width * 0.05f, screenSize.Height*0.05f);

            //Carga barra de vida personaje 1
            pj1.life.lifebar = new TgcSprite();
            pj1.life.lifebar.Texture = TgcTexture.createTexture(mediaMPath + "\\Life\\vida.png");
            pj1.life.lifebar.Scaling = new Vector2(0.54f * ((float)screenSize.Width / 961f), 0.59f * ((float)screenSize.Height / 507f));
            pj1.life.lifebar.Position = new Vector2(screenSize.Width * 0.11f, screenSize.Height * 0.085f);

            //Carga barra de efecto de daño personaje 1
            pj1.life.bloodybar = new TgcSprite();
            pj1.life.bloodybar.Texture = TgcTexture.createTexture(mediaMPath + "\\Life\\Blood.png");
            pj1.life.bloodybar.Scaling = new Vector2(0.54f * ((float)screenSize.Width / 961f), 0.59f * ((float)screenSize.Height / 507f));
            pj1.life.bloodybar.Position = new Vector2(screenSize.Width * 0.11f, screenSize.Height * 0.085f);

            //Carga barra de vida personaje 2
            pj2.life.lifebar = new TgcSprite();
            pj2.life.lifebar.Texture = TgcTexture.createTexture(mediaMPath + "\\Life\\vida.png");
            pj2.life.lifebar.Scaling = new Vector2(0.54f * ((float)screenSize.Width / 961f), 0.59f * ((float)screenSize.Height / 507f));
            pj2.life.lifebar.Position = new Vector2(screenSize.Width * 0.553f, screenSize.Height * 0.085f);

            //Carga barra de efecto de daño personaje 2
            pj2.life.bloodybar = new TgcSprite();
            pj2.life.bloodybar.Texture = TgcTexture.createTexture(mediaMPath + "\\Life\\Blood.png");
            pj2.life.bloodybar.Scaling = new Vector2(0.54f * ((float)screenSize.Width / 961f), 0.59f * ((float)screenSize.Height / 507f));
            pj2.life.bloodybar.Position = new Vector2(screenSize.Width * 0.553f, screenSize.Height * 0.085f);

            //Carga nombre personaje 1
            pj1.playername = new TgcText2d();
            pj1.playername.Text = "TGC-Robot";
            pj1.playername.Color = Color.White;
            pj1.playername.Align = TgcText2d.TextAlign.LEFT;
            pj1.playername.Position = new Point((int)Math.Round(screenSize.Width * 0.11), (int)Math.Round(screenSize.Height * 0.13));
            pj1.playername.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 12 * (float)screenSize.Width / 961f, FontStyle.Bold | FontStyle.Italic));

            //Carga nombre personaje 2
            pj2.playername = new TgcText2d();
            pj2.playername.Text = "GreenBot";
            pj2.playername.Color = Color.White;
            pj2.playername.Align = TgcText2d.TextAlign.LEFT;
            pj2.playername.Position = new Point((int)Math.Round(screenSize.Width * 0.75), (int)Math.Round(screenSize.Height * 0.13));
            pj2.playername.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 12 * (float)screenSize.Width / 961f, FontStyle.Bold | FontStyle.Italic));

            //Carga contador de vida personaje 1
            pj1.life.hpText = new TgcText2d();
            pj1.life.hpText.Text = "TGC-Robot";
            pj1.life.hpText.Color = Color.White;
            pj1.life.hpText.Align = TgcText2d.TextAlign.LEFT;
            pj1.life.hpText.Position = new Point((int)Math.Round(screenSize.Width * 0.07), (int)Math.Round(screenSize.Height * 0.085));
            pj1.life.hpText.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 12 * (float)screenSize.Width / 961f, FontStyle.Bold));

            //Carga contador de vida personaje 2
            pj2.life.hpText = new TgcText2d();
            pj2.life.hpText.Text = "TGC-Robot";
            pj2.life.hpText.Color = Color.White;
            pj2.life.hpText.Align = TgcText2d.TextAlign.LEFT;
            pj2.life.hpText.Position = new Point((int)Math.Round(screenSize.Width * 0.85), (int)Math.Round(screenSize.Height * 0.085));
            pj2.life.hpText.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 12 * (float)screenSize.Width / 961f, FontStyle.Bold));
        #endregion

            //Configurar animacion inicial
            pj1.mesh.playAnimation("Parado", true);
            pj2.mesh.playAnimation("Parado", true);
            
            //Configurar camara en estado inicial
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3((pj2.mesh.Position.X + pj1.mesh.Position.X) / 2,
                                                                           (pj2.mesh.Position.Y + pj1.mesh.Position.Y) / 2,
                                                                            pj2.mesh.Position.Z),
                                                               100, -400);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 120, 0);

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string skyboxPath = mediaMPath + "Ambiental\\Textures\\sky20\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, skyboxPath + "roof.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, skyboxPath + "floor.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, skyboxPath + "right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, skyboxPath + "left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, skyboxPath + "back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, skyboxPath + "front.jpg");
            skyBox.updateValues();

        #region GUIController Modifiers
            //Modifier para ver BoundingSpheres
            GuiController.Instance.Modifiers.addBoolean("GlobalSphere", "Show", false);
            GuiController.Instance.Modifiers.addBoolean("Multi-Sphere", "Show", false);

            //Modifiers para desplazamiento de camara
            GuiController.Instance.Modifiers.addFloat("Camera Offset", 200, 700, 400);

            //Modifier para activar la IA del 2do player
            GuiController.Instance.Modifiers.addBoolean("2nd Player AI", "Active", true);

            //UserVars con vector movimiento y posicion de los personajes
            GuiController.Instance.UserVars.addVar("Mov PJ1");
            GuiController.Instance.UserVars.addVar("Mov PJ2");
            GuiController.Instance.UserVars.addVar("Pos PJ1");
            GuiController.Instance.UserVars.addVar("Pos PJ2");
        #endregion

            match = 0;
            soundFile = null;
        }

        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener boolean para saber si hay que mostrar Bounding
            bool showGS = (bool)GuiController.Instance.Modifiers.getValue("GlobalSphere");
            bool showMS = (bool)GuiController.Instance.Modifiers.getValue("Multi-Sphere");
            //Obtener boolean para saber si hay que activar la IA
            playerAI = (bool)GuiController.Instance.Modifiers.getValue("2nd Player AI");

            switch(match)
            {
                case 0://Inicia el combate
            #region Initial Fight Set

                    //Cargo sonido y muestro el banner correspondiente
                    loadMp3(mediaMPath + "Music\\Final_Round.mp3");
                    if (player.getStatus() == TgcMp3Player.States.Open)
                    { 
                        player.play(false);

                        banner = new TgcSprite();
                        banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\FinalRound.png");
                        banner.Scaling = new Vector2(0.8f * ((float)screenSize.Width / 961f), 0.8f * ((float)screenSize.Height / 507f));
                        banner.Position = new Vector2(screenSize.Width * 0.3f, screenSize.Height * 0.5f);
                    }
                    //Cuando termina el sonido anterior, 
                    //cargo el nuevo sonido y muestro el banner correspondiente
                    if (player.getStatus() == TgcMp3Player.States.Stopped)
                    {
                        loadSound(mediaMPath + "Music\\Fight.wav");
                        sound.play(false);

                        banner.dispose();
                        banner = new TgcSprite();
                        banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\Fight.png");
                        banner.Scaling = new Vector2(((float)screenSize.Width / 961f), ((float)screenSize.Height / 507f));
                        banner.Position = new Vector2(screenSize.Width * 0.35f, screenSize.Height * 0.5f);

                        //Reproduce Musica de combate é inicia la pelea
                        player.closeFile();
                        loadMp3(mediaMPath + "Music\\Burning Battlefield.mp3");
                        player.play(true);
                        match = 1;
                    }
                
        #endregion
                    break;

                case 1://Combate
                    if ((pj1.actions.moving || pj1.actions.kick || pj1.actions.power || pj1.actions.punch ||
                         pj2.actions.moving || pj2.actions.kick || pj2.actions.power || pj2.actions.punch)&& banner != null)
                    {
                        banner.dispose();
                        banner = null;
                    }

            #region KeyButtonControl
                    //Calcular proxima posicion de personaje segun Input
                    TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
                    //Determina direccion de la vista del personaje
                    face = pj1.mesh.Rotation.Y > 3;
                    pj1.actions = setActionsForNextRender(pj1.actions.jump, pj1.actions.hittimer);

                        //Derecha
                        if (d3dInput.keyDown(Key.D))
                        {
                            pj1.actions.moveForward = velocidadCaminar * elapsedTime * (face ? -1 : 1);
                            pj1.actions.moving = true;
                            if (face)
                                pj1.poder.combo[1] = hitdelay * elapsedTime;
                            else
                            {
                                setComboToZero();
                            }
                        }

                        //Izquierda
                        if (d3dInput.keyDown(Key.A))
                        {
                            pj1.actions.moveForward = velocidadCaminar * elapsedTime * (face ? 1 : -1);
                            pj1.actions.moving = true;
                            if (!face)
                                pj1.poder.combo[1] = hitdelay * elapsedTime;
                            else
                            {
                                setComboToZero();
                            }
                        }

                        //Golpear o activar Poder
                        if (d3dInput.keyDown(Key.K))
                        {
                            pj1.poder.combo[2] = hitdelay * elapsedTime;

                            //si se apreta j mientras punching con k tira el poder by fede
                            if (comboReady() || d3dInput.keyPressed(Key.J))
                                pj1.actions.power = true;
                            else
                                pj1.actions.punch = true;
                        }

                        //Patada
                        if (d3dInput.keyDown(Key.L))
                        {
                            pj1.actions.kick = true;
                            setComboToZero();
                        }

                        //Inicia Combo de Poder
                        if (d3dInput.keyDown(Key.S))
                        {
                            pj1.poder.combo[0] = hitdelay * elapsedTime;
                        }

                        //Saltar
                        if (pj1.actions.jump == 0)
                        {

                            if (d3dInput.keyDown(Key.W))
                            {
                                setComboToZero();
                                //mod by fede Harcodeado, arreglar (Listo, Fixed, nah no anda, volvemos a harcodeado) Multiplica un valor por los fps (fps= 1/elapsedTime) para que el salto sea relativo a los fps de la maquina que lo corre
                                pj1.actions.jump = 15;// (float)0.05 * 1 / elapsedTime;

                                pj1.actions.moving = true;
                            }
                        }//Esta en el salto
                        else
                        {
                            setComboToZero();
                            //mod by fede
                            pj1.actions.jump -= 15.0f * (float)elapsedTime;

                            if (pj1.mesh.Position.Y < 8) pj1.actions.jump = 0;
                            pj1.actions.moving = (pj1.actions.jump != 0);
                        }

                        //IA del 2do Player
                        movePlayer2(elapsedTime);
                        if (pj2.actions.jump != 0)
                        {
                            //mod by fede
                            pj2.actions.jump -= 15.0f * (float)elapsedTime;

                            if (pj2.mesh.Position.Y + pj2.actions.jump < 10) pj2.actions.jump = 0;
                            pj2.actions.moving = (pj2.actions.jump != 0);
                        }
                        setComboDown(elapsedTime);
            #endregion

            #region Animations
                    //Si hubo desplazamiento
                    if (pj1.actions.moving)
                    {
                        //Activar animacion de caminando
                        if (face != (pj1.actions.moveForward > 0))
                            pj1.mesh.playAnimation((face ? "Caminando" : "CaminandoRev"), true);
                        else
                            pj1.mesh.playAnimation((!face ? "Caminando" : "CaminandoRev"), true);
                    }

                    
                    else
                    {
                        if (pj1.actions.punch)//Activar animacion de golpe
                            pj1.mesh.playAnimation("Pegar", false);
                        else
                        {
                            if (pj1.actions.kick)//Activar animacion de patada
                                pj1.mesh.playAnimation("Patear", false);
                            else
                            {
                                if (pj1.actions.power)
                                {//Activar animacion de arrojar poder y activar poder
                                    pj1.mesh.playAnimation("Arrojar", false);
                                    if (!pj1.poder.active)
                                    {
                                        pj1.poder.active = true;
                                        pj1.poder.movementVector = new Vector3(FastMath.Sin(pj1.mesh.Rotation.Y) *
                                                                                -velocidadCaminar, 0, 0);
                                        pj1.poder.mesh.Position = pj1.spheres.GlobalSphere.Center +
                                                                  new Vector3(pj1.spheres.GlobalSphere.Radius * (face ? 1 : -1), 0, 0);
                                        pj1.poder.mesh.rotateY(Geometry.DegreeToRadian(((face ? 0f : 180f))));
                                        pj1.poder.globalSphere = new TgcBoundingSphere(pj1.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                                                       pj1.poder.mesh.BoundingBox.calculateAxisRadius().Y);
                                        pj1.poder.mesh.playAnimation("Animation", true);
                                    }
                                }
                                else //Si no se esta moviendo ni golpeando, activar animacion de Parado
                                    pj1.mesh.playAnimation("Parado", true);
                            }
                        }
                    }

                    if (playerAI)
                    {
                        //Si hubo desplazamiento
                        if (pj2.actions.moving)
                        {
                            //Activar animacion de caminando
                            if (face != (pj2.actions.moveForward > 0))
                                pj2.mesh.playAnimation((face ? "Caminando" : "CaminandoRev"), true);
                            else
                                pj2.mesh.playAnimation((!face ? "Caminando" : "CaminandoRev"), true);
                        }

                        //Si no se esta moviendo, activar animacion de Parado
                        else
                        {
                            if (pj2.actions.punch)//Activar animacion de golpe
                                pj2.mesh.playAnimation("Pegar", false);
                            else
                            {
                                if (pj2.actions.kick)//Activar animacion de patada
                                    pj2.mesh.playAnimation("Patear", false);
                                else
                                {
                                    if (pj2.actions.power)
                                    {//Activar animacion de arrojar poder y activar poder
                                        pj2.mesh.playAnimation("Arrojar", false);
                                        if (!pj2.poder.active)
                                        {
                                            pj2.poder.active = true;
                                            pj2.poder.movementVector = new Vector3(FastMath.Sin(pj2.mesh.Rotation.Y) *
                                                                                    -velocidadCaminar, 0, 0);
                                            pj2.poder.mesh.Position = pj2.spheres.GlobalSphere.Center +
                                                                      new Vector3(pj2.spheres.GlobalSphere.Radius * (face ? -1 : 1), 0, 0);
                                            pj2.poder.mesh.rotateY(Geometry.DegreeToRadian(((face ? 180f : 0f))));
                                            pj2.poder.globalSphere = new TgcBoundingSphere(pj2.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                                                           pj2.poder.mesh.BoundingBox.calculateAxisRadius().Y);
                                            pj2.poder.mesh.playAnimation("Animation", true);
                                        }
                                    }
                                    else//Si no se esta moviendo ni golpeando, activar animacion de Parado
                                        pj2.mesh.playAnimation("Parado", true);
                                }
                            }
                        }//Si la IA esta desactivada, activar animacion de Parado
                    } else pj2.mesh.playAnimation("Parado", true);
            #endregion

            #region Movements Asign
                    //Vector de movimiento
                    pj1.movementVector = Vector3.Empty;
                    if (pj1.actions.moving)
                    {
                        //Aplicar movimiento, desplazarse en base a la direccion actual del personaje
                        pj1.movementVector = new Vector3(FastMath.Sin(pj1.mesh.Rotation.Y) * pj1.actions.moveForward, pj1.actions.jump, 0);
                    }
                    if (pj1.poder.active)
                    {//Aplicar movimiento al poder
                        pj1.poder.mesh.move(pj1.poder.movementVector * elapsedTime);
                        pj1.poder.globalSphere.moveCenter(pj1.poder.movementVector * elapsedTime);
                    }

                    //Vector de movimiento
                    pj2.movementVector = Vector3.Empty;
                    if (pj2.actions.moving)
                    {
                        //Aplicar movimiento, desplazarse en base a la direccion actual del personaje
                        pj2.movementVector = new Vector3(FastMath.Sin(pj2.mesh.Rotation.Y) * pj2.actions.moveForward * elapsedTime, pj2.actions.jump, 0);
                    }
                    if (pj2.poder.active)
                    {//Aplicar movimiento al poder
                        pj2.poder.mesh.move(pj2.poder.movementVector * elapsedTime);
                        pj2.poder.globalSphere.moveCenter(pj2.poder.movementVector * elapsedTime);
                    }
            #endregion

            #region Collisions & Hits
                    nextMoveCalculation(elapsedTime);

                    //Si el Player1 recibio un golpe, le aplico un tiempo de demora hasta que pueda recibir el siguiente golpe
                    if (pj1.actions.hit) pj1.actions.hittimer = (pj1.actions.hittimer <= 0 ? hitdelay  * elapsedTime : pj1.actions.hittimer);

                    if (pj1.actions.hittimer != 0)
                    {   //Si fue golpeado recien, disminuyo la vida de acuerdo al tipo de golpe
                        if (Math.Round(pj1.actions.hittimer,5) == Math.Round(hitdelay * elapsedTime,5))
                        {
                            pj1.mesh.setColor(Color.Red);
                            if (pj2.actions.punch)
                            {
                                pj1.life.healthpoints -= damagepunch;
                                loadSound(mediaMPath + "Music\\punch.wav");
                                sound.play(false);
                            }
                            else if (pj2.actions.kick)
                            {
                                pj1.life.healthpoints -= damagekick;
                                loadSound(mediaMPath + "Music\\Kick.wav");
                                sound.play(false);
                            }
                            else if (pj2.poder.powerhit)
                            {
                                pj1.life.healthpoints -= damagepower;
                                loadSound(mediaMPath + "Music\\Kick.wav");
                                sound.play(false);
                            }
                            pj2.poder.powerhit = false;
                            DisbandPower2();
                            if(pj1.life.healthpoints <= 0)
                            {
                                pj1.life.healthpoints = 0;
                                match = 3;
                            }
                        }
                        pj1.actions.hittimer -= 100 * elapsedTime;

                        if (pj1.actions.hittimer <= 0)
                        {   //Si expiro el tiempo
                            pj1.mesh.setColor(Color.White);
                            pj1.actions.hittimer = 0;
                        }
                    }

                    //Si el Player2 recibio un golpe, le aplico un tiempo de demora hasta que pueda recibir el siguiente golpe
                    if (pj2.actions.hit) pj2.actions.hittimer = (pj2.actions.hittimer == 0 ? hitdelay * elapsedTime : pj2.actions.hittimer);
                    
                    if (pj2.actions.hittimer != 0)
                    {   //Si fue golpeado recien, disminuyo la vida de acuerdo al tipo de golpe
                        if (Math.Round(pj2.actions.hittimer, 5) == Math.Round(hitdelay  * elapsedTime, 5))
                        {
                            pj2.mesh.setColor(Color.Red);
                            if (pj1.actions.punch)
                            {
                                pj2.life.healthpoints -= damagepunch;
                                loadSound(mediaMPath + "Music\\punch.wav");
                                sound.play(false);
                            }
                            else if (pj1.actions.kick)
                            {
                                pj2.life.healthpoints -= damagekick;
                                loadSound(mediaMPath + "Music\\Kick.wav");
                                sound.play(false);
                            }
                            else if (pj1.poder.powerhit)
                            {
                                pj2.life.healthpoints -= damagepower;
                                loadSound(mediaMPath + "Music\\Kick.wav");
                                sound.play(false);
                            }
                            pj1.poder.powerhit = false;
                            DisbandPower1();
                            if (pj2.life.healthpoints <= 0)
                            {
                                pj2.life.healthpoints = 0;
                                match = 2;
                            }
                        }
                        pj2.actions.hittimer -= 100 * elapsedTime;

                        if (pj2.actions.hittimer <= 0)
                        {   //Si expiro el tiempo
                            pj2.mesh.setColor(Color.LightGreen);
                            pj2.actions.hittimer = 0;
                        }
                    }
            #endregion

            #region Movements Aplication
                    //Mover personaje con detección de colisiones y gravedad
                    pj1.mesh.move(pj1.movementVector);
                    pj2.mesh.move(pj2.movementVector);
                    //Aplico movimiento a la esfera global de c/personaje
                    pj1.spheres.GlobalSphere.moveCenter(pj1.movementVector);
                    pj2.spheres.GlobalSphere.moveCenter(pj2.movementVector);
            #endregion
                    break;

                case 2://despues del combate - Victoria
                    pj1.mesh.playAnimation("Win", true);
                    pj2.mesh.playAnimation("Lose",true);

                    banner = new TgcSprite();
                    banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\Win.png");
                    banner.Scaling = new Vector2(0.8f * ((float)screenSize.Width / 961f), 0.8f * ((float)screenSize.Height / 507f));
                    banner.Position = new Vector2(screenSize.Width * 0.3f, screenSize.Height * 0.5f);

                    loadSound(mediaMPath + "Music\\YouWin.wav");
                    sound.play(false);
                    match = 4;
                    break;
                case 3://despues del combate - Derrota
                    pj1.mesh.playAnimation("Lose", true);
                    pj2.mesh.playAnimation("Win", true);

                    banner = new TgcSprite();
                    banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\Lose.png");
                    banner.Scaling = new Vector2(0.8f * ((float)screenSize.Width / 961f), 0.8f * ((float)screenSize.Height / 507f));
                    banner.Position = new Vector2(screenSize.Width * 0.3f, screenSize.Height * 0.5f);

                    loadSound(mediaMPath + "Music\\YouLose.wav");
                    sound.play(false);
                    match = 4;
                    break;
                default: //fin del combate
                    break;
            }
            //Hacer que la camara muestre a los personajes en su nueva posicion
            float offset = (float)GuiController.Instance.Modifiers.getValue("Camera Offset");
            GuiController.Instance.ThirdPersonCamera.Target = new Vector3((pj2.mesh.Position.X + pj1.mesh.Position.X) / 2,
                                                                          (pj2.mesh.Position.Y + pj1.mesh.Position.Y) / 2,
                                                                           pj2.mesh.Position.Z);
            GuiController.Instance.ThirdPersonCamera.OffsetForward = Math.Min(Math.Min(
                                                                                   Math.Abs(pj2.mesh.Position.X
                                                                                          - pj1.mesh.Position.X) * (-1),
                                                                                   Math.Abs(pj2.mesh.Position.Y
                                                                                          - pj1.mesh.Position.Y) * (-1.6f))
                                                                        , -offset);

            //Cargar desplazamientos realizados en UserVar
            GuiController.Instance.UserVars.setValue("Mov PJ1", TgcParserUtils.printVector3(pj1.movementVector));
            GuiController.Instance.UserVars.setValue("Mov PJ2", TgcParserUtils.printVector3(pj2.movementVector));
            GuiController.Instance.UserVars.setValue("Pos PJ1", TgcParserUtils.printVector3(new Vector3(
                                                                          FastMath.Floor(pj1.mesh.Position.X),
                                                                          FastMath.Floor(pj1.mesh.Position.Y),
                                                                          FastMath.Floor(pj1.mesh.Position.Z))));
            GuiController.Instance.UserVars.setValue("Pos PJ2", TgcParserUtils.printVector3(new Vector3(
                                                                          FastMath.Floor(pj2.mesh.Position.X),
                                                                          FastMath.Floor(pj2.mesh.Position.Y),
                                                                          FastMath.Floor(pj2.mesh.Position.Z))));
           
            //Renderiza escenario
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                mesh.render();
                if (showGS||showMS)
                {
                    mesh.BoundingBox.render();
                }
            }

        #region Mesh Rotation

            //Determina si hay que cambiar las direcciones de las vistas de los personajes
            if (pj1.mesh.Position.X > pj2.mesh.Position.X)
            {
                if (Math.Round(pj1.mesh.Rotation.Y, 4) != Math.Round(Geometry.DegreeToRadian(90f), 4))
                {
                    pj1.mesh.rotateY(Geometry.DegreeToRadian(-180f));
                    pj2.mesh.rotateY(Geometry.DegreeToRadian(180f));

                }
            }
            else
            {
                if (Math.Round(pj1.mesh.Rotation.Y, 4) != Math.Round(Geometry.DegreeToRadian(270f), 4))
                {
                    pj1.mesh.rotateY(Geometry.DegreeToRadian(180f));
                    pj2.mesh.rotateY(Geometry.DegreeToRadian(-180f));
                }
            }
        #endregion

        #region MultiSpheres Rotation
            //Aplico las transformaciones de las BoundingMultiSphere 
            // dependiendo de estado actual de los huesos
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj1.spheres.Bones)
            {
                Vector3 vr = pj1.mesh.getBoneByName(par.Key).StartPosition;
                Matrix transf = par.Value.offset
                              * pj1.mesh.getBoneByName(par.Key).MatFinal
                              * Matrix.RotationYawPitchRoll(pj1.mesh.Rotation.Y, 0, 0)
                              * Matrix.Translation(pj1.mesh.Position);

                par.Value.bonesphere.setCenter(Vector3.TransformCoordinate(vr, transf));
            }

            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj2.spheres.Bones)
            {
                Vector3 vr = pj2.mesh.getBoneByName(par.Key).StartPosition;
                Matrix transf = par.Value.offset
                              * pj2.mesh.getBoneByName(par.Key).MatFinal
                              * Matrix.RotationYawPitchRoll(pj2.mesh.Rotation.Y, 0, 0)
                              * Matrix.Translation(pj2.mesh.Position);

                par.Value.bonesphere.setCenter(Vector3.TransformCoordinate(vr, transf));
            }
        #endregion

        #region Renders Players & Skybox
            pj1.mesh.animateAndRender();
            if (showMS)
                foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj1.spheres.Bones)
                {
                    par.Value.bonesphere.render();
                }
            if (showGS) pj1.spheres.GlobalSphere.render();
            if (pj1.poder.active)
            { 
                pj1.poder.mesh.animateAndRender();
                if (showGS || showMS) pj1.poder.globalSphere.render();
            }

            pj2.mesh.animateAndRender();
            if (showMS)
                foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj2.spheres.Bones)
                {
                    par.Value.bonesphere.render();
                }
            if (showGS) pj2.spheres.GlobalSphere.render();
            if (pj2.poder.active)
            {
                pj2.poder.mesh.animateAndRender();
                if (showGS || showMS) pj2.poder.globalSphere.render();
            }

            //Render SkyBox
            skyBox.render();
        #endregion

        #region Renders Bars & Text
            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            if (pj1.life.bloodypoints > pj1.life.healthpoints) pj1.life.bloodypoints-= 0.2M;
            if (pj2.life.bloodypoints > pj2.life.healthpoints) pj2.life.bloodypoints-= 0.2M;
            pj1.life.bloodybar.Scaling = new Vector2((float)(pj1.life.bloodypoints / 100) * 0.54f * ((float)screenSize.Width / 961f), pj1.life.bloodybar.Scaling.Y);
            pj2.life.bloodybar.Scaling = new Vector2((float)(pj2.life.bloodypoints / 100) * 0.54f * ((float)screenSize.Width / 961f), pj2.life.bloodybar.Scaling.Y);
            pj1.life.bloodybar.render();
            pj2.life.bloodybar.render();

            pj1.life.lifebar.Scaling = new Vector2((float)(pj1.life.healthpoints / 100) * 0.54f * ((float)screenSize.Width / 961f), pj1.life.lifebar.Scaling.Y);
            pj2.life.lifebar.Scaling = new Vector2((float)(pj2.life.healthpoints / 100) * 0.54f * ((float)screenSize.Width / 961f), pj2.life.lifebar.Scaling.Y);
            pj1.life.lifebar.render();
            pj2.life.lifebar.render();

            pj1.life.hpText.Text = pj1.life.healthpoints.ToString();
            pj1.life.hpText.render();
            pj2.life.hpText.Text = pj2.life.healthpoints.ToString();
            pj2.life.hpText.render();
            pj1.playername.render();
            pj2.playername.render();
            barras.render();

            if (banner != null)
                banner.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();
        #endregion
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            pj1.life.bloodybar.dispose();
            pj1.life.lifebar.dispose();
            pj1.life.hpText.dispose();
            pj1.playername.dispose();
            pj1.poder.globalSphere.dispose();
            pj1.poder.mesh.dispose();
            pj1.spheres.GlobalSphere.dispose();
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj1.spheres.Bones)
                par.Value.bonesphere.dispose();
            pj1.mesh.dispose();

            pj2.life.bloodybar.dispose();
            pj2.life.lifebar.dispose();
            pj2.life.hpText.dispose();
            pj2.playername.dispose();
            pj2.poder.globalSphere.dispose();
            pj2.poder.mesh.dispose();
            pj2.spheres.GlobalSphere.dispose();
            foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par in pj2.spheres.Bones)
                par.Value.bonesphere.dispose();
            pj2.mesh.dispose();

            player.closeFile();
            if(sound != null)
                sound.dispose();
            escenario.disposeAll();
            skyBox.dispose();
        }
        
#region Metodos Auxiliares

        /// <summary>
        /// Resetea las acciones ya realizadas para poder hacer las siguientes
        /// </summary>
        /// <param name="jump">Salto actual del personaje</param>
        /// <param name="timer"> Tiempo de delay del golpe</param>
        public Actions setActionsForNextRender(float jump, float timer)
        {
            Actions acc = new Actions();
            acc.moving = false;
            acc.kick = false;
            acc.punch = false;
            acc.hit = false;
            acc.power = false;
            acc.hittimer = timer;
            acc.moveForward = 0f;
            acc.jump = jump;
            return acc;
        }

        /// <summary>
        /// Calcula los movimientos de los personajes de acuerdo a la deteccion de las colisiones
        /// </summary>
        /// <param name="elapsedTime"> Tiempo de entre frames</param>
        public void nextMoveCalculation(float elapsedTime)
        {
            string bonehit1 = String.Empty;
            string bonehit2 = String.Empty;

            //Chequeo el contacto/golpe entre los personajes
            if (TgcCollisionUtils.testSphereSphere(pj1.spheres.GlobalSphere, pj2.spheres.GlobalSphere))
            {
                foreach(KeyValuePair<string, BoundingMultiSphere.Sphere> par1 in pj1.spheres.Bones)
                {
                    if (TgcCollisionUtils.testSphereSphere(par1.Value.bonesphere, pj2.spheres.GlobalSphere))
                    { 
                        foreach(KeyValuePair<string, BoundingMultiSphere.Sphere> par2 in pj2.spheres.Bones)
                            if (TgcCollisionUtils.testSphereSphere(par1.Value.bonesphere, par2.Value.bonesphere))
                            {
                                bonehit1 = par1.Key;
                                bonehit2 = par2.Key;
                                break;
                            }
                        if (bonehit1 != String.Empty) break;
                    }
                }
            }

            //Chequeo el contacto del Poder1 contra el Player2
            if (pj1.poder.active)
            {
                if (TgcCollisionUtils.testSphereSphere(pj1.poder.globalSphere, pj2.spheres.GlobalSphere))
                {
                    foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par2 in pj2.spheres.Bones)
                        if (TgcCollisionUtils.testSphereSphere(pj1.poder.globalSphere, par2.Value.bonesphere))
                        {
                            pj1.poder.powerhit = true;
                            break;
                        }
                }
            }

            //Chequeo el contacto del Poder2 contra el Player1
            if (pj2.poder.active)
            {
                if (TgcCollisionUtils.testSphereSphere(pj2.poder.globalSphere, pj1.spheres.GlobalSphere))
                {
                    foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par1 in pj1.spheres.Bones)
                        if (TgcCollisionUtils.testSphereSphere(pj2.poder.globalSphere, par1.Value.bonesphere))
                        {
                            pj2.poder.powerhit = true;
                            break;
                        }
                }
            }

            //Diferencio entre si es un contacto o un golpe
            pj1.actions.hit = (pj2.poder.powerhit || (bonehit2.Contains("Hand") && pj2.actions.punch) 
                           || ((bonehit2.Contains("L Foot") || bonehit2.Contains("L Calf")) && pj2.actions.kick));
            pj2.actions.hit = (pj1.poder.powerhit || (bonehit1.Contains("Hand") && pj1.actions.punch)
                           || ((bonehit1.Contains("L Foot") || bonehit2.Contains("L Calf")) && pj1.actions.kick));

            //Aplico gravedad
            pj1.movementVector = pj1.movementVector + gravity;
            pj2.movementVector = pj2.movementVector + gravity;

            //Si hay contacto/golpe, aplica un retroceso entre ambos personajes
            if ((bonehit1 != String.Empty) || pj2.poder.powerhit)
                pj1.movementVector.X -= velocidadCaminar * elapsedTime * (face ? 1 : -1);
            if ((bonehit1 != String.Empty) || pj1.poder.powerhit)
                pj2.movementVector.X += velocidadCaminar * elapsedTime * (face ? 1 : -1);
            
            //Calculo del Personaje1 VS el suelo
            if (pj1.mesh.Position.Y + pj1.movementVector.Y < floor)
                pj1.movementVector.Y = pj1.movementVector.Y + (floor - (pj1.mesh.Position.Y + pj1.movementVector.Y));

            //Calculo del Personaje1 VS la pared derecha
            if (pj1.spheres.GlobalSphere.Center.X + pj1.movementVector.X > 
                wallright - Math.Truncate(pj1.spheres.GlobalSphere.Radius))
            {    
                pj1.movementVector.X = pj1.movementVector.X + ((pj1.mesh.Position.X)
                                    - (float)(wallright - Math.Truncate(pj1.spheres.GlobalSphere.Radius)))*120*elapsedTime;
                if (bonehit1 != String.Empty) pj2.movementVector.X += velocidadCaminar * elapsedTime * (2f) * (face ? 1 : -1);
            }

            //Calculo del Personaje1 VS la pared izquierda
            if (pj1.spheres.GlobalSphere.Center.X + pj1.movementVector.X < 
                wallleft + Math.Truncate(pj1.spheres.GlobalSphere.Radius))
            {
                pj1.movementVector.X = pj1.movementVector.X - (pj1.mesh.Position.X)
                                    + (float)(wallleft + Math.Truncate(pj1.spheres.GlobalSphere.Radius));
                if (bonehit1 != String.Empty) pj2.movementVector.X += velocidadCaminar * elapsedTime * (2f) * (face ? 1 : -1);
            }

            //Calculo del Personaje2 VS el suelo
            if (pj2.mesh.Position.Y + pj2.movementVector.Y < floor)
                pj2.movementVector.Y = pj2.movementVector.Y + (floor - (pj2.mesh.Position.Y + pj2.movementVector.Y));

            //Calculo del Personaje2 VS la pared derecha
            if (pj2.spheres.GlobalSphere.Center.X + pj2.movementVector.X > 
                wallright - Math.Truncate(pj2.spheres.GlobalSphere.Radius))
            {
                pj2.movementVector.X = pj2.movementVector.X + ((pj2.mesh.Position.X)
                                    - (float)(wallright - Math.Truncate(pj2.spheres.GlobalSphere.Radius))) * 120 * elapsedTime;
                if (bonehit2 != String.Empty) pj1.movementVector.X -= velocidadCaminar * elapsedTime * (2f) * (face ? 1 : -1);
            }

            //Calculo del Personaje2 VS la pared izquierda
            if (pj2.spheres.GlobalSphere.Center.X + pj2.movementVector.X < 
                wallleft + Math.Truncate(pj2.spheres.GlobalSphere.Radius))
            {
                pj2.movementVector.X = pj2.movementVector.X - (pj2.mesh.Position.X)
                                    + (float)(wallleft + Math.Truncate(pj2.spheres.GlobalSphere.Radius));
                if (bonehit2 != String.Empty) pj1.movementVector.X -= velocidadCaminar * elapsedTime * (2f) * (face ? 1 : -1);
            }

            //Calculo de Poder1 VS paredes
            if ((pj1.poder.globalSphere.Center.X < wallleft) || (pj1.poder.globalSphere.Center.X > wallright))
                DisbandPower1();
            

            //Calculo de Poder2 VS paredes
            if ((pj2.poder.globalSphere.Center.X < wallleft) || (pj2.poder.globalSphere.Center.X > wallright))
                DisbandPower2();

            //Calculo de Poder1 VS Poder2
            if (TgcCollisionUtils.testSphereSphere(pj1.poder.globalSphere, pj2.poder.globalSphere))
            {
                DisbandPower1();
                DisbandPower2();
            }
        }

        /// <summary>
        /// Desactiva el Poder1
        /// </summary>
        public void DisbandPower1()
        {
            pj1.poder.mesh.Position = new Vector3(0, -1000, 1000);
            pj1.poder.movementVector = Vector3.Empty;
            pj1.poder.mesh.rotateY(-pj1.poder.mesh.Rotation.Y);
            pj1.poder.globalSphere = new TgcBoundingSphere(pj1.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                           pj1.poder.mesh.BoundingBox.calculateAxisRadius().Y);
            pj1.poder.active = false;
        }

        /// <summary>
        /// Desactiva el Poder2
        /// </summary>
        public void DisbandPower2()
        {
            pj2.poder.mesh.Position = new Vector3(0, -1000, -1000);
            pj2.poder.movementVector = Vector3.Empty;
            pj2.poder.mesh.rotateY(-pj2.poder.mesh.Rotation.Y);
            pj2.poder.globalSphere = new TgcBoundingSphere(pj2.poder.mesh.BoundingBox.calculateBoxCenter(),
                                                           pj2.poder.mesh.BoundingBox.calculateAxisRadius().Y);
            pj2.poder.active = false;
        }

        /// <summary>
        /// Cargar un nuevo MP3 si hubo una variacion
        /// </summary>
        /// <param name="filePath"> Dirección del MP3 </param>
        public void loadMp3(string filePath)
        {
            if (musicFile == null || musicFile != filePath)
            {
                musicFile = filePath;

                //Cargar archivo
                GuiController.Instance.Mp3Player.closeFile();
                GuiController.Instance.Mp3Player.FileName = musicFile;
            }
        }

        /// <summary>
        /// Cargar un nuevo WAV si hubo una variacion
        /// </summary>
        /// <param name="filePath"> Dirección del WAV </param>
        public void loadSound(string filePath)
        {
            if (soundFile == null || soundFile != filePath)
            {
                soundFile = filePath;

                //Borrar sonido anterior
                if (sound != null)
                {
                    sound.dispose();
                    sound = null;
                }

                //Cargar sonido
                sound = new TgcStaticSound();
                sound.loadSound(soundFile);
            }
        }

        /// <summary>
        /// Pone a cero los contadores de tiempo de la combinación de activación del poder
        /// </summary>
        public void setComboToZero()
        {
            pj1.poder.combo[0] = 0;
            pj1.poder.combo[1] = 0;
            pj1.poder.combo[2] = 0;
        }

        /// <summary>
        /// Reduce los contadores de tiempo de la combinación de activación del poder
        /// </summary>
        /// <param name="elapsedTime"> Tiempo de entre frames</param>
        public void setComboDown(float elapsedtime)
        {
            pj1.poder.combo[0] -= (pj1.poder.combo[0] <= 0 ? 0 : 100 * elapsedtime);
            pj1.poder.combo[1] -= (pj1.poder.combo[1] <= 0 ? 0 : 100 * elapsedtime);
            pj1.poder.combo[2] -= (pj1.poder.combo[2] <= 0 ? 0 : 100 * elapsedtime);
        }

        /// <summary>
        /// Comprueba si se esta en condiciones de activar el poder
        /// </summary>
        public bool comboReady()
        {
            bool ret;
            ret = (pj1.poder.combo[0] > 0 && pj1.poder.combo[0] < pj1.poder.combo[1]
                && pj1.poder.combo[1] > 0 && pj1.poder.combo[1] < pj1.poder.combo[2]
                && pj1.poder.combo[2] > 0);
            return ret;
        }

        /// <summary>
        /// Inteligencia Artificial del 2do Player
        /// </summary>
        /// <param name="elapsedTime"> Tiempo de entre frames</param>
        public void movePlayer2(float elapsedTime)
        {
            if (playerAI)
            {
                int idx = (new Random()).Next(0, 7);

                if (delayPJ2 == 0)
                {
                    int delayplus;
                    pj2.actions = setActionsForNextRender(pj2.actions.jump, pj2.actions.hittimer);
                    switch (idx)
                    {
                        case 0:
                            pj2.actions.moveForward = velocidadCaminar * (face ? 1 : -1);
                            pj2.actions.moving = true;
                            delayplus = 10;
                            break;
                        case 1:
                            pj2.actions.moveForward = velocidadCaminar * (face ? -1 : 1);
                            pj2.actions.moving = true;
                            delayplus = 10;
                            break;
                        case 2:
                            pj2.actions.punch = true;
                            delayplus = 5;
                            break;
                        case 3:
                            pj2.actions.power = true;
                            delayplus = 7;
                            break;
                        case 4:
                            pj2.actions.kick = true;
                            delayplus = 7;
                            break;
                        case 5:
                            pj2.actions.moveForward = velocidadCaminar * (face ? 1 : -1);
                            pj2.actions.moving = true;
                            delayplus = 10;
                            break;
                        case 6:
                            if (pj2.actions.jump == 0)
                            {
                                //mod by fede Harcodeado, arreglar (Listo, Fixed) Multiplica un valor por los fps (fps= 1/elapsedTime) para que el salto sea relativo a los fps de la maquina que lo corre
                                pj2.actions.jump = 15;// (float)0.05 * 1 / elapsedTime;

                                pj2.actions.moveForward = velocidadCaminar * (face ? -1 : 1);
                                pj2.actions.moving = true;
                            }
                            delayplus = 1;
                            break;
                        default:
                            if (pj2.actions.jump == 0)
                            {
                                pj2.actions.jump = 15;
                                pj2.actions.moving = true;
                            }
                            delayplus = 1;
                            break;
                    }
                    
                    delayPJ2 = hitdelay * elapsedTime * delayplus*(350/60);
                }

                delayPJ2--;
                delayPJ2 = (delayPJ2 < 0 ? 0 : delayPJ2);
            }
            else pj2.actions = setActionsForNextRender(pj2.actions.jump, pj2.actions.hittimer);
        }
#endregion
    }
}
