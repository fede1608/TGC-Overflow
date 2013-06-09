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
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Collision.ElipsoidCollision;

namespace AlumnoEjemplos.overflowDT
{
    public class BoundingMultiSphere
    {
        #region Variables

        float scale;

        public struct Sphere
        {
            public TgcBoundingSphere bonesphere;
            public Matrix offset;
            public int vertexreferidx;
        }

        private Dictionary<string, Sphere> bones;

        /// <summary>
        /// Diccionario de cada Sphere identificado por hueso.
        /// </summary>
        public Dictionary<string, Sphere> Bones
        {
            get { return bones; }
            set { bones = value; }
        }

        private TgcElipsoid globalSphere;
        /// <summary>
        /// TgcBoundingSphere que envuelve a todo el personajo.
        /// </summary>
        public TgcElipsoid GlobalSphere
        {
            get { return globalSphere; }
            set { globalSphere = value; }
        }

        public BoundingMultiSphere()
        {
            bones = new Dictionary<string, Sphere>();
            globalSphere = new TgcElipsoid();
        }
        #endregion

        #region Metodos
        /// <summary>
        /// Método para obtener los vertices del SkeletalMesh
        /// </summary>
        /// <param name="filePath"> Direccion absoluta del XML del mesh</param>
        /// <param name="pj"> SkeletalMesh previamente cargado</param>
        public void getVerticesForBox(string filePath, TgcSkeletalMesh pj, float escala)
        {
            try
            {
                scale = escala;
                string xmlString = File.ReadAllText(filePath);
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xmlString);
                XmlElement root = dom.DocumentElement;
                XmlElement meshNode = (XmlElement)root.GetElementsByTagName("mesh")[0];

                XmlNode verticesNode = meshNode.GetElementsByTagName("vertices")[0];
                int count = int.Parse(verticesNode.Attributes["count"].InnerText) / 3;
                Vector3[] vertices = new Vector3[count];

                XmlNode coordVNode = meshNode.GetElementsByTagName("coordinatesIdx")[0];
                count = int.Parse(coordVNode.Attributes["count"].InnerText);
                float[] coordV = TgcParserUtils.parseFloatStream(coordVNode.InnerText, count);
                int[] order = new int[vertices.Length];
                for (int i = 0; i < order.Length; i++)
                {
                    int j = 0;
                    while (j < coordV.Length)
                    {
                        if (coordV[j] == i)
                        {
                            order[i] = j;
                            break;
                        }
                        j++;
                    }
                }

                pj.playAnimation("Cruz", true);
                Vector3[] vert = pj.getVertexPositions();
               // for (int cont = 0; cont < vert.Length; cont++)
               // {
               //     vert[cont] = Vector3.Scale(vert[cont], 0.05f);
               // }
                vertices = updateBMSvertex(vert, order, vertices.Length);

                XmlNode skeletonNode = meshNode.GetElementsByTagName("skeleton")[0];
                TgcSkeletalBoneData[] bonesData = new TgcSkeletalBoneData[skeletonNode.ChildNodes.Count];
                string[] bonesname = new string[skeletonNode.ChildNodes.Count];
                foreach (XmlElement boneNode in skeletonNode.ChildNodes)
                { bonesname[int.Parse(boneNode.Attributes["id"].InnerText)] = boneNode.Attributes["name"].InnerText; }

                XmlNode weightsNode = meshNode.GetElementsByTagName("weights")[0];
                int[][] vweight = new int[bonesname.Length][];
                Vector3[] aux = parseFAtoV3A(TgcParserUtils.parseFloatStream(weightsNode.InnerText, int.Parse(weightsNode.Attributes["count"].InnerText)));
                List<int> grupo;
                //for (int cont = 0; cont < count; cont++)
                //{
                //    vertices[cont] = Vector3.Scale(vertices[cont], scale);
                //}
                for (int i = 0; i < bonesname.Length; i++)
                {
                    vertices[i] = vertices[i]* scale;
                    Sphere bonny = new Sphere();
                    bonny.vertexreferidx = -1;
                    grupo = new List<int>();
                    for (int j = 0; j < aux.Length; j++)
                    {
                        if (aux[j].Y == i)
                        {
                            grupo.Add((int)(aux[j].X));
                            if (bonny.vertexreferidx == -1)
                                bonny.vertexreferidx = (int)(aux[j].X);
                        }
                    }
                    vweight[i] = grupo.ToArray();
                   
                    TgcBoundingBox auxbox = new TgcBoundingBox(getPointMin(vertices, vweight[i]),
                                                               getPointMax(vertices, vweight[i]));
                    //auxbox.scaleTranslate(new Vector3(0,0,0), new Vector3(0.05f,0.05f,0.05f));
                    float radius = FastMath.Max(FastMath.Max(auxbox.calculateAxisRadius().X,
                                                             auxbox.calculateAxisRadius().Y),
                                                auxbox.calculateAxisRadius().Z);

                    TgcSkeletalBone meshbone = pj.getBoneByName(bonesname[i]);
                    bonny.bonesphere = new TgcBoundingSphere(auxbox.calculateBoxCenter() , radius*0.05f);

                    Vector3 vv = pj.getBoneByName(bonesname[i]).StartPosition;
                    Vector3 vc = bonny.bonesphere.Center;
                    vc = Vector3.TransformCoordinate(vc, Matrix.Invert(meshbone.MatFinal));
                    bonny.offset = Matrix.Translation(vc - vv);
                    //bonny.offset.Scale(new Vector3(0.05f,0.05f,0.05f));
                    this.bones.Add(bonesname[i], bonny);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar mesh desde archivo: " + filePath, ex);
            }
        }

        /// <summary>
        /// Parsea un array de float en un array de Vector3
        /// </summary>
        /// <param name="lista"> Array de floats a parsear</param>
        public Vector3[] parseFAtoV3A(float[] lista)
        {
            Vector3[] ret = null;
            if ((lista.Length / 3) - Math.Round((decimal)(lista.Length / 3)) == 0)
            {
                ret = new Vector3[lista.Length / 3];
                for (int i = 0; i < lista.Length / 3; i++)
                {
                    ret[i].X = lista[i * 3];
                    ret[i].Y = lista[i * 3 + 1];
                    ret[i].Z = lista[i * 3 + 2];
                }
            }
            else { throw new Exception("Error al parsear float[] a vector."); }
            return ret;
        }

        /// <summary>
        /// Obtiene el punto minimo para un BB en base a una seleccion de un array de vertices
        /// </summary>
        /// <param name="veclist"> Array de vertices</param>
        /// <param name="indice"> Array de con indices de la seleccion de vertices</param>
        public Vector3 getPointMin(Vector3[] veclist, int[] indice)
        {
            Vector3 ret;
            ret.X = 9999f;
            ret.Y = 9999f;
            ret.Z = 9999f;
            foreach (int ind in indice)
            {
                ret.X = (ret.X > veclist[ind].X ? veclist[ind].X : ret.X);
                ret.Y = (ret.Y > veclist[ind].Y ? veclist[ind].Y : ret.Y);
                ret.Z = (ret.Z > veclist[ind].Z ? veclist[ind].Z : ret.Z);
            }
            return ret;
        }

        /// <summary>
        /// Obtiene el punto maximo para un BB en base a una seleccion de un array de vertices
        /// </summary>
        /// <param name="veclist"> Array de vertices</param>
        /// <param name="indice"> Array de con indices de la seleccion de vertices</param>
        public Vector3 getPointMax(Vector3[] veclist, int[] indice)
        {
            Vector3 ret;
            ret.X = -9999f;
            ret.Y = -9999f;
            ret.Z = -9999f;
            foreach (int ind in indice)
            {
                ret.X = (ret.X < veclist[ind].X ? veclist[ind].X : ret.X);
                ret.Y = (ret.Y < veclist[ind].Y ? veclist[ind].Y : ret.Y);
                ret.Z = (ret.Z < veclist[ind].Z ? veclist[ind].Z : ret.Z);
            }
            return ret;
        }

        /// <summary>
        /// Actualiza los vertices para el BMS
        /// </summary>
        /// <param name="newlist"> Array de los vertices actualizados</param>
        /// <param name="order"> Array de los indices de la seleccion de vertices</param>
        /// <param name="vertexcount"> cantidad precalculada de vertices total</param>
        public Vector3[] updateBMSvertex(Vector3[] newlist, int[] order, int vertexcount)
        {
            Vector3[] ret = new Vector3[vertexcount];
            for (int i = 0; i < order.Length; i++)
            {
                ret[i] = newlist[order[i]];
            }
            return ret;
        }

        #endregion
    }
}