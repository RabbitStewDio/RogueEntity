using System;
using JetBrains.Annotations;

namespace RogueEntity.Generator.CellularAutomata
{
    public readonly struct CARules
    {
        readonly bool[] BirthRules;    // Transition from dead cell to alive cell
        readonly bool[] SurvivalRules; // Transition from alive cell to dead cell.

        public CARules([NotNull] bool[] birthRules, [NotNull] bool[] survivalRules)
        {
            BirthRules = birthRules ?? throw new ArgumentNullException(nameof(birthRules));
            SurvivalRules = survivalRules ?? throw new ArgumentNullException(nameof(survivalRules));
            
            if (BirthRules.Length < 9) throw new ArgumentException();
            if (SurvivalRules.Length < 9) throw new ArgumentException();
        }

        public bool Surive(int neighbours) => SurvivalRules[neighbours];
        public bool Birth(int neighbours) => BirthRules[neighbours];
    }
    
    public static class CARuleParser
    {
        public static CARules ParseRuleString(string ruleString)
        {
            var birthRules = new bool[9];
            var survivalRules = new bool[9];

            if (ruleString.Length != 0)
            {
                var bsSyntax = (ruleString[0] == 'b' || ruleString[0] == 'B');
                // B{n}/S{n} syntax.
                var isBornRuleFlag = bsSyntax;
                // state == true => set born flag 
                // state == false => clear death flag
                foreach (var c in ruleString)
                {
                    if (c == '/')
                    {
                        isBornRuleFlag = !isBornRuleFlag;
                    }
                    else if (char.IsDigit(c))
                    {
                        var idx = c - '0';
                        if (isBornRuleFlag)
                        {
                            birthRules[idx] = true;
                        }
                        else
                        {
                            survivalRules[idx] = true;
                        }
                    }
                }
            }

            return new CARules(birthRules, survivalRules);
        }

    }
}