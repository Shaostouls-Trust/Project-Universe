using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using org.mariuszgromada.math.mxparser;

using AX;
using AX.Expression;
using Parser = AX.Expression.ExpressionParser;

public class TestExpressionParser : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //Debug.Log("PARSE IT: \"2*radius\" = " + parseMath("2 * radius"));


        parseMath("2 * radius");

        //var parser = new ExpressionParser();
        //Expression exp = parser.EvaluateExpression("(5+3)*8^2-5*(-2)");
        //Debug.Log("Result: " + exp.Value);  // prints: "Result: 522"
    }

    // Update is called once per frame
    void Update()
    {

    }

    // PARSE_MATH
    public double parseMath(string expr)
    {
        Parser parser = Archimatix.GetMathParser();

        //AX.Expression.ExpressionParser parser = new AX.Expression.ExpressionParser();


        Expression exp = parser.EvaluateExpression("(5+3)*8^2-5*(-2)");


        Debug.Log("Result: " + exp.Value);  // prints: "Result: 522"
        Debug.Log("Result: " + parser.Evaluate("ln(e^5)"));  // prints: "Result: 5"
        Debug.Log("Result iseven: " + parser.Evaluate("iseven(3)"));  // prints: "Result: 5"


        // unknown identifiers are simply translated into parameters:

        // LIKE SYMBOLS
        Expression exp2 = parser.EvaluateExpression("sin(x * PI/180) + Sin(y * PI/180)");

        Debug.Log("parameter count: " + exp2.Parameters.Count);

        // foreach parameter look up value in ax.parameters. If not there, then on setvars in PO.
        exp2.Parameters["x"].Value = 45; // set the named parameter "x"
        exp2.Parameters["y"].Value = 45; // set the named parameter "x"
        Debug.Log("sin(45°): " + exp2.Value); // prints "sin(45°): 0.707106781186547" 


        // convert the expression into a delegate:

        var sinFunc = exp2.ToDelegate("x");
        Debug.Log("sin(90°): " + sinFunc(90)); // prints "sin(90°): 1" 


        // It's possible to return multiple values, but it generates extra garbage for each call due to the return array:

        //var exp3 = parser.EvaluateExpression("sin(angle/180*PI) * length, cos(angle/180*PI) * length");
        //var f = exp3.ToMultiResultDelegate("angle", "length");
        //double[] res = f(45, 2);  // res contains [1.41421356237309, 1.4142135623731]


        // To add custom functions to the parser, you just have to add it before parsing an expression:

        parser.AddFunc("test", (p) => {
            Debug.Log("TEST: " + p.Length);
            return p[0];
        });
        Expression ex3 = parser.EvaluateExpression("2*test(x,5)");
        ex3.Parameters["x"].Value = 45;

        Debug.Log("Result: " + ex3.Value); // prints "TEST: 2" and "Result: 84"
        //Debug.Log("Result: " + parser.Evaluate("2*test(x,5)")); // prints "TEST: 2" and "Result: 84"



        //Expression eh = new Expression(expr);

        //eh.AddConstant(new Constant("radius", 5));

        //Debug.Log("result: " + expr + " --- " + eh.calculate());
        return 1;
    }

}
