using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public static class GPACalculator
    {
        public class Answer
        {
            public double GPA;
            public double CreditsAffectingGpa;
            public double CreditsEarned;
            public bool HasGrades;
        }

        public static Answer Calculate(IEnumerable<IGPACredits> lowerItems)
        {
            double gradePoints = 0;
            double creditsEarned = 0;
            double creditsAffectingGpa = 0;
            double gradePointsNone = 0;

            int count = 0;

            bool usingCredits = false;
            bool isGradesActive = false;

            bool hasGrades = false;

            foreach (IGPACredits g in lowerItems)
            {
                // If no credits
                if (g.CreditsAffectingGpa == -1)
                {
                    switch (GetGpaType(g))
                    {
                        case GpaType.PassFail:
                            // Nothing, since meaningless if doesn't have credits
                            break;

                        default:
                            {
                                // Only include if it has grades
                                if (g.HasGrades)
                                {
                                    gradePointsNone += g.GPA;
                                    count++;
                                    isGradesActive = true;
                                    hasGrades = true;
                                }
                            }
                            break;
                    }
                }

                // Otherwise, has credits
                else
                {
                    usingCredits = true;

                    // This works for both pass/fail classes and normal classes, and classes that don't have grades
                    creditsEarned += g.CreditsEarned;
                    creditsAffectingGpa += g.CreditsAffectingGpa;
                    gradePoints += g.GPA * g.CreditsAffectingGpa;
                    isGradesActive = true;

                    if (g.HasGrades)
                    {
                        hasGrades = true;
                    }
                }
            }


            Answer answer = new Answer();

            if (isGradesActive)
            {
                if (usingCredits)
                {
                    if (creditsAffectingGpa == 0)
                        answer.GPA = -1;
                    else
                        answer.GPA = gradePoints / creditsAffectingGpa;
                }

                else
                    answer.GPA = gradePointsNone / count;
            }

            else
                answer.GPA = -1;

            if (usingCredits)
            {
                answer.CreditsAffectingGpa = creditsAffectingGpa;
                answer.CreditsEarned = creditsEarned;
            }
            else
            {
                answer.CreditsAffectingGpa = -1;
                answer.CreditsEarned = -1;
            }

            answer.HasGrades = hasGrades;

            return answer;
        }

        public static GpaType GetGpaType(IGPACredits g)
        {
            if (g is ViewItemClass c)
            {
                return c.GpaType;
            }

            return GpaType.Standard;
        }

        public static bool IsPassing(IGPACredits g)
        {
            if (g is ViewItemClass c)
            {
                return c.IsPassing;
            }

            return false;
        }
    }
}
