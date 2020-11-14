using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using UnityEngine;
using AX.SimpleJSON;

using AX.Expression;
using Parser = AX.Expression.ExpressionParser;

using AX;
using AXGeometry;
using AX.Generators;



public class AXScriptParser
{



    // PO [parameters]
    //AXParametricObject po;
   
    // PARSER
    private static Parser parser = new Parser();



    public AXScriptParser()
    {
        //Debug.Log("AXScriptParser init");

    }

    /// <summary>
    /// 
    /// ExecuteCodeBlock is called to parse a script and effect changes to a po,
    /// such as setting parameter values or setting the output paths of the po.
    /// 
    /// In the future, it could also set the output meshes or even generate objects outside of a po.
    /// But for now, a script resides in a po and that po is the home data store for the 
    /// opersations in the script.
    /// 
    /// </summary>
    /// <param name="lines">Lines.</param>
    /// <param name="shape">Shape.</param>

    public void executeCodeBloc(AXParametricObject po, List<string> lines, AXTurtle t)
    {


        //Debug.Log(Name+" executeCodeBloc ====================== lines.Count=" + lines.Count);



        bool scanningLoop = false;
        bool scanningIf = false;

        List<string> blockLines = new List<string>();
        //bool isParsingLoop = false;

        string codeAsLine = "";
        foreach (string line in lines)
        {
            string newline = "";
            if (codeAsLine != "")
                newline = "%";
            codeAsLine += newline + line;
        }

        int loopct = 0;
        double repeats = 3;
        int step = 1;
        string loopCounterName = "count";

        int if_ct = 0;
        string condition = "";


        // LINES

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string trimmedLine = Regex.Replace(line.Trim(), @"\s+", " ");

            trimmedLine = trimmedLine.Replace(", ", ",");

            string[] chunks = trimmedLine.Split(" "[0]);

            if (string.IsNullOrEmpty(chunks[0]))
                continue;


            if (chunks[0] == "end")
                return;


            // LOOP CONTROL

            // -- LOOP
            if (chunks[0] == "loop")
            {

                if (loopct == 0 && !scanningIf)
                {
                    //Debug.Log("*** Start scanning loop");
                    scanningLoop = true;

                    try
                    {
                        //Debug.Log(chunks [1]);

                        // REPEATS
                        repeats = parseMath_ValueOnly(po, chunks[1]) - 1;

                        // COUNTER VARIABLE NAME
                        if ((chunks.Length > 2) && (!string.IsNullOrEmpty(chunks[2])))
                            loopCounterName = chunks[2];
                        else
                            loopCounterName = "count";


                        // STEP
                        if ((chunks.Length > 3) && (!string.IsNullOrEmpty(chunks[3])))
                            step = (int)parseMath_ValueOnly(po, chunks[3]);

                        if (step < 1)
                            step = 1;
                        //Debug.Log("**** use loopCounterName="+loopCounterName);
                    }
                    catch
                    {
                        Debug.Log("parsing error");
                    }
                }
                else
                {
                    // keep inner loop in this block
                    blockLines.Add(trimmedLine);
                }
                loopct++;
                continue;
            }

            // -- ENDLOOP
            else if (chunks[0] == "endloop")
            {

                loopct--;

                //Debug.Log(" [ loopct="+loopct+", scanningIf = " + scanningIf);
                if (loopct == 0 && !scanningIf)
                {

                    scanningLoop = false;
                    //Debug.Log("*** *** Execute block " + repeats + " times. blockLines.Count = " + blockLines.Count);
                    for (int rr = 0; rr <= repeats; rr += step)
                    {
                        //Debug.Log ("EXECUTING BLOCK: "+blockLines[0]);
                        //Debug.Log("set value "+loopCounterName + "="+ rr);
                        po.setVar(loopCounterName, rr);
                        executeCodeBloc(po, blockLines, t);
                        continue;
                    }

                    blockLines.Clear();
                }
                else
                {
                    // Add nested, inner endloop to this blok
                    blockLines.Add(trimmedLine);
                }
                continue;

            }




            // CONDIITONAL CONTROL
            //Debug.Log(" do: " + chunks [0]);

            // -- IF
            if (chunks[0] == "if")
            {
                if (if_ct == 0 && !scanningLoop)
                {
                    scanningIf = true;
                    condition = trimmedLine;
                    //Debug.Log ("--> " + condition);
                }
                else
                    blockLines.Add(trimmedLine);

                // keep adding ifs until endif count balances
                if_ct++;
                continue;
            }
            // -- ENDIF
            else if (chunks[0] == "endif")
            {

                if_ct--;

                //Debug.Log(" [ if_ct="+if_ct+", scanningLoop = " + scanningLoop);

                if (if_ct == 0 && !scanningLoop)
                {
                    // process conditional

                    scanningIf = false;

                    string[] cond_chunks = condition.Split(" "[0]);
                    string cond1 = cond_chunks[1];
                    bool not = (cond1.StartsWith("!")) ? true : false;
                    if (not)
                        cond1 = cond1.TrimStart('!');
                    AXParameter cond1_P = po.getControlParameter(cond1);

                    if (cond1_P != null && cond1_P.Type == AXParameter.DataType.Bool)
                    {
                        if ((cond1_P.boolval && !not) || (!cond1_P.boolval && not))
                        {
                            executeCodeBloc(po, blockLines, t);
                        }
                        else
                            blockLines.Clear();
                    }
                    else
                    {
                        string cond = (cond_chunks.Length < 3) ? "EQ" : cond_chunks[2];
                        float cval_1 = (float)parseMath_ValueOnly(po, cond_chunks[1]);
                        if (cond_chunks.Length < 4)
                        {
                            if (cval_1 == 1) executeCodeBloc(po, blockLines, t);
                            continue;
                        }

                        float cval_2 = (float)parseMath_ValueOnly(po, cond_chunks[3]);
                        switch (cond)
                        {
                            case "EQ":
                                if (cval_1 == cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;
                            case "NE":
                                if (cval_1 != cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;
                            case "LT":
                                if (cval_1 < cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;
                            case "LE":
                                if (cval_1 <= cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;
                            case "GE":
                                if (cval_1 >= cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;

                            case "GT":
                                if (cval_1 > cval_2) executeCodeBloc(po, blockLines, t);
                                else blockLines.Clear();
                                break;
                        }
                        // Debug.Log ("EXECUTING BLOCK: "+blockLines[0]);
                    }
                    blockLines.Clear();

                }
                else
                {
                    // keep the endif in this block
                    blockLines.Add(trimmedLine);
                }
                continue;
            }



            if (scanningLoop || scanningIf)
            {

                blockLines.Add(trimmedLine);
                continue;

            }









            float val1 = 0.0f;
            float val2 = 0.0f;
            int val3 = 0;
            //string expr;





            if (chunks[0] == "set")
            {

                if (chunks.Length == 3)
                {
                    po.setVar(chunks[1], (float)parseMath_ValueOnly(po, chunks[2]));

                    AXParameter param = po.getParameter(chunks[1]);

                    //Debug.Log(chunks [1] + " " );

                    if (param != null)
                    {

                        // INT
                        if (param.Type == AXParameter.DataType.Int)
                        {
                            //intValue(chunks[1], (int)parseMath(chunks[2]));
                            po.initiateRipple_setIntValueFromGUIChange(chunks[1], (int)parseMath_ValueOnly(po, chunks[2]));
                        }


                        // FLOAT
                        else if (param.Type == AXParameter.DataType.Float)
                            po.setParameterValueByName(chunks[1], (float)parseMath_ValueOnly(po, chunks[2]));

                        // BOOL
                        else if (param.Type == AXParameter.DataType.Bool)
                        {
                            bool b = (chunks[2] == "true" || chunks[2] == "on" || chunks[2] == "yes" || chunks[2] == "1");
                            //boolValue(chunks [1],  b);

                            po.initiateRipple_setBoolParameterValueByName(chunks[1], b);
                        }
                        else if (param.Type == AXParameter.DataType.String)
                            param.StringVal = chunks[2];
                        else if (param.Type == AXParameter.DataType.CustomOption)
                        {
                            if (chunks[1] == "Channel")
                            {
                                //Debug.Log("channel " + chunks [2] );
                                po.intValue(chunks[1], (int)parseMath_ValueOnly(po, chunks[2]));

                            }
                            //intValue (chunks [1], (int)parseMath (chunks [2]));

                        }
                    }
                }

            }
            else
            {



                // REQUIRE AXTurtle t

                if (t == null)
                    return;


                //Debug.Log(chunks [0] + " " + chunks.Length);

                switch (chunks[0])
                {
                    case "closed":
                        if (chunks[1] == "false")
                        {
                            po.isClosed = false;
                            if (po.generator.P_Output != null)
                                po.generator.P_Output.shapeState = ShapeState.Open;
                            //ShapeState
                        }
                        else
                        {
                            po.isClosed = true;
                            if (po.generator.P_Output != null)
                                po.generator.P_Output.shapeState = ShapeState.Closed;
                        }
                        break;
                    case "let":
                        if (chunks.Length == 3)
                        {
                            po.setVar(chunks[1], (float)parseMath_ValueOnly(po, chunks[2]));
                        }
                        break;
                    case "mov":
                        if (chunks.Length < 3)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        t.mov(val1, val2);
                        if (chunks.Length == 4 && chunks[3] != null)
                            t.dir((float)parseMath_ValueOnly(po, chunks[3]));
                        break;
                    case "collider":
                        t.colllider();
                        break;
                    case "dir":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999)  continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        t.dir(val1);
                        break;
                    case "fwd":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999) continue;
                        //Debug.Log ("chunks[1] " + chunks[1]);
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        if (chunks.Length > 2)
                        {
                            val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                            t.fwd(val1, val2);
                        }
                        else
                        {
                            t.fwd(val1);
                        }
                        break;
                    case "movfwd":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("movfwd needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999) continue;
                        //Debug.Log ("chunks[1] " + chunks[1]);
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        if (chunks.Length > 2)
                        {
                            val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                            t.movfwd(val1, -val2);
                        }
                        else
                            t.movfwd(val1);
                        break;
                    case "back":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999) continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        if (chunks.Length > 2)
                        {
                            val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                            t.back(val1, -val2);
                        }
                        else
                            t.back(val1);
                        break;
                    case "left":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999) continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        t.left(val1);
                        break;
                    case "right":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999) continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        t.right(val1);
                        break;
                    case "drw":
                        if (chunks.Length <= 2)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999)  continue;
                        //if ( (val2= parseFloatToken(chunks[2]) ) == -999999)  continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        t.drw(val1, val2);
                        break;
                    case "rdrw":
                        if (chunks.Length <= 2)
                        {
                            Debug.Log("rdrw needs two arguments: line ignored");
                            continue;
                        }
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        t.rdrw(val1, val2);
                        break;
                    case "rmov":
                        if (chunks.Length <= 2)
                        {
                            Debug.Log("rmov needs two arguments: line ignored");
                            continue;
                        }
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        t.rmov(val1, val2);
                        break;
                    case "arcl":
                        if (chunks.Length <= 3)
                        {
                            Debug.Log("arcl needs two arguments: line ignored");
                            continue;
                        }
                        //if ( (val1= parseFloatToken(chunks[1]) ) == -999999)  continue;
                        //if ( (val2= parseFloatToken(chunks[2]) ) == -999999)  continue;
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        //if ( (val3= (int) parseFloatToken(chunks[3]) ) == -999999)    continue;
                        //val3 = (int) parseMath(chunks[3]);

                        //Debug.Log("model.segmentReductionFactor=" + model.segmentReductionFactor);

                        val3 = Mathf.Max(1, Mathf.FloorToInt(((float)parseMath_ValueOnly(po, chunks[3]) * po.model.segmentReductionFactor)));
                        if (chunks.Length > 4)
                        {
                            float val4 = (float)parseMath_ValueOnly(po, chunks[4]);
                            t.arcl(val1, val2, val3, val4);
                        }
                        else
                        {
                            t.arcl(val1, val2, val3);
                        }
                        break;
                    case "arcr":
                        if (chunks.Length <= 3)
                        {
                            Debug.Log("arcr needs two arguments: line ignored");
                            continue;
                        }
                        //Debug.Log("model.segmentReductionFactor=" + model.segmentReductionFactor);

                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        val2 = (float)parseMath_ValueOnly(po, chunks[2]);
                        val3 = Mathf.Max(1, Mathf.FloorToInt(((float)parseMath_ValueOnly(po, chunks[3]) * po.model.segmentReductionFactor)));
                        if (chunks.Length > 4)
                        {
                            float val4 = (float)parseMath_ValueOnly(po, chunks[4]);
                            t.arcr(val1, val2, val3, val4);
                        }
                        else
                        {
                            t.arcr(val1, val2, val3);
                        }
                        break;
                    case "bezier":
                        if (chunks.Length <= 9)
                        {
                            Debug.Log("mov needs nine arguments: line ignored");
                            continue;
                        }
                        try
                        {
                            Vector2 a = new Vector2((float)parseMath_ValueOnly(po, chunks[1]), (float)parseMath_ValueOnly(po, chunks[2]));
                            Vector2 b = new Vector2((float)parseMath_ValueOnly(po, chunks[3]), (float)parseMath_ValueOnly(po, chunks[4]));
                            Vector2 c = new Vector2((float)parseMath_ValueOnly(po, chunks[5]), (float)parseMath_ValueOnly(po, chunks[6]));
                            Vector2 d = new Vector2((float)parseMath_ValueOnly(po, chunks[7]), (float)parseMath_ValueOnly(po, chunks[8]));
                            int segs = Mathf.Max(1, Mathf.FloorToInt(((float)parseMath_ValueOnly(po, chunks[9]) * po.model.segmentReductionFactor)));
                            t.bezier(a, b, c, d, segs);
                        }
                        catch
                        {
                            Debug.Log("bad bezier");
                        }
                        //int segs = (int) parseMath(chunks[9]);
                        break;
                    case "molding":
                        //  type  a.x a.y b.x b.y segs tension
                        if (chunks.Length <= 6)
                        {
                            Debug.Log("mov needs six arguments: line ignored");
                            continue;
                        }
                        string mtype = chunks[1];
                        Vector2 pt1 = new Vector2((float)parseMath_ValueOnly(po, chunks[2]), (float)parseMath_ValueOnly(po, chunks[3]));
                        Vector2 pt2 = new Vector2((float)parseMath_ValueOnly(po, chunks[4]), (float)parseMath_ValueOnly(po, chunks[5]));
                        //int msegs = (int) parseMath(chunks[6]);
                        int msegs = Mathf.Max(1, Mathf.FloorToInt(((float)parseMath_ValueOnly(po, chunks[6]) * po.model.segmentReductionFactor)));
                        float tension = (chunks.Length > 7) ? (float)parseMath_ValueOnly(po, chunks[7]) : .3f;
                        t.molding(mtype, pt1, pt2, msegs, tension);
                        break;
                    case "turnl":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        t.turnl(val1);
                        break;
                    case "turnr":
                        if (chunks.Length <= 1)
                        {
                            Debug.Log("mov needs two arguments: line ignored");
                            continue;
                        }
                        val1 = (float)parseMath_ValueOnly(po, chunks[1]);
                        t.turnr(val1);
                        break;
                }


            }
            // process each line
        }

    }



    // general purpose math parser that uses parameters managed by this object

    // PARSE_MATH
    public double parseMath_ValueOnly(AXParametricObject po, string exprString)
    {

        // OPTIMIZATION
        if (exprString == "0")
            return 0;


        AX.Expression.Expression expression = parser.EvaluateExpression(exprString);

        // SET SYMBOL VALUES: for "parameters" in the expression string.
        setExpressionParameters(expression, po);

        // CALCULATE EXPRESSION VALUE
        return expression.Value;

    }





    // VALUE and LIST OF SYMBOLS FOUND in epression string

    public MathParseResults parseMathWithResults(string exprString, AXParametricObject po)
    {
        // Use PARSER to Create EXPRESSION
        AX.Expression.Expression expression = parser.EvaluateExpression(exprString);

        // SET SYMBOL VALUES: for "parameters" in the expression string.
        setExpressionParameters(expression, po);

        // MATH_PARSE_RESULTS HAS THE RESULTANT VALUE AND A LIST OF SYMBOLS ALREADY USED TO PREVENT INTERNAL CYCLING
        MathParseResults mathParseResults = new MathParseResults();

        // CALCULATE EXPRESSION VALUE AND GET LIST OF PARAMETERS IN EXPRESSION
        mathParseResults.result = (float)expression.Value;
        mathParseResults.symbolsFound = new List<string>(expression.Parameters.Keys);


        return mathParseResults;

    }


    public void setExpressionParameters(AX.Expression.Expression expr, AXParametricObject po)
    {
        //Debug.Log("expr.Parameters.Count = " + expr.Parameters.Count);

        if (expr.Parameters.Count > 0)
        {
            foreach (KeyValuePair<string, AX.Expression.Parameter> eparam in expr.Parameters)
            {
                AXParameter p = null;


                // 1. 
                string paramName = eparam.Key;

                // 2. turn into guid

                string guidstr = ArchimatixUtils.keyToGuid(paramName);

                // Debug.Log("guidstr="+ guidstr);

                if (guidstr.Length > 20 && IsValidGUID(guidstr))
                {
                    // this symbol is a guid
                    p = po.model.getParameterByGUID(guidstr);
                    //Debug.Log("Use Value For: " + p.parametricObject.Name + "."+p.Name);
                }
                else
                {
                    //Debug.Log(" this symbol is a parameter.Name = " + paramName);

                    p = po.getParameter(paramName);
                }



                if (p != null)
                {
                    // use parameters validators here!
                    // Debug.Log("Found " + p.Name);


                    if (p.Type == AXParameter.DataType.Float)
                        expr.Parameters[eparam.Key].Value = p.FloatVal;
                    else if (p.Type == AXParameter.DataType.Int)
                        expr.Parameters[eparam.Key].Value = p.IntVal;
                }
                else if (paramName == "DetailLevel")
                {
                    expr.Parameters[paramName].Value = po.model.segmentReductionFactor;
                    return;
                }
                else if (po.vars != null)
                {
                    // vars - code defined parameters
                    foreach (KeyValuePair<string, float> item in po.vars)
                    {
                        if (item.Key == paramName)
                        {
                            //variableDefined = true;
                            expr.Parameters[paramName].Value = item.Value;
                            break;
                        }
                    }
                }
                else
                    expr.Parameters[paramName].Value = 0;

                //Debug.Log(eparam.Key + ": " + eparam.Value);

            }


        }
    }






    private bool IsValidGUID(string GUIDCheck)
    {
        if (!string.IsNullOrEmpty(GUIDCheck))
        {
            return new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$").IsMatch(GUIDCheck);
        }
        return false;
    }


}
