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

        TgcSkyBox skyBox;
        TgcScene escenario;
        Size screenSize = GuiController.Instance.Panel3d.Size;
        Vector3 gravity = new Vector3(0f, -10f, 0f);
        float velocidadCaminar = 400f;
        int hitdelay = 1000;
        static int damagepunch = 2;
        static int damagekick = 5;
        static int damagepower = 10;
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
        
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
            TgcSceneLoader loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(mediaMPath + "\\Ambiental\\CuartoLucha-TgcScene.xml");

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("velocidadX");
            GuiController.Instance.UserVars.addVar("variablePrueba2");
            GuiController.Instance.UserVars.addVar("variablePrueba3");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("velocidadX", 0);
            GuiController.Instance.UserVars.setValue("variablePrueba2", 542251);
            GuiController.Instance.UserVars.setValue("variablePrueba2", 25451);

            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("distanciaCam", 1f, 1500f, 500f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));



            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 500, 1500), 500);


            /*
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
            */

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
            personaje1.setPosition(new Vector3(300f, 6f, 0f));
            personaje1.setRotation(Geometry.DegreeToRadian(90f));

            personaje2 = new Personaje();
            personaje2.Init();
            personaje2.setPosition(new Vector3(-300f, 6f, 0f));
            personaje2.setRotation(Geometry.DegreeToRadian(270f));









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
            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;
                      
           

            //Obtener valor de UserVar (hay que castear)
            //int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");


            //Obtener valores de Modifiers
            float distanciaCam = (float)GuiController.Instance.Modifiers["distanciaCam"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];
            GuiController.Instance.RotCamera.CameraDistance = distanciaCam;

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                //Tecla F apretada
                
            }
            //izquierda
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                personaje1.actions.moveForward = -velocidadCaminar * elapsedTime * (float)personaje1.Direccion;
                personaje1.actions.moving = true;
                personaje1.mesh.playAnimation("CaminandoRev", true);
            }
            //derecha
            else if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                personaje1.actions.moveForward = velocidadCaminar * elapsedTime * (float)personaje1.Direccion;
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
            

            //derecha
            

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
                if (true)
                {
                    mesh.BoundingBox.render();
                }
            }
            personaje1.setPosition(personaje1.getPosition() + new Vector3(personaje1.actions.moveForward,0f,0f));
            personaje1.mesh.animateAndRender();
            personaje2.mesh.animateAndRender();



            //settear uservars
            GuiController.Instance.UserVars.setValue("velocidadX", (personaje1.actions.moveForward*1/elapsedTime));
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
