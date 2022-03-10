using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;


using FuzzyBinary = System.Func<Tochas.FuzzyLogic.IFuzzyExpression, Tochas.FuzzyLogic.IFuzzyExpression, Tochas.FuzzyLogic.IFuzzyExpression>;
using FuzzyUnary = System.Func<Tochas.FuzzyLogic.IFuzzyExpression, Tochas.FuzzyLogic.IFuzzyExpression>;



// Extensions by Bob Kerner (OMSCS student in GameAI Su21)

namespace GameAI
{
    public partial class AIVehicle
    {
        protected static readonly Type FveType = typeof(FuzzyVariableExpression<>);
        protected static readonly Type FuzzyExpressionType = typeof(IFuzzyExpression);

        protected static class ExprCast<TExpr>
        {
            public static readonly Func<TExpr, IFuzzyExpression> Convert;
            static ExprCast()
            {
                var type = typeof(TExpr);
                if (FuzzyExpressionType.IsAssignableFrom(type))
                    Convert = a => (IFuzzyExpression)a;
                else
                {
                    var parameterizedType = FveType.MakeGenericType(new Type[] { type });
                    var ctor = parameterizedType
                      .GetConstructor(new Type[] { type });
                    var ctorParam = ctor?.GetParameters().First();
                    var param = Expression.Parameter(type, ctorParam?.Name);
                    var e = Expression.New(ctor ?? throw new InvalidOperationException(),
                      new Expression[] { param });
                    Convert = Expression.Lambda<Func<TExpr, IFuzzyExpression>>(e, param).Compile();
                }
            }
        }

        public static IFuzzyExpression Op<T1, T2>(T1 a, T2 b, FuzzyBinary fn)
          => fn(ExprCast<T1>.Convert(a), ExprCast<T2>.Convert(b));

        public static IFuzzyExpression Op<T>(T a, FuzzyUnary fn)
          => fn(ExprCast<T>.Convert(a));

        protected static IFuzzyExpression And<T1, T2>(T1 a, T2 b) =>
          Op(a, b, (x, y) => x.And(y));

        protected static IFuzzyExpression And<T1, T2, T3>(T1 a, T2 b, T3 c) =>
          And(a, And(b, c));

        protected static IFuzzyExpression And<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) =>
          And(a, And(b, c, d));

        protected static IFuzzyExpression Or<T1, T2>(T1 a, T2 b) =>
          Op(a, b, (x, y) => x.Or(y));

        protected static IFuzzyExpression Or<T1, T2, T3>(T1 a, T2 b, T3 c) =>
          Or(a, Or(b, c));

        protected static IFuzzyExpression Or<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) =>
          Or(a, Or(b, c, d));

        protected static IFuzzyExpression Not<T>(T a) =>
          ExprCast<T>.Convert(a).Not();

        protected static IFuzzyExpression Very<T>(T a) =>
          new FuzzyVery(ExprCast<T>.Convert(a));

        protected static IFuzzyExpression Fairly<T>(T a) =>
          new FuzzyFairly(ExprCast<T>.Convert(a));

        protected static IFuzzyExpression Nor<T1, T2>(T1 a, T2 b) =>
          Not(Or(a, b));

        protected static IFuzzyExpression Nor<T1, T2, T3>(T1 a, T2 b, T3 c) =>
          Not(Or(a, b, c));

        protected static IFuzzyExpression Nor<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) =>
          Not(Or(a, b, c, d));

        protected static IFuzzyExpression Nand<T, U>(T a, U b) =>
          Not(And(a, b));

        protected static IFuzzyExpression Nand<T1, T2, T3>(T1 a, T2 b, T3 c) =>
          Not(And(a, b, c));

        protected static IFuzzyExpression Nand<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) =>
          Not(And(a, b, c, d));

        protected static IFuzzyExpression If<T>(T a)
          => ExprCast<T>.Convert(a);

        protected static Func<FuzzyValueSet, float> BuildEvaluationFunction<T>(
          [NotNull] List<FuzzyRule<T>> rules,
          [NotNull] FuzzySet<T> set,
          IDefuzzer<T> defuzzer = null, // Default: MaxAvDefuzzer
                IFuzzyValuesMerger<T> merger = null // Default: CachedOutputsFuzzyValuesMerger
            )
          where T : struct, IConvertible
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (set == null) throw new ArgumentNullException(nameof(set));
            defuzzer = defuzzer ?? new MaxAvDefuzzer<T>();
            merger = new CachedOutputsFuzzyValuesMerger<T>();
            var evaluator = new RuleEvaluator<T>();
            var mergedValues = new FuzzyValueSet();
            return inputs =>
            {
                var outputs = evaluator.EvaluateRules(rules, inputs);
                merger.MergeValues(outputs, mergedValues);
                return defuzzer.Defuzze(set, mergedValues);
            };
        }


    }

}