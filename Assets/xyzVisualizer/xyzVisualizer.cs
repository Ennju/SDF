using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class xyzVisualizer : MonoBehaviour {
    public Material waterMat;

    private bool debug = false;
    private bool write2PNG = true;
    private const int maxIndex = 65000;
    private string path = "E:\\liquid engine\\fluid-engine\\fluid-engine-dev\\" +
        "build\\src\\examples\\sph_sim\\sph_sim_output";
    private string outpath;
    private string[] paths;
    private Mesh[] meshes;

    private Mesh[] preMeshes;
    private bool meshIsUsing;
    private bool writeCurFrame;
    private int curFrame;
    private void Start()
    {
        if (debug)
        {
            Debugsth();
        }
        else
        {
            outpath = path + "\\" + "output";
            paths = Utils.GetFilesName(path, "*.xyz");
            meshIsUsing = false;
            writeCurFrame = false;
            curFrame = -1;
            StartCoroutine(LoadPointsAndPrepareMesh());
        }
    }

    private void Update()
    {
        if (debug)
        {

        }
        else
        {
            DrawFrame();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (write2PNG && writeCurFrame)
        {
            Utils.SaveRenderTexture2PNG(source, outpath, curFrame.ToString());
            writeCurFrame = false;
            Debug.Log("frame " + curFrame + " has complete!    (" + curFrame + " / " + paths.Length + ")");
        }
        Graphics.Blit(source, destination);
    }

    private IEnumerator LoadPointsAndPrepareMesh()
    {
        for(int i = 0; i < paths.Length; ++i)
        {
            while (preMeshes != null || meshIsUsing)
            {
                yield return null;
            }
            meshIsUsing = true;

            // 开始准备数据
            List<Vector3> list = new List<Vector3>();
            int num = Utils.ReadXYZFilePos(paths[i], ref list);
            int meshnum = (num - 1) / maxIndex + 1;
            preMeshes = new Mesh[meshnum];
            List<Vector3> poslist;
            int[] indexlist;
            int curIndex = 0;
            for(int j = 0; j < preMeshes.Length; ++j)
            {
                int listNum = Mathf.Min(maxIndex, num - curIndex);
                preMeshes[j] = new Mesh();
                poslist = new List<Vector3>();
                indexlist = new int[listNum];
                for(int k = 0; k < listNum; ++k)
                {
                    poslist.Add(list[k + curIndex]);
                    indexlist[k] = k;
                }
                curIndex += listNum;
                preMeshes[j].SetVertices(poslist);
                preMeshes[j].SetIndices(indexlist, MeshTopology.Points, 0);
                // Debug.Log("mesh " + j + " has completed");
            }

            meshIsUsing = false;
            yield return null;
        }
    }

    private void CheckMeshes()
    {
        if(preMeshes == null || meshIsUsing)
        {
            return;
        }
        meshIsUsing = true;
        meshes = preMeshes;
        preMeshes = null;
        writeCurFrame = true;
        ++curFrame;
        meshIsUsing = false;
    }

    private void DrawFrame()
    {
        // load data pos 2 mesh
        CheckMeshes();

        if(meshes == null)
        {
            return;
        }

        // draw meshes
        for(int i = 0; i < meshes.Length; ++i)
        {
            Graphics.DrawMesh(meshes[i], Matrix4x4.identity, waterMat, 0);
        }
    }

    private void Debugsth()
    {

    }

}
