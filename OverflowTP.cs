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

        Personaje personaje1;
        Personaje personaje2;

        //luces
        Effect effect;
        TgcBox[] lightMeshes;
        InterpoladorVaiven interp;
        Vector3[] origLightPos;

        //FinLuces

        List<Collider> objetosColisionables = new List<Collider>();
        ElipsoidCollisionManager collisionManager;

        TgcSkyBox skyBox;
        TgcScene escenario;
        Size screenSize = GuiController.Instance.Panel3d.Size;
        float velocidadCaminar = 30f;
        int hitdelay = 1000;
        static int damagepunch = 2;
        static int damagekick = 5;
        static int damagepower = 10;
        bool gravity = true;

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
        public override void init()
        {

            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
            TgcSceneLoader loader = new TgcSceneLoader();
           // escenario = loader.loadSceneFromFile(mediaMPath + "\\Escenario\\lvl01-TgcScene.xml");
            escenario = loader.loadSceneFromFile(mediaMPath + "\\Nivel_Militar\\lvl_01a-TgcScene.xml");

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("velocidadX");
            GuiController.Instance.UserVars.addVar("camX");
            GuiController.Instance.UserVars.addVar("camY");
            GuiController.Instance.UserVars.addVar("camZ");
            GuiController.Instance.UserVars.addVar("Movement");
            GuiController.Instance.UserVars.addVar("Position");
            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("velocidadX", 0);
            GuiController.Instance.UserVars.setValue("camX", 0);
            GuiController.Instance.UserVars.setValue("camY", 0);
            GuiController.Instance.UserVars.setValue("camZ", 0);
            GuiController.Instance.UserVars.setValue("Movement", 0);
            GuiController.Instance.UserVars.setValue("Position", 0);

            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("distanciaCam", 1f, 300f, 40f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));



            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 500, 1500), 500);
           // GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 0);

            
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            //GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1942, 9, -3257), new Vector3(0, 0, 0));
            //Configurar camara en estado inicial
            

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string skyboxPath = mediaMPath + "Ambiental\\Textures\\sky26\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, skyboxPath + "roof.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, skyboxPath + "floor.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, skyboxPath + "back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, skyboxPath + "front.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, skyboxPath + "left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, skyboxPath + "right.jpg");
            skyBox.updateValues();

            personaje1 = new Personaje();
            personaje1.Init();
            personaje1.setPosition(new Vector3(1900f, 0.5f, -3209f));
            personaje1.setRotation(Geometry.DegreeToRadian(270f));

            personaje2 = new Personaje();
            personaje2.Init();
            personaje2.setPosition(new Vector3(1956f, 0.5f, -3209f));
            personaje2.setRotation(Geometry.DegreeToRadian(90f));
            
            
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3((personaje2.getPosition().X + personaje1.getPosition().X) / 2,
                                                                           (personaje2.getPosition().Y + personaje1.getPosition().Y) / 2,
                                                                            personaje2.getPosition().Z),
                                                               10, -40);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 12, 0);







            //Luces


            effect = TgcShaders.loadEffect(mediaMPath + "Shaders\\MultiDiffuseLights.fx");


            //Crear 4 mesh para representar las 4 para la luces. Las ubicamos en distintas posiciones del escenario, cada una con un color distinto.
            lightMeshes = new TgcBox[4];
            origLightPos = new Vector3[lightMeshes.Length];
            Color[] c = new Color[5] { Color.Red, Color.Blue, Color.Green, Color.Yellow,Color.Violet };
            for (int i = 0; i < lightMeshes.Length; i++)
            {
                Color co = c[i % c.Length];
                lightMeshes[i] = TgcBox.fromSize(new Vector3(5, 5, 5), co);
                
            }
            origLightPos[0] = new Vector3(1880f, 70f, -3169f);//-40, 20 + i * 20, 400);
            origLightPos[1] = new Vector3(1880f, 70f, -3249f);//-40, 20 + i * 20, 400);
            origLightPos[2] = new Vector3(1960f, 70f, -3169f);//-40, 20 + i * 20, 400);
            origLightPos[3] = new Vector3(1960f, 70f, -3249f);
           

            //Interpolador para mover las luces de un lado para el otro
            interp = new InterpoladorVaiven();
            interp.Min = -100f;
            interp.Max = 100f;
            interp.Speed = 50f;
            interp.Current = 0f;
            //Fin Luces




            //Almacenar volumenes de colision del escenario
           // objetosColisionables.Clear();
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                //Los objetos del layer "TriangleCollision" son colisiones a nivel de triangulo
                if (mesh.Name == "Room-1-Floor-0")
                {
                    objetosColisionables.Add(TriangleMeshCollider.fromMesh(mesh));
                }
                //El resto de los objetos son colisiones de BoundingBox. Las colisiones a nivel de triangulo son muy costosas asi que deben utilizarse solo
                //donde es extremadamente necesario (por ejemplo en el piso). El resto se simplifica con un BoundingBox
                else
                {
                    objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
            }
            
            objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(personaje2.mesh.BoundingBox));
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


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            personaje1.update(elapsedTime);
            personaje2.update(elapsedTime);
            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;
                 
     


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
            //personaje1.mesh.Effect = currentShader;
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

                lightMesh.Position = origLightPos[i];// +Vector3.Scale(move, i + 1);
                
                lightColors[i] = ColorValue.FromColor(lightMesh.Color);
                pointLightPositions[i] = TgcParserUtils.vector3ToVector4(lightMesh.Position);
                pointLightIntensity[i] = 15f;
                pointLightAttenuation[i] = 0.20f;
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


            //Renderizar meshes de luz
            for (int i = 0; i < lightMeshes.Length; i++)
            {
                TgcBox lightMesh = lightMeshes[i];
                lightMesh.render();
            }


            ////////FINLUCES




            //Obtener valor de UserVar (hay que castear)
            //int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");


            //Obtener valores de Modifiers
            float distanciaCam = (float)GuiController.Instance.Modifiers["distanciaCam"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];
            //GuiController.Instance.RotCamera.CameraDistance = distanciaCam;

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                //Tecla F apretada
                personaje1.tirarPoder();
            }
            //izquierda
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                personaje1.actions.moveForward = velocidadCaminar * elapsedTime * (float)personaje1.Direccion;
                personaje1.actions.moving = true;
                personaje1.mesh.playAnimation("CaminandoRev", true);
            }
            //derecha
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                personaje1.actions.moveForward = -velocidadCaminar * elapsedTime * (float)personaje1.Direccion;
                personaje1.actions.moving = true;
                personaje1.mesh.playAnimation("Caminando", true);

            }
                //ninguna de las dos
            else
            {
                personaje1.actions.moveForward = 0;
                personaje1.actions.moving = false;
                personaje1.mesh.playAnimation("Parado", true);
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
                collisionManager.GravityEnabled = true;
            }
            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }
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
                Vector3 realMovement = collisionManager.moveCharacter(personaje1.Spheres.GlobalSphere, new Vector3 (personaje1.actions.moveForward,personaje1.actions.jump,0) , objetosColisionables);
                personaje1.move(realMovement);
                
            //}
            personaje1.render(elapsedTime);
            personaje2.render(elapsedTime);
            



            //settear uservars
            GuiController.Instance.UserVars.setValue("velocidadX", (personaje1.actions.moveForward*1/elapsedTime));
            GuiController.Instance.UserVars.setValue("camX", GuiController.Instance.ThirdPersonCamera.Position.X);
            GuiController.Instance.UserVars.setValue("camY", GuiController.Instance.ThirdPersonCamera.Position.Y);
            GuiController.Instance.UserVars.setValue("camZ", GuiController.Instance.ThirdPersonCamera.Position.Z);
            GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));
            GuiController.Instance.UserVars.setValue("Position", TgcParserUtils.printVector3(personaje1.getPosition()));

            float offsetforward = Math.Abs(personaje2.getPosition().X - personaje1.getPosition().X) / (-2) - (10+(personaje2.getPosition().X - personaje1.getPosition().X)/10);
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3((personaje2.getPosition().X + personaje1.getPosition().X) / 2,
                                                                           (personaje2.getPosition().Y + personaje1.getPosition().Y) / 2,
                                                                            personaje2.getPosition().Z),
                                                                            13, (offsetforward < -40 ? offsetforward : -40));

        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }

    }
}
