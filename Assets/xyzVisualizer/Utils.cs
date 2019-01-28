using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class Utils {
    public static void SaveRenderTexture2PNG(RenderTexture rt, string fileDir, string pngName)
    {
        fileDir = ConvertSlash(fileDir);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        if (!Directory.Exists(fileDir))
        {
            Directory.CreateDirectory(fileDir);
        }
        try
        {
            using (FileStream fs = File.Open(fileDir + "\\" + pngName + ".png", FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
            }
        }
        catch(Exception e)
        {
            Debug.Log("failed to create file ---------" + fileDir + "\\" + pngName + ".png");
        }
        Texture2D.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
    }

    // @"D:\a\b\c" 2 "D:\\a\\b\\c"
    public static string ConvertSlash(string str)
    {
        return str.Replace("\\", "\\\\");
    }

    // last format "*.xxx|*.yyy|*.zzz"
    public static string[] GetFilesName(string fileDir, string type)
    {
        fileDir = ConvertSlash(fileDir);
        string[] types = type.Split('|');
        string[] result;
        List<string> list = new List<string>();
        for(int i = 0; i < types.Length; ++i)
        {
            string[] dirs = Directory.GetFiles(fileDir, types[i]);
            Debug.Log(dirs.Length);
            for(int j = 0; j < dirs.Length; ++j)
            {
                list.Add(dirs[j]);
            }
        }
        result = new string[list.Count];
        int index = 0;
        foreach(string i in list)
        {
            result[index] = i;
            ++index;
        }
        return result;
    }

    // TODO debug
    public static void ReadXYZFilesPos(string fileDir, ref List< List<Vector3> > points)
    {
        fileDir = ConvertSlash(fileDir);
        string[] files = GetFilesName(fileDir, "*.xyz");
        if (points == null)
        {
            points = new List<List<Vector3>>();
        }
        List<Vector3> pointlist;
        for (int i = 0; i < files.Length; ++i)
        {
            pointlist = new List<Vector3>();
            ReadXYZFilePos(files[i], ref pointlist);
            points.Add(pointlist);
        }
    }

    public static int ReadXYZFilePos(string file, ref List<Vector3> points)
    {
        file = ConvertSlash(file);
        int pointNum = 0;
        try
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs);
                string[] substrs;
                Vector3 vec;
                if (points == null)
                {
                    points = new List<Vector3>();
                }
                for (string str = sr.ReadLine(); str != null; str = sr.ReadLine())
                {
                    ++pointNum;
                    substrs = str.Split(' ');
                    vec.x = Convert.ToSingle(substrs[0]);
                    vec.y = Convert.ToSingle(substrs[1]);
                    vec.z = Convert.ToSingle(substrs[2]);
                    points.Add(vec);
                }
            }
        }catch(Exception e){
            Debug.Log("failed to open file --------- " + file);
        }
        return pointNum;
    }
}
