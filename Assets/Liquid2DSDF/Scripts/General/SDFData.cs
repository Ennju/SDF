using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFData : MonoBehaviour {

    // input a resolution, return resolution + 2 
    public static int GenerateBeginRT2D(Texture2D srcTex, ref RenderTexture dest, int resolutionX, int resolutionY)
    {
        int targetResolutionX = resolutionX + 2;
        int targetResolutionY = resolutionY + 2;

        Texture2D target = new Texture2D(targetResolutionX, targetResolutionY, TextureFormat.RFloat, false);

        int srcWidth = srcTex.width;
        int srcHeight = srcTex.height;
        Debug.Log(srcWidth + " " + srcHeight);

        Color blockColor = Color.black;
        Color freeColor = Color.white;
        int brickBegini, brickEndi, brickBeginj, brickEndj, bricki, brickj;
        float brickWidth = (float)srcWidth / (float)resolutionX;
        float brickHeight = (float)srcHeight / (float)resolutionY;
        for(int i = 0; i < targetResolutionX; ++i)
        {
            for(int j = 0; j < targetResolutionY; ++j)
            {
                if(i == 0 || j == 0)
                {
                    target.SetPixel(i, j, freeColor);
                    continue;
                }

                bricki = i - 1;
                brickj = j - 1;
                brickBegini = Mathf.CeilToInt(brickWidth * bricki);
                brickEndi = Mathf.CeilToInt(brickWidth * (bricki + 1));
                brickBeginj = Mathf.CeilToInt(brickHeight * brickj);
                brickEndj = Mathf.CeilToInt(brickHeight * (brickj + 1));
                bool isBlock = false;
                //Debug.Log(brickBegini + " " + brickEndi + " " + brickBeginj + " " + brickEndj);
                for(int ii = brickBegini; ii < brickEndi; ++ii)
                {
                    for(int jj = brickBeginj; jj < brickEndj; ++jj)
                    {
                        if(srcTex.GetPixel(ii, jj).r < 0.5f)
                        {
                            isBlock = true;
                            break;
                        }
                    }
                    if (isBlock)
                        break;
                }

                target.SetPixel(i, j, isBlock ? blockColor : freeColor);
            }
        }

        target.Apply();
        if (dest == null)
            dest = new RenderTexture(targetResolutionX, targetResolutionY, 0, RenderTextureFormat.RFloat);
        dest.enableRandomWrite = true;
        Graphics.Blit(target, dest);
        Destroy(target);
        return targetResolutionX;
    }

}
