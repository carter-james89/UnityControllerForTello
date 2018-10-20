using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DebugVector : MonoBehaviour
{
    public Transform trans;
    // Transform anchorTrans;
  //  public enum Direction { x,y,z,all}

    GameObject anchorObject;
    GameObject xObject;
    GameObject yObject;
    GameObject zObject;

    float xDis;
    float yDis;
    float zDis;


    public static void DrawVector(     
        Transform transformToDraw, 
        Color colorX, 
        Color colorY, 
        Color colorZ,
        Vector3 dir, 
        float length = .3f, 
        float width = .01f)
    {       
        var newVector = transformToDraw.gameObject.AddComponent(typeof(DebugVector)) as DebugVector;
        newVector.CustomAwake(dir,length, width,colorX, colorY, colorZ);
    }
    public void CustomAwake(Vector3 dir, float length, float width, Color colorX, Color colorY, Color colorZ)
    {
        anchorObject = new GameObject();
        this.trans = anchorObject.transform;

        //add thee children to the anchor object
        xObject = new GameObject();
        yObject = new GameObject();
        zObject = new GameObject();
        //match the rotations
        xObject.transform.rotation = this.trans.localRotation;
        yObject.transform.rotation = this.trans.localRotation;
        zObject.transform.rotation = this.trans.localRotation;
        //xObject.transform.Rotate(new Vector3(0, 90, 0));
        //set the children to the anchor object
        xObject.transform.parent = this.trans;
        yObject.transform.parent = this.trans;
        zObject.transform.parent = this.trans;
        //move children to anchors origin
        xObject.transform.localPosition = new Vector3(0, 0, 0);
        yObject.transform.localPosition = new Vector3(0, 0, 0);
        zObject.transform.localPosition = new Vector3(0, 0, 0);
        //match anchors rotation then set parent, then move to origin
        this.trans.rotation = transform.rotation;
        this.trans.SetParent(transform);
        this.trans.localPosition = new Vector3(0, 0, 0);
        // this.trans.position = transform.position;


        CreateVector(dir, length, width,colorX,colorY,colorZ);
    }

    public void Rotate(Vector3 newRot)
    {
        trans.Rotate(newRot);
    }
    public void CreateVector(Vector3 dir, float length, float width, Color colorX, Color colorY, Color colorZ)
    {
        if(dir.x == 1)
        {
            LineRenderer xRenderer = xObject.AddComponent<LineRenderer>();
            xRenderer.useWorldSpace = false;
            xRenderer.SetPosition(0, xObject.transform.localPosition);
            xRenderer.SetPosition(1, new Vector3(xObject.transform.localPosition.x + length, xObject.transform.localPosition.y, xObject.transform.localPosition.z));
            Material redMat = new Material(Shader.Find("Transparent/Diffuse"));
            redMat.color = colorX;
            xRenderer.material = redMat;
            xRenderer.SetWidth(width, width);
        }
        else if(dir.x == -1)
        {
             LineRenderer xRenderer = xObject.AddComponent<LineRenderer>();
            xRenderer.useWorldSpace = false;
            xRenderer.SetPosition(0, xObject.transform.localPosition);
            xRenderer.SetPosition(1, new Vector3(xObject.transform.localPosition.x - length, xObject.transform.localPosition.y, xObject.transform.localPosition.z));
            Material redMat = new Material(Shader.Find("Transparent/Diffuse"));
            redMat.color = colorX;
            xRenderer.material = redMat;
            xRenderer.SetWidth(width, width);
        }

        if (dir.y == 1)
        {
            yRenderer = yObject.AddComponent<LineRenderer>();
            yRenderer.useWorldSpace = false;
            yRenderer.SetPosition(0, yObject.transform.localPosition);
            yRenderer.SetPosition(1, new Vector3(yObject.transform.localPosition.x, yObject.transform.localPosition.y + length, yObject.transform.localPosition.z));
            Material greenMat = new Material(Shader.Find("Transparent/Diffuse"));
            greenMat.color = colorY;
            yRenderer.material = greenMat;
            yRenderer.SetWidth(width, width);
        }
        else if(dir.y == -1)
        {
              yRenderer = yObject.AddComponent<LineRenderer>();
            yRenderer.useWorldSpace = false;
            yRenderer.SetPosition(0, yObject.transform.localPosition);
            yRenderer.SetPosition(1, new Vector3(yObject.transform.localPosition.x, yObject.transform.localPosition.y - length, yObject.transform.localPosition.z));
            Material greenMat = new Material(Shader.Find("Transparent/Diffuse"));
            greenMat.color = colorY;
            yRenderer.material = greenMat;
            yRenderer.SetWidth(width, width);
        }

        if (dir.z == 1)
        {
            LineRenderer zRenderer = zObject.AddComponent<LineRenderer>();
            zRenderer.useWorldSpace = false;
            zRenderer.SetPosition(0, zObject.transform.localPosition);
            zRenderer.SetPosition(1, new Vector3(zObject.transform.localPosition.x, zObject.transform.localPosition.y, zObject.transform.localPosition.z + length));
            Material blueMat = new Material(Shader.Find("Transparent/Diffuse"));
            blueMat.color = colorZ;
            zRenderer.material = blueMat;
            zRenderer.SetWidth(width, width);
        }
        else if(dir.z == -1)
        {
               LineRenderer zRenderer = zObject.AddComponent<LineRenderer>();
            zRenderer.useWorldSpace = false;
            zRenderer.SetPosition(0, zObject.transform.localPosition);
            zRenderer.SetPosition(1, new Vector3(zObject.transform.localPosition.x, zObject.transform.localPosition.y, zObject.transform.localPosition.z - length));
            Material blueMat = new Material(Shader.Find("Transparent/Diffuse"));
            blueMat.color = colorZ;
            zRenderer.material = blueMat;
            zRenderer.SetWidth(width, width);
        }

      //  anchorObject.hideFlags = HideFlags.HideInHierarchy;
    }
    LineRenderer yRenderer;
    public void Update()
    {
        //yRenderer.SetPosition(1, new Vector3(yObject.transform.localPosition.x, yObject.transform.localPosition.y + -VesselControl.offSet.y, yObject.transform.localPosition.z));
    }
}