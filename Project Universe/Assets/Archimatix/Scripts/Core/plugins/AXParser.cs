using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace AX.Expression
{


    public class AXParser : ExpressionParser
    {

        //private Dictionary<string, System.Func<string[]>> m_TurtleFuncs = new Dictionary<string, System.Func<string[]>>();

        public AXParser()
        {
            // ADD CONSTANTS
            AddConst("pi", () => System.Math.PI);
            AddConst("E", () => System.Math.E);
            AddConst("epsilon", () => Mathf.Epsilon);


            // ADD MATH PARSING FUNCTIONS

            AddFunc("iseven", (p) =>
            {
                return ((int)p.FirstOrDefault() % 2 == 0) ? 0 : 1;
            });
            AddFunc("modulo", (p) =>
            {
                return (int)p.FirstOrDefault() % (float)p.ElementAtOrDefault(1);
            });
            AddFunc("arctan", (p) => System.Math.Atan2(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
            AddFunc("lesser", (p) => System.Math.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
            AddFunc("greater", (p) => System.Math.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
            AddFunc("clamp", (p) => Mathf.Clamp((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2)));
            AddFunc("range", (p) => Mathf.Clamp((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2)));
            AddFunc("movetowards", (p) =>
            {
                return Mathf.MoveTowards((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2));
            });

            AddFunc("lerp", (p) =>
            {
                return (double)Mathf.Lerp((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2));
            });
            AddFunc("movetowardsangle", (p) =>
            {
                return Mathf.MoveTowardsAngle((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2));
            });
            AddFunc("pow", (p) => System.Math.Pow((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1)));
            AddFunc("repeat", (p) => Mathf.Repeat((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1)));
            AddFunc("int", (p) => Mathf.FloorToInt((float)p.FirstOrDefault()));

            AddFunc("ceiltoint", (p) => Mathf.CeilToInt((float)p.FirstOrDefault()));
            AddFunc("floortoint", (p) => Mathf.FloorToInt((float)p.FirstOrDefault()));
            AddFunc("exp", (p) => Mathf.Exp((float)p.FirstOrDefault()));
            AddFunc("sign", (p) => Mathf.Sign((float)p.FirstOrDefault()));
            AddFunc("range", (p) => Mathf.Clamp((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1), (float)p.ElementAtOrDefault(2)));
            AddFunc("random", (p) => Random.Range((float)p.FirstOrDefault(), (float)p.ElementAtOrDefault(1)));
        }


        //public addTurleFunc




    }


}