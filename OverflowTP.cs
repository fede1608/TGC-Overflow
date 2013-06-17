using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Collision.ElipsoidCollision;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.overflowDT
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class FightGameManager : TgcExample
    {
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Grupo Overflow";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "Juego de Pelea con efecto de Luces";
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>

          //Creo el sprite drawer
            Drawer spriteDrawer = new Drawer();
            Sprite newSprite; Sprite s_barrita1; Sprite s_barrita2, s_barritaP2,s_barritaP1;

        Personaje personaje1;
        Personaje personaje2;

        #region 4LUCES
        //luces
        Effect effect;
        TgcBox[] lightMeshes;
        InterpoladorVaiven interp;
        Vector3[] origLightPos;


        
        //FinLuces
        #endregion

        #region SONIDO
        //Sonido
        TgcMp3Player player = GuiController.Instance.Mp3Player;
        TgcStaticSound sound;   
        string musicFile;
        string soundFile=null;
        int match=0;
        TgcSprite banner;
        //Fin Sonido
        #endregion

        #region HEIGHTMAP
            //Inicialización de variables de HeightMap
            TgcSimpleTerrain terrain;
            string currentHeightmap;
            string currentTexture;
            float currentScaleXZ;
            float currentScaleY;
            //Fin de incicialización HM
        #endregion
            TgcBoundingBox bb_piso;
        List<Collider> objetosColisionables = new List<Collider>();
        List<Collider> objColtmp=new List<Collider>();
        List<Collider> objColtmp2 = new List<Collider>();
        ElipsoidCollisionManager collisionManager;

        TgcText2d clock1;
        TgcText2d clock2;
        int clock = 90;
        float time1=0;
        float time2 = 0;

        TgcSkyBox skyBox;
        TgcScene escenario;
        Size screenSize = GuiController.Instance.Panel3d.Size;
        float velocidadCaminar = 30f;
        bool gravity = true;
        bool gravity2= true;
        string animation2 = "Parado";
        bool estado=true;

        private string mediaMPath = GuiController.Instance.AlumnoEjemplosMediaDir + "OverflowDT\\";
        public string MediaMPath
        {
            get { return mediaMPath; }
            set { mediaMPath = value; }
        }
        private string textureMPath = GuiController.Instance.AlumnoEjemplosMediaDir + "OverflowDT\\" + "SkeletalAnimations\\Robot\\Textures\\";
        public string TextureMPath
        {
            get { return textureMPath; }
            set { textureMPath = value; }
        }
        private string mediaPath = GuiController.Instance.ExamplesMediaDir;
        public string MediaPath
        {
            get { return mediaPath; }
            set { mediaPath = value; }
        }
        private string texturePath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Textures\\";
        public string TexturePath
        {
            get { return texturePath; }
            set { texturePath = value; }
        }
        public List<Collider> ObjetosColisionables
        {
            get { return objetosColisionables; }
            set { objetosColisionables = value; }
        }
        public TgcBox[] LightMeshes
        {
            get { return lightMeshes; }
            set { lightMeshes = value; }
        }
        public override void init()
        {

            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
            TgcSceneLoader loader = new TgcSceneLoader();
           // escenario = loader.loadSceneFromFile(mediaMPath + "\\Escenario\\lvl01-TgcScene.xml");
            escenario = loader.loadSceneFromFile(mediaMPath + "\\Nivel_Militar\\lvl_01a2-TgcScene.xml");

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("velocidadX");
            GuiController.Instance.UserVars.addVar("camX");
            GuiController.Instance.UserVars.addVar("camY");
            GuiController.Instance.UserVars.addVar("camZ");
            GuiController.Instance.UserVars.addVar("Movement");
            GuiController.Instance.UserVars.addVar("Position");
            GuiController.Instance.UserVars.addVar("Vida1");
            GuiController.Instance.UserVars.addVar("Vida2");
            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("velocidadX", 0);
            GuiController.Instance.UserVars.setValue("camX", 0);
            GuiController.Instance.UserVars.setValue("camY", 0);
            GuiController.Instance.UserVars.setValue("camZ", 0);
            GuiController.Instance.UserVars.setValue("Movement", 0);
            GuiController.Instance.UserVars.setValue("Position", 0);
            GuiController.Instance.UserVars.setValue("Vida1", 100);
            GuiController.Instance.UserVars.setValue("Vida2", 100);

            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("distanciaCam", 1f, 300f, 40f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addBoolean("boundingbox", "Ver las Bounding Box y las B. Spheres", false);
            GuiController.Instance.Modifiers.addBoolean("IA/Pj2", "True para IA, False para Jugador 2",false);
             GuiController.Instance.Modifiers.addBoolean("estado", "Estado de la IA true ataque false defensa",true);
            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 500, 1500), 500);
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 0);

            
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            //GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1942, 9, -3257), new Vector3(0, 0, 0));
            //GuiController.Instance.FpsCamera.setCamera(new Vector3(10, 10, 10), new Vector3(0, 0, 0));
            //Configurar camara en estado inicial
            

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string skyboxPath = mediaMPath + "Ambiental\\Textures\\sky6\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, skyboxPath + "top.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, skyboxPath + "top.jpg");//En esta cara pongo cualquier textura, es necesario para que ande el programa y no puede no estar asignada
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, skyboxPath + "back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, skyboxPath + "front.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, skyboxPath + "left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, skyboxPath + "right.jpg");
            skyBox.updateValues();
            
            personaje1 = new Personaje();
            personaje1.Init();
            personaje1.setPosition(new Vector3(1900f, 0f, -3209f));
            personaje1.setRotation(Geometry.DegreeToRadian(270f));
            personaje1.Enemigo = personaje2;
            personaje2 = new Personaje();
            personaje2.Init();
            personaje2.setPosition(new Vector3(1956f, 0f, -3209f));
            personaje2.setRotation(Geometry.DegreeToRadian(90f));
            personaje2.Direccion = -1;
            
            //handler de evento de fin de animacion
            personaje1.mesh.AnimationEnds += new TgcSkeletalMesh.AnimationEndsHandler(mesh_AnimationEnds);
            personaje2.mesh.AnimationEnds += new TgcSkeletalMesh.AnimationEndsHandler(mesh_AnimationEnds2);
            personaje1.Enemigo = personaje2;
            personaje2.Enemigo = personaje1;
            personaje1._fightGameManager = this;
            personaje2._fightGameManager = this;
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3((personaje2.getPosition().X + personaje1.getPosition().X) / 2,
                                                                            (personaje2.getPosition().Y + personaje1.getPosition().Y) / 2,
                                                                            personaje2.getPosition().Z), 10, -40);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 12, 0);

            //sprites
            Bitmap barra = new Bitmap(mediaMPath  + "//barras4.png", GuiController.Instance.D3dDevice);
            Bitmap barrita1 = new Bitmap(mediaMPath + "//barrita.png", GuiController.Instance.D3dDevice);
            Bitmap poder1 = new Bitmap(mediaMPath + "//poder.png", GuiController.Instance.D3dDevice);
            Vector2 spriteSize = new Vector2(1100, 130);
            newSprite = new Sprite();
            newSprite.Bitmap = barra;
           
            newSprite.SrcRect = new Rectangle(-25, -10, (int)spriteSize.X, (int)spriteSize.Y);
            newSprite.Scaling = new Vector2(0.9f, 0.7f);

            s_barrita1 = new Sprite();
            s_barrita1.Bitmap = barrita1;
            s_barrita1.SrcRect = new Rectangle(0,0, 255, 30);
            s_barrita1.Scaling = new Vector2(1.32f, 0.72f);
            s_barrita1.Position = new Vector2((screenSize.Width * 0.105f), 41);

            s_barrita2 = new Sprite();
            s_barrita2.Bitmap = barrita1;
            s_barrita2.SrcRect = new Rectangle(0, 0, 255, 30);
            s_barrita2.Scaling = new Vector2(-1.32f, 0.72f);
            s_barrita2.Position = new Vector2(862, 41);

            s_barritaP1 = new Sprite();
            s_barritaP1.Bitmap = poder1;
            s_barritaP1.SrcRect = new Rectangle(0, 0, 510, 30);
            s_barritaP1.Scaling = new Vector2(0.2f, 0.32f);
            s_barritaP1.Position = new Vector2((screenSize.Width * 0.15f), 70);

            s_barritaP2 = new Sprite();
            s_barritaP2.Bitmap = poder1;
            s_barritaP2.SrcRect = new Rectangle(0, 0, 510, 30);
            s_barritaP2.Scaling = new Vector2(-0.2f, 0.32f);
            s_barritaP2.Position = new Vector2(820, 70);
            //fin sprites

            //texto
            clock1 = new TgcText2d();
            clock1.Text = "90";
            clock1.Color = Color.DarkBlue;
            clock1.Align = TgcText2d.TextAlign.LEFT;
            clock1.Position = new Point(463, 33);
            clock1.Size = new Size(300, 100);
            clock1.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            
            clock2 = new TgcText2d();
            clock2.Text = "90";
            clock2.Color = Color.Blue;
            clock2.Align = TgcText2d.TextAlign.LEFT;
            clock2.Position = new Point(460, 30);
            clock2.Size = new Size(300, 100);
            clock2.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            //Fin textos

            #region Modifiers HeightMap
            //Modificadores para la carga del HeightMap y su customización

            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = mediaMPath + "HeightMap\\" + "HM2JPG64_90IZQ.jpg";
            GuiController.Instance.Modifiers.addTexture("heightmap", currentHeightmap);

            //Modifiers para variar escala del mapa
            currentScaleXZ = 23.9960f;
            
            GuiController.Instance.Modifiers.addFloat("scaleXZ", 1f, 30f, currentScaleXZ);
            currentScaleY = 0.7f;
            GuiController.Instance.Modifiers.addFloat("scaleY", 0.1f, 2f, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = mediaMPath + "HeightMap\\Texturas\\" + "suelo_arena_lo_dark.jpg";
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture);


            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(80, -69, -110)); //X(Rojo, Y (Verde), Z(Azul)
            terrain.loadTexture(currentTexture);

            //TODO: Revisar como rotar el HM, esto puede servir: personaje2.setRotation(Geometry.DegreeToRadian(180f));

            #endregion

            #region 4LUCES
            //Shader Luces
            effect = TgcShaders.loadEffect(mediaMPath + "Shaders\\MultiDiffuseLights.fx");


            //Crear 4 mesh para representar las 4 para la luces. Las ubicamos en distintas posiciones del escenario, cada una con un color distinto.
            lightMeshes = new TgcBox[4];
            origLightPos = new Vector3[lightMeshes.Length];
            Color[] c = new Color[6] {Color.LightYellow, Color.Red, Color.Blue, Color.Green, Color.Yellow,Color.Violet };
            for (int i = 0; i < lightMeshes.Length; i++)
            {
                Color co = c[i % c.Length];
                lightMeshes[i] = TgcBox.fromSize(new Vector3(0.5f, 0.5f, 0.5f), co);
                
            }
            //origLightPos[0] = new Vector3(1880f, 70f, -3169f);//-40, 20 + i * 20, 400);
            //origLightPos[1] = new Vector3(1880f, 70f, -3249f);//-40, 20 + i * 20, 400);
            //origLightPos[2] = new Vector3(1960f, 70f, -3169f);//-40, 20 + i * 20, 400);
            //origLightPos[3] = new Vector3(1960f, 70f, -3249f);
            origLightPos[0] = new Vector3(1910f, 150f, -3199f);//-40, 20 + i * 20, 400);
            
            //Interpolador para mover las luces de un lado para el otro
            interp = new InterpoladorVaiven();
            interp.Min = -100f;
            interp.Max = 100f;
            interp.Speed = 50f;
            interp.Current = 0f;
            //Fin Luces
            #endregion
            
            //Almacenar volumenes de colision del escenario
           // objetosColisionables.Clear();
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                //Los objetos del layer "TriangleCollision" son colisiones a nivel de triangulo
                if (mesh.Name == "Room-1-Floor-0")
                {
                    objetosColisionables.Add(TriangleMeshCollider.fromMesh(mesh));
                    bb_piso = mesh.BoundingBox;
                }
                //El resto de los objetos son colisiones de BoundingBox. Las colisiones a nivel de triangulo son muy costosas asi que deben utilizarse solo
                //donde es extremadamente necesario (por ejemplo en el piso). El resto se simplifica con un BoundingBox
                else
                {
                   // objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
            }
            
            //objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(personaje2.mesh.BoundingBox));
           // objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(personaje2.Spheres.GlobalSphere));
            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;



            ///////////////LISTAS EN C#//////////////////
            //crear
            List<string> lista = new List<string>();

            //agregar elementos
            lista.Add("elemento1");
            lista.Add("elemento2");

            //obtener elementos
            string elemento1 = lista[0];

            //bucle foreach
            foreach (string elemento in lista)
            {
                //Loggear por consola del Framework
                GuiController.Instance.Logger.log(elemento);
            }

            //bucle for
            for (int i = 0; i < lista.Count; i++)
            {
                string element = lista[i];
            }


        }

        void mesh_AnimationEnds(TgcSkeletalMesh mesh)
        {
            //handler pj1
            if(personaje1.actions.punch&&!personaje1.mesh.PlayLoop)personaje1.actions.punch = false;
            if (personaje1.actions.kick && !personaje1.mesh.PlayLoop) personaje1.actions.kick = false;
            if (personaje1.actions.power && !personaje1.mesh.PlayLoop) personaje1.actions.power = false;
            personaje1.actions.hit = false;

        }
        void mesh_AnimationEnds2(TgcSkeletalMesh mesh)
        {
            //handler pj2
            if (personaje2.actions.punch && !personaje2.mesh.PlayLoop) personaje2.actions.punch = false;
            if (personaje2.actions.kick && !personaje2.mesh.PlayLoop) personaje2.actions.kick = false;
            if (personaje2.actions.power && !personaje2.mesh.PlayLoop) personaje2.actions.power = false;
            personaje2.actions.hit = false;
        }

        public void update(float elapsedTime)
        {
            if (time1 >= 1)
            {
                clock -= 1;
                if (clock > 0)
                {
                    clock1.Text = clock.ToString();
                    clock2.Text = clock.ToString();
                }
                if (personaje1.energia <= 97) { personaje1.energia += 3; }
                else { personaje1.energia = 100; }

                if (personaje2.energia <= 97) { personaje2.energia += 3; }
                else { personaje2.energia = 100; }
                
                
                time1 = 0;
            }

            s_barrita1.Scaling=new Vector2(1.32f * personaje1.life / 100,0.74f);
            s_barrita2.Scaling = new Vector2(-1.32f * personaje2.life / 100, 0.74f);

            s_barritaP1.Scaling = new Vector2(0.2f * personaje1.energia / 100, 0.32f);
            s_barritaP2.Scaling = new Vector2(-0.2f * personaje2.energia / 100, 0.32f);
        }
        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        

        public override void render(float elapsedTime)
        {
            time1 += elapsedTime;
            update(elapsedTime);

            #region SONIDO Y BANNERS
            //match = 0;//saca el sonido
            switch (match)
            {
                case 0://Inicia el combate
                    

                    //Cargo sonido y muestro el banner correspondiente
                    loadMp3(mediaMPath + "Music\\Final_Round.mp3");
                    if (player.getStatus() == TgcMp3Player.States.Open)
                    {
                        player.play(false);

                        //banner = new TgcSprite();
                        //banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\FinalRound.png");
                        //banner.Scaling = new Vector2(0.8f * ((float)screenSize.Width / 961f), 0.8f * ((float)screenSize.Height / 507f));
                        //banner.Position = new Vector2(screenSize.Width * 0.3f, screenSize.Height * 0.5f);
                    }
                    //Cuando termina el sonido anterior, 
                    //cargo el nuevo sonido y muestro el banner correspondiente
                    if (player.getStatus() == TgcMp3Player.States.Stopped)
                    {
                        //loadSound(mediaMPath + "Music\\Fight.wav");
                        //sound.play(false);

                        //banner.dispose();
                        //banner = new TgcSprite();
                        //banner.Texture = TgcTexture.createTexture(mediaMPath + "\\Banners\\Fight.png");
                        //banner.Scaling = new Vector2(((float)screenSize.Width / 961f), ((float)screenSize.Height / 507f));
                        //banner.Position = new Vector2(screenSize.Width * 0.35f, screenSize.Height * 0.5f);

                        //Reproduce Musica de combate é inicia la pelea
                        player.closeFile();
                        loadMp3(mediaMPath + "Music\\Reptile2.mp3");
                        player.play(true);
                        match = 1;
                    }

                    
                    break;
               
                default: //fin del combate
                    break;
            }
            #endregion

            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;
            //texts
            clock1.render();
            clock2.render();

            //sprites
            spriteDrawer.BeginDrawSprite();
            spriteDrawer.DrawSprite(newSprite);
            spriteDrawer.DrawSprite(s_barrita1);
            spriteDrawer.DrawSprite(s_barrita2);
            spriteDrawer.DrawSprite(s_barritaP1);
            spriteDrawer.DrawSprite(s_barritaP2);
            spriteDrawer.EndDrawSprite();
            //fsprites


            personaje1.update(elapsedTime);
            personaje2.update(elapsedTime);


            //objColtmp = objetosColisionables;
            //objColtmp.Add(BoundingBoxCollider.fromBoundingBox(personaje2.mesh.BoundingBox));

            #region Render HeightMap
            //Rutinas de renderización de HeightMap

            //Ver si cambio el heightmap
            string selectedHeightmap = (string)GuiController.Instance.Modifiers["heightmap"];
            if (currentHeightmap != selectedHeightmap)
            {
                //Volver a cargar el Heightmap
                currentHeightmap = selectedHeightmap;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(80, -66, -110)); //X(Rojo, Y (Verde), Z(Azul)
            }

            //Ver si cambio alguno de los valores de escala
            float selectedScaleXZ = (float)GuiController.Instance.Modifiers["scaleXZ"];
            float selectedScaleY = (float)GuiController.Instance.Modifiers["scaleY"];
            if (currentScaleXZ != selectedScaleXZ || currentScaleY != selectedScaleY)
            {
                //Volver a cargar el Heightmap
                currentScaleXZ = selectedScaleXZ;
                currentScaleY = selectedScaleY;
                terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(80, -66, -110)); //X(Rojo, Y (Verde), Z(Azul)
            }

            //Ver si cambio la textura del terreno
            string selectedTexture = (string)GuiController.Instance.Modifiers["texture"];
            if (currentTexture != selectedTexture)
            {
                //Volver a cargar el DiffuseMap
                currentTexture = selectedTexture;
                terrain.loadTexture(currentTexture);
            }

            //Renderizar terreno
            terrain.render();
            
            //Fin de Rutinas de HM
            #endregion

            #region 4LUCES
            /////LUCES
            Effect currentShader;
            String currentTechnique;
            currentShader = this.effect;
            currentTechnique = "MultiDiffuseLightsTechnique";
           
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                mesh.Effect = currentShader;
                mesh.Technique = currentTechnique;
            }
            //terrain.Effect = currentShader;
            //terrain.Technique = currentTechnique;
            
           // personaje1.mesh.Effect = currentShader;
            //personaje2.mesh.Effect = currentShader;

            //personaje1.mesh.Technique = currentTechnique;
            //personaje2.mesh.Technique = currentTechnique;

            Vector3 move = new Vector3(interp.update(), 0,0  );
            ColorValue[] lightColors = new ColorValue[lightMeshes.Length];
            Vector4[] pointLightPositions = new Vector4[lightMeshes.Length];
            float[] pointLightIntensity = new float[lightMeshes.Length];
            float[] pointLightAttenuation = new float[lightMeshes.Length];
            for (int i = 0; i < lightMeshes.Length; i++)
            {
                TgcBox lightMesh = lightMeshes[i];

                if (i == 2)
                {
                    //personaje1 luz
                    lightMesh.Position = personaje1.getPowerPosition();//origLightPos[i];// +Vector3.Scale(move, i + 1);
                    pointLightIntensity[i] = 100f;
                    pointLightAttenuation[i] = 0.50f;
                
                
                }
                else if (i == 3)
                {
                    //personaje2 luz
                    lightMesh.Position = personaje2.getPowerPosition();
                    pointLightIntensity[i] = 100f;
                    pointLightAttenuation[i] = 0.50f;

                }
                else
                {
                    //luz de ambiente
                    lightMesh.Position = origLightPos[i];
                    pointLightIntensity[i] = 30f;
                    pointLightAttenuation[i] = 0.40f;

                }
                lightColors[i] = ColorValue.FromColor(lightMesh.Color);
                pointLightPositions[i] = TgcParserUtils.vector3ToVector4(lightMesh.Position);
                
            }


            //Renderizar meshes
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                
                    //Cargar variables de shader
                    mesh.Effect.SetValue("lightColor", lightColors);
                    mesh.Effect.SetValue("lightPosition", pointLightPositions);
                    mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                    mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)Color.Black));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)Color.White));
                

                //Renderizar modelo
                mesh.render();
            }
            //terrain.Effect.SetValue("lightColor", lightColors);
            //terrain.Effect.SetValue("lightPosition", pointLightPositions);
            //terrain.Effect.SetValue("lightIntensity", pointLightIntensity);
            //terrain.Effect.SetValue("lightAttenuation", pointLightAttenuation);
            //terrain.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)Color.Black));
            //terrain.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)Color.White));
            //terrain.render();
            //Renderizar meshes de luz
            for (int i = 0; i < lightMeshes.Length; i++)
            {
                TgcBox lightMesh = lightMeshes[i];
                lightMesh.render();
            }


            ////////FINLUCES
            #endregion


            
            

            //Obtener valor de UserVar (hay que castear)
            //int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");


            //Obtener valores de Modifiers
            float distanciaCam = (float)GuiController.Instance.Modifiers["distanciaCam"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];
            //GuiController.Instance.RotCamera.CameraDistance = distanciaCam;

            ///////////////INPUT//////////////////
            String animation= "Parado";
            bool tiropoder = false;
            //Capturar Input teclado
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.G))
            {
               // personaje1.mesh.playAnimation("Pegar", false);
                personaje1.actions.punch = true;

                //Tecla G apretada

            }
            
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.H))
            {
                //personaje1.mesh.playAnimation("Patear", false);
                //Tecla H apretada
                personaje1.actions.kick = true;
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                //Tecla F apretada
               
                if (personaje1.energia >= 10)
                personaje1.actions.power = true;

            }
            //izquierda
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                personaje1.actions.moveForward = -velocidadCaminar * elapsedTime;
                personaje1.actions.moving = true;
                if (personaje1.Direccion == 1)
                {
                    animation = "CaminandoRev";
                    //personaje1.mesh.playAnimation("CaminandoRev", true);
                }
                else
                {
                    animation = "Caminando";
                    //personaje1.mesh.playAnimation("Caminando", true);
                }

                //personaje1.mesh.AutoUpdateBoundingBox = true;
            }
            //derecha
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                personaje1.actions.moveForward = velocidadCaminar * elapsedTime;
                personaje1.actions.moving = true;

                if (personaje1.Direccion == 1)
                {
                    animation = "Caminando";
                    //personaje1.mesh.playAnimation("Caminando", true);
                }
                else
                {
                    animation = "CaminandoRev";
                    //personaje1.mesh.playAnimation("CaminandoRev", true);
                }

                //personaje1.mesh.AutoUpdateBoundingBox = true;
            }
            //ninguna de las dos
            else
            {
                personaje1.actions.moveForward = 0;
                personaje1.actions.moving = false;
                //personaje1.mesh.AutoUpdateBoundingBox = false;
                //personaje1.mesh.playAnimation("Parado", true);
            }

            //saltar
            if ((GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.W)|| GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))&& !personaje1.actions.jumping)
            {
                
                personaje1.actions.jumping = true;
                collisionManager.GravityEnabled = false;
                gravity = false;
            }

            if (personaje1.actions.jumping && personaje1.getPosition().Y < 5)
            {
                personaje1.actions.jump += 5 * elapsedTime ;
            }
            else if (!gravity)
            {
                personaje1.actions.jump -= 5 * elapsedTime;
                gravity = personaje1.getPosition().Y <= 2;
                personaje1.actions.jumping = false;
            }
            else
            {
                personaje1.actions.jump = 0;
                personaje1.actions.jumping = false;
                //collisionManager.GravityEnabled = true;
            }
           
            //Animaciones big if
            if (personaje1.actions.power) { if (personaje1.mesh.PlayLoop & personaje1.energia >= 10) { personaje1.tirarPoder(); personaje1.mesh.playAnimation("Arrojar", false, 100); loadSound(mediaPath + "Sound\\ráfaga helada.wav"); sound.play(false); } }
            else
            {
                if (personaje1.actions.punch) {

                    if (personaje1.mesh.PlayLoop) { personaje1.mesh.playAnimation("Pegar", false, 50); loadSound(mediaPath + "Sound\\puñetazo.wav"); sound.play(false); }
                    personaje1.verificarColision(personaje1.Spheres.Bones["Bip01 L Hand"].bonesphere.Center, personaje1.Spheres.Bones["Bip01 L Hand"].bonesphere.Radius, personaje1.Enemigo.Spheres.Bones);
                }
                else
                {
                    if (personaje1.actions.kick) {
                        if (personaje1.mesh.PlayLoop) { personaje1.mesh.playAnimation("Patear", false, 60); loadSound(mediaPath + "Sound\\golpe sordo.wav"); sound.play(false); }
                        personaje1.verificarColision(personaje1.Spheres.Bones["Bip01 L Foot"].bonesphere.Center, personaje1.Spheres.Bones["Bip01 R Foot"].bonesphere.Radius, personaje1.Enemigo.Spheres.Bones);
                
                    }
                    else
                    {
                        if (personaje1.actions.moving) { personaje1.mesh.playAnimation(animation, true); }
                        else { if (!personaje1.actions.moving) { personaje1.mesh.playAnimation("Parado", true); } }
                    }
                }
            }




            bool IA = (bool)GuiController.Instance.Modifiers["IA/Pj2"];


            //player2
            //Capturar Input teclado

            //inicio pj2 manual
            
            if (!IA)
            {

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.K))
                {
                    //Tecla control right apretada
                    personaje2.actions.punch = true;


                } if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.L))
                {
                    //Tecla control right apretada
                    personaje2.actions.kick = true;

                }
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.RightControl))
                {
                    //Tecla control right apretada
                    if (personaje2.energia >= 10)
                    personaje2.actions.power = true;
                    //tiropoder = personaje2.tirarPoder();

                }
                //izquierda
                if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
                {
                    personaje2.actions.moveForward = -velocidadCaminar * elapsedTime;
                    personaje2.actions.moving = true;
                    if (personaje2.Direccion == -1)
                    {
                        animation2 = "Caminando";
                    }
                    else
                    {
                        animation2 = "CaminandoRev";
                    }
                }
                //derecha
                else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightArrow))
                {
                    personaje2.actions.moveForward = velocidadCaminar * elapsedTime;
                    personaje2.actions.moving = true;
                    if (personaje2.Direccion == -1)
                    {
                        animation2 = "CaminandoRev";
                    }
                    else
                    {
                        animation2 = "Caminando";

                    }

                }
                //ninguna de las dos
                else
                {
                    personaje2.actions.moveForward = 0;
                    personaje2.actions.moving = false;
                    //personaje2.mesh.playAnimation("Parado", true);
                }

                //saltar
                if ((GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.UpArrow) || GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.RightShift)) && !personaje2.actions.jumping)
                {
                    personaje2.actions.jumping = true;
                    //collisionManager.GravityEnabled = false;
                    gravity2 = false;
                }
            } //fin PJ2 manual
            else
                //Inicio IA
            {
                time2 += elapsedTime;
                Random random = new Random();
                int rndval1 = random.Next(0, 100);
                //estado = (bool)GuiController.Instance.Modifiers["estado"];
                float distancia = FastMath.Abs(personaje1.getPosition().X - personaje2.getPosition().X);
                
                // Se define si el estado de pelea es Ataque o Defensa, si es ataque el personaje intentara acercarse al pj1 a su vez de intentar golpearlo
                // Si esta en defensa intentara alejarse, esquivar los poderes y a su vez tirarle poder 
                // El estado se definira por la cantidad de vida que le queda empezando en estado de ataque y cuando le quede menos de 25% de vida pasara a defensa, a la vez que cuando pase a menos de 5% 
                // haya una posibilidad de pasar a ataque para jugarsela.
                if (time2 > 0.200f)
                {
                    time2 = 0;
                    if (estado)
                    {
                        //Ataque
                        //moverse al personaje
                        if (distancia > 6)
                        {
                            if (rndval1 < 30) { personaje2.actions.jumping = true; gravity2 = false; }
                            if (rndval1 < 70)
                            {
                                personaje2.actions.moveForward = personaje2.Direccion * velocidadCaminar * elapsedTime;
                                personaje2.actions.moving = true;
                                animation2 = "Caminando";
                            }
                            if (rndval1 > 80) { personaje2.actions.jumping = true; gravity2 = false; }
                            if (rndval1 >= 99 & personaje2.energia >= 10) { personaje2.actions.power = true; }
                            
                            if (rndval1 >= 70)
                            {
                                personaje2.actions.moveForward = -personaje2.Direccion * velocidadCaminar * elapsedTime;
                                personaje2.actions.moving = true;
                                animation2 = "CaminandoRev";
                            }

                        }
                        else
                        {
                            personaje2.actions.moveForward = 0;
                            personaje2.actions.moving = false;
                            if (rndval1 < 50) personaje2.actions.punch = true;
                            if (rndval1 >= 50 & rndval1 < 75) personaje2.actions.kick = true;
                            if (rndval1 >= 99 & personaje2.energia >= 10) { personaje2.actions.power = true; }//tiropoder = personaje2.tirarPoder(); }
                            if (rndval1 >= 75 & rndval1 < 95)
                            {
                                personaje2.actions.moveForward = personaje2.Direccion * velocidadCaminar * elapsedTime;
                                personaje2.actions.moving = true;
                                animation2 = "Caminando";
                            }
                        }

                    }
                    else
                    {
                        //Defensa
                        if (distancia < 35)
                        {
                            if (rndval1 >= 70)
                            {
                                personaje2.actions.moveForward = personaje2.Direccion * velocidadCaminar * elapsedTime;
                                personaje2.actions.moving = true;
                                animation2 = "Caminando";
                            }
                            if (rndval1 < 70)
                            {
                                personaje2.actions.moveForward = -personaje2.Direccion * velocidadCaminar * elapsedTime;
                                personaje2.actions.moving = true;
                                animation2 = "CaminandoRev";
                            }
                            if (rndval1 < 70 & distancia<6) personaje2.actions.punch = true;
                            //if (rndval1 >= 50 & rndval1 < 75) personaje2.actions.kick = true;
                            if (rndval1 >= 90 & distancia > 6 & distancia < 16 & personaje2.energia>=10) { personaje2.actions.power = true; }//tiropoder=personaje2.tirarPoder(); }

                        }
                        else
                        {
                            int posibSalto = 20;
                            if (personaje1.poder.Count >= 1) posibSalto = 55;
                            //si esta suficientemente lejos 
                            personaje2.actions.moveForward = 0;
                            personaje2.actions.moving = false;
                            if (rndval1 < 1) personaje2.actions.punch = true;
                            if (rndval1 >= 74 & rndval1 < 75) personaje2.actions.kick = true;
                            if (rndval1 >= 75 & personaje2.energia >= 10) { personaje2.actions.power = true; }//tiropoder = personaje2.tirarPoder(); }
                            if (rndval1 < posibSalto) { personaje2.actions.jumping = true; gravity2 = false; }

                        }
                        
                    }

                    //fuera de estado
                    if (personaje2.life < 25 & rndval1<50) estado = false;
                    if (distancia > 100) estado = true;
                    
                }
                
            }



            if (personaje2.actions.jumping && personaje2.getPosition().Y < 5)
            {
                personaje2.actions.jump += 5 * elapsedTime;
            }
            else if (!gravity2)
            {
                personaje2.actions.jump -= 5 * elapsedTime;
                gravity2 = personaje2.getPosition().Y <= 2;
                personaje2.actions.jumping = false;
            }
            else
            {
                personaje2.actions.jump = 0;
                personaje2.actions.jumping = false;
                //collisionManager.GravityEnabled = true;
            }

            if (personaje2.actions.power) { if (personaje2.mesh.PlayLoop & personaje2.energia >= 10) { personaje2.tirarPoder(); personaje2.mesh.playAnimation("Arrojar", false, 100); loadSound(mediaPath + "Sound\\ráfaga helada.wav"); sound.play(false); } }
            else
            {
                if (personaje2.actions.punch) {
                    if (personaje2.mesh.PlayLoop) { personaje2.mesh.playAnimation("Pegar", false, 50); loadSound(mediaPath + "Sound\\puñetazo.wav"); sound.play(false); }
                    personaje2.verificarColision(personaje2.Spheres.Bones["Bip01 L Hand"].bonesphere.Center, personaje2.Spheres.Bones["Bip01 L Hand"].bonesphere.Radius, personaje2.Enemigo.Spheres.Bones);
                }
                else
                {
                    if (personaje2.actions.kick) { 
                        if (personaje2.mesh.PlayLoop){ personaje2.mesh.playAnimation("Patear", false, 60); loadSound(mediaPath + "Sound\\golpe sordo.wav"); sound.play(false); }
                        personaje2.verificarColision(personaje2.Spheres.Bones["Bip01 L Foot"].bonesphere.Center, personaje2.Spheres.Bones["Bip01 R Foot"].bonesphere.Radius, personaje2.Enemigo.Spheres.Bones);
                
                    }
                    else
                    {
                        if (personaje2.actions.moving) { personaje2.mesh.playAnimation(animation2, true); }
                        else { if (!personaje2.actions.moving) { personaje2.mesh.playAnimation("Parado", true); } }
                    }
                }
            }

            //Capturar Input Mouse
            //if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            //{
            //    //Boton izq apretado
            //}

            skyBox.render();
            //Renderiza escenario
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                mesh.render();
                if (false)
                {
                    mesh.BoundingBox.render();
                }
            }
            //personaje1.setPosition(personaje1.getPosition() + new Vector3(personaje1.actions.moveForward, personaje1.actions.jump, 0f));
            //if (personaje1.actions.moving || personaje1.actions.jumping)
            //{

            //objColtmp.Clear();
            //objColtmp2.Clear();
            //objetosColisionables.ForEach(delegate(Collider obj)
            //{
            //    objColtmp.Add(obj);
            //    objColtmp2.Add(obj);

            //});
            //objColtmp.Add(BoundingBoxCollider.fromBoundingBox(personaje2.mesh.BoundingBox));
            //objColtmp2.Add(BoundingBoxCollider.fromBoundingBox(personaje1.mesh.BoundingBox));

                //Vector3 realMovement = collisionManager.moveCharacter(personaje1.Spheres.GlobalSphere, new Vector3 (personaje1.actions.moveForward,personaje1.actions.jump,0) , objColtmp);
                //Vector3 realMovement2 = collisionManager.moveCharacter(personaje2.Spheres.GlobalSphere, new Vector3(personaje2.actions.moveForward, personaje2.actions.jump, 0), objColtmp2);
           
            //proximo movimiento
            Vector3 realMovement =new Vector3 (personaje1.actions.moveForward,personaje1.actions.jump,0) ;
           Vector3 realMovement2 =new Vector3 (personaje2.actions.moveForward,personaje2.actions.jump,0) ;


           if (TgcCollisionUtils.testAABBAABB(personaje1.mesh.BoundingBox, bb_piso)) { realMovement.Y = 0; personaje1.actions.jump = 0; } //personaje1.setPosition(new Vector3(personaje1.mesh.Position.X,0,personaje1.mesh.Position.Z)); };
           if (TgcCollisionUtils.testAABBAABB(personaje2.mesh.BoundingBox, bb_piso)) { realMovement2.Y = 0; personaje2.actions.jump = 0; } //personaje2.setPosition(new Vector3(personaje2.mesh.Position.X, 0, personaje2.mesh.Position.Z)); };


               
            if (verificarColisionEntrePersonajes())
                {
                    //if (realMovement.X != 0)
                    //{
                        realMovement.X -= personaje1.movementVector.X*elapsedTime*personaje1.Direccion;
                        realMovement2.X -= personaje2.movementVector.X* elapsedTime*personaje2.Direccion;
                        //personaje1.Spheres.GlobalSphere.moveCenter(new Vector3(-realMovement.X, 0, 0));
                        //personaje2.Spheres.GlobalSphere.moveCenter(new Vector3(realMovement.X, 0, 0));
                        
                    //}else if (realMovement2.X != 0)
                    //{
                    //    realMovement2.X *= 0.5f;
                    //    realMovement.X = realMovement2.X;
                    //    personaje1.Spheres.GlobalSphere.moveCenter(new Vector3(realMovement2.X, 0, 0));
                    //    personaje2.Spheres.GlobalSphere.moveCenter(new Vector3(-realMovement2.X, 0, 0));
                    //}

                    
                }

            //personaje1.Spheres.GlobalSphere.moveCenter(realMovement);
            //personaje2.Spheres.GlobalSphere.moveCenter(realMovement2);

            personaje1.move(realMovement);
            personaje2.move(realMovement2);

            personaje1.render(elapsedTime);
            personaje2.render(elapsedTime);

            if ((bool)GuiController.Instance.Modifiers["boundingbox"])
            {
                personaje1.renderbb(elapsedTime);
                personaje2.renderbb(elapsedTime);

            }


            //settear uservars
            GuiController.Instance.UserVars.setValue("velocidadX", (personaje2.Direccion));
            GuiController.Instance.UserVars.setValue("camX", GuiController.Instance.ThirdPersonCamera.Position.X);
            GuiController.Instance.UserVars.setValue("camY", GuiController.Instance.ThirdPersonCamera.Position.Y);
            GuiController.Instance.UserVars.setValue("camZ", GuiController.Instance.ThirdPersonCamera.Position.Z);
            GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));
            GuiController.Instance.UserVars.setValue("Position", TgcParserUtils.printVector3(personaje1.getPosition()));
            GuiController.Instance.UserVars.setValue("Vida1", personaje1.life);
            GuiController.Instance.UserVars.setValue("Vida2", personaje2.life);

            float offsetforward = Math.Abs(personaje2.getPosition().X - personaje1.getPosition().X) / (-2) - (10+(personaje2.getPosition().X - personaje1.getPosition().X)/10);
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3((personaje2.getPosition().X + personaje1.getPosition().X) / 2,
                                                                           (personaje2.getPosition().Y + personaje1.getPosition().Y) / 2,
                                                                            personaje2.getPosition().Z),
                                                                            13, (offsetforward < -40 ? offsetforward : -40));

            if ((personaje1.getPosition().X - personaje2.getPosition().X) >= 0 && personaje1.Direccion != -1)
            {
                personaje1.Direccion = -1;
                personaje2.Direccion = 1;
                personaje1.setRotation(Geometry.DegreeToRadian(180f));
                personaje2.setRotation(Geometry.DegreeToRadian(180f));
            }
            else if ((personaje1.getPosition().X - personaje2.getPosition().X) < 0 && personaje1.Direccion != 1)
            {
                personaje1.Direccion = 1;
                personaje2.Direccion = -1;
                personaje2.setRotation(Geometry.DegreeToRadian(180f));
                personaje1.setRotation(Geometry.DegreeToRadian(180f));
            }

        }

        public bool verificarColisionEntrePersonajes()
        {
            //TgcBoundingSphere esfera1;
            //esfera1 = new TgcBoundingSphere();
            //esfera1.setValues(personaje1.Spheres.GlobalSphere.Center,personaje1.Spheres.GlobalSphere.Radius.Y)
            
            //if ((personaje1.Spheres.GlobalSphere.Radius.X + personaje2.Spheres.GlobalSphere.Radius.X) > (personaje1.Spheres.GlobalSphere.Center - personaje2.Spheres.GlobalSphere.Center).Length())
                if (TgcCollisionUtils.testAABBAABB(personaje1.mesh.BoundingBox, personaje2.mesh.BoundingBox))
            {
                foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par1 in personaje1.Spheres.Bones)
                {
                    
                        foreach (KeyValuePair<string, BoundingMultiSphere.Sphere> par2 in personaje2.Spheres.Bones)
                            if (TgcCollisionUtils.testSphereSphere(par1.Value.bonesphere, par2.Value.bonesphere))
                            {
                               return true;
                                
                            }
                       
                    
                }
            } return false;
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }
        #region Musica
        ///METODOS AUXILIARES
        
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
        #endregion
    }
}
