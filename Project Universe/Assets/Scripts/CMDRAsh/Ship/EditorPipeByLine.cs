using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorPipeByLine : MonoBehaviour
{
    public GameObject PipeObjMaxLen;
    public float pipeMaxLength;
    public GameObject PipeObjLen2;
    public float pipe2Length;
    public GameObject PipeObjMinLen;
    public float pipeMinLength;
    public Vector3 pipePositiveZDirection;
    public GameObject PipeCurve90;
    public float pipeCurveOuterDiam;
    public Vector3 curvePositiveZDirection;
    public bool generate;
    public GameObject endpointB;
    private float xpos;
    private float ypos;
    private float zpos;
    //passed by to next segment upon generation to account for curve distance.
    private float xoffset = 0f;
    private float yoffset = 0f;
    private float zoffset = 0f;

    public float XOffset
    {
        set { xoffset = value; }
    }
    public float YOffset
    {
        set { yoffset = value; }
    }
    public float ZOffset
    {
        set { zoffset = value; }
    }

    // Update is called once per frame
    /// <summary>
    /// get change in x,y,z and place pipes across those distances in the direction of point B such that the path
    /// of the pipes in x,y,z is equal to the 3D distance between points A and B.
    /// </summary>
    void Update()
    {
        if (generate)
        {
            if (endpointB != null)
            {
                GenerateSection();
            }
        }
    }

    public void GenerateSection()
    {
        if (endpointB != null)
        {
            xpos = transform.position.x;// + xoffset;
            ypos = transform.position.y;// + yoffset;
            zpos = transform.position.z;// + zoffset;
            generate = false;
            //get x,y,z deltas
            float x = endpointB.transform.position.x - transform.position.x;
            float y = endpointB.transform.position.y - transform.position.y;
            float z = endpointB.transform.position.z - transform.position.z;
            int lencnt;
            float remain;
            //use abs values for greatest direction
            float absx = Mathf.Abs(x);
            float absy = Mathf.Abs(y);
            float absz = Mathf.Abs(z);
            float dir = 1f;//+-1f;
                           //generate pipes along line to point B
            Vector3 pipeDir = pipePositiveZDirection;
            //Z dir main
            if (absz > absx && absz > absy)
            {
                if (z < 0f)
                {
                    pipeDir.y += 180f;
                    dir = -1f;
                }
                lencnt = (int)(absz / pipeMaxLength);
                //place # of max length pipes
                for (int i = 0; i < lencnt; i++)
                {
                    Instantiate(PipeObjMaxLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                        , Quaternion.Euler(pipePositiveZDirection), transform);
                    zpos += (pipeMaxLength * dir);
                }
                remain = absz % pipeMaxLength;
                if (PipeObjLen2 != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjLen2, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipePositiveZDirection), transform);
                        zpos += (pipe2Length * dir);
                    }
                    remain = lencnt % pipeMaxLength;
                }
                if (PipeObjMinLen != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjMinLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipePositiveZDirection), transform);
                        zpos += (pipeMinLength * dir);
                    }
                }
            }

            // X direction main
            else if (absx > absy && absx > absz)
            {
                if (x > 0f)
                {
                    pipeDir.y += 90f;
                    dir = 1f;
                }
                else if (x < 0f)
                {
                    pipeDir.y -= 90f;
                    dir = -1f;
                }
                lencnt = (int)(absx / pipeMaxLength);
                //place # of max length pipes
                for (int i = 0; i < lencnt; i++)
                {
                    Instantiate(PipeObjMaxLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                        , Quaternion.Euler(pipeDir), transform);
                    xpos += (pipeMaxLength * dir);
                }
                remain = absx % pipeMaxLength;
                if (PipeObjLen2 != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjLen2, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipeDir), transform);
                        xpos += (pipe2Length * dir);
                    }
                    remain = lencnt % pipeMaxLength;
                }
                if (PipeObjMinLen != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjMinLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipeDir), transform);
                        xpos += (pipeMinLength * dir);
                    }
                }
            }

            else if (absy > absx && absy > absz)
            {
                if (y > 0f)
                {
                    pipeDir.x -= 90f;
                    dir = 1f;
                }
                else if (y < 0f)
                {
                    pipeDir.x += 90f;
                    dir = -1f;
                }
                lencnt = (int)(absy / pipeMaxLength);
                //place # of max length pipes
                for (int i = 0; i < lencnt; i++)
                {
                    Instantiate(PipeObjMaxLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                        , Quaternion.Euler(pipeDir), transform);
                    ypos += (pipeMaxLength * dir);
                }
                remain = absy % pipeMaxLength;
                if (PipeObjLen2 != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjLen2, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipeDir), transform);
                        ypos += (pipe2Length * dir);
                    }
                    remain = lencnt % pipeMaxLength;
                }
                if (PipeObjMinLen != null)
                {
                    lencnt = (int)(remain / pipe2Length);
                    for (int i = 0; i < lencnt; i++)
                    {
                        Instantiate(PipeObjMinLen, new Vector3(xpos + xoffset, ypos + yoffset, zpos + zoffset)
                            , Quaternion.Euler(pipeDir), transform);
                        ypos += (pipeMinLength * dir);
                    }
                }
            }

            //if point b has a point c (assuming point c is not in a straight line from b), then
            //place a curve in the direction of c
            if (endpointB != null)
            {
                if (endpointB.GetComponent<EditorPipeByLine>().endpointB != null)
                {
                    Vector3 cPos = endpointB.GetComponent<EditorPipeByLine>().endpointB.transform.position;
                    Vector3 bPos = endpointB.transform.position;
                    Vector3 rot = curvePositiveZDirection;
                    float x2 = cPos.x - bPos.x;
                    float y2 = cPos.y - bPos.y;
                    float z2 = cPos.z - bPos.z;
                    float absx2 = Mathf.Abs(x2);
                    float absy2 = Mathf.Abs(y2);
                    float absz2 = Mathf.Abs(z2);
                    if (absz > absx && absz > absy)
                    {
                        //rotate along z to align x/y
                        //primary movement in x
                        if (absx2 >= absy2)
                        {
                            if (x2 > 0f)
                            {
                                rot.z -= 90f;
                                xoffset += pipeCurveOuterDiam;
                            }
                            else
                            {
                                rot.z += 90f;
                                xoffset -= pipeCurveOuterDiam;
                            }
                        }
                        //primary movement in y
                        else if (absy2 > absx2)
                        {
                            if (y2 < 0f)
                            {
                                rot.z += 180f;
                                zoffset -= pipeCurveOuterDiam;
                            }
                        }
                    }

                    else if (absx > absy && absx > absz)
                    {
                        //align y/z
                        //primary movement in z
                        if (absz2 >= absy2)
                        {
                            if (z2 > 0f)
                            {
                                rot.y -= 90f;
                                rot.z += 90f;
                                zoffset += pipeCurveOuterDiam;
                            }
                            else
                            {
                                rot.y += 90f;
                                rot.z -= 90f;
                                zoffset -= pipeCurveOuterDiam;
                            }
                        }
                        //primary movement in y
                        else if (absy2 > absz2)
                        {
                            if (y2 > 0f)
                            {
                                rot.y -= 90f;
                                yoffset += pipeCurveOuterDiam;
                            }
                            else if (y2 < 0f)
                            {
                                rot.y += 90f;
                                yoffset -= pipeCurveOuterDiam;
                            }
                        }
                    }

                    else if (absy > absx && absy > absz)
                    {
                        //align x/z
                        //primary movement in z
                        if (absz2 >= absx2)
                        {
                            if (z2 > 0f)
                            {
                                rot.y += 180f;
                                rot.x -= 90f;
                                zoffset += pipeCurveOuterDiam;
                            }
                            else
                            {
                                rot.x -= 90f;
                                zoffset -= pipeCurveOuterDiam;
                            }
                        }
                        //primary movement in x
                        if (absx2 >= absz2)
                        {
                            if (x2 > 0f)
                            {
                                rot.y -= 90f;
                                rot.x -= 90f;
                                xoffset += pipeCurveOuterDiam;
                            }
                            else
                            {
                                rot.y += 90f;
                                rot.x -= 90f;
                                xoffset -= pipeCurveOuterDiam;
                            }
                        }
                    }
                    //instantiate curved pipe
                    Instantiate(PipeCurve90, new Vector3(xpos, ypos , zpos ), Quaternion.Euler(rot), transform);
                }// + yoffset+ yoffset+ zoffset
                //update startpos for next pipe section
                EditorPipeByLine epbl = endpointB.GetComponent<EditorPipeByLine>();
                //if()
                //epbl.XOffset = xoffset;
                //epbl.YOffset = yoffset;
                //epbl.ZOffset = zoffset;
                //generate next section
                epbl.GenerateSection();
            }
        }
        //reset offsets
        xoffset = 0f;
        yoffset = 0f;
        zoffset = 0f;
    }
}
