using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GradientDescentSharp.GradientDescents;

public class NaturalDescent : GradientDescentBase
{
    /// <summary>
    /// Descent process can be logged here
    /// </summary>
    public ILogger? Logger;
    Func<IDataAccess<double>, double> likelihood;
    /// <summary>
    /// Used to randomly sample given parameters.<br/>
    /// When computing fisher information, we will need to compute expectation over
    /// some range of parameter space over some function, so this method is used
    /// to generate parameters.
    /// </summary>
    public Func<int, double> GenerateParameterSample;
    private Vector<double> parametersVector;
    /// <summary>
    /// How much decrease descent rate when we step into bigger error value.<br/>
    /// By default it is 0.1, so when we step into worse error function value,
    /// we will divide  learning rate by 10.
    /// </summary>
    public double DescentRateDecreaseRate = 0.1;
    /// <summary>
    /// To compute expectation for fisher information, we need to generate a range of samples.
    /// This parameter describes how much. General rule is that count of samples should grow
    /// exponentially proportional to parameters dimensions
    /// </summary>
    public int ExpectationsSampleCount = 100;
    public NaturalDescent(IDataAccess<double> variables, Func<IDataAccess<double>, double> function) : base(variables, function)
    {
        //to transform error function to likelihood function, use following mapping
        likelihood = (IDataAccess<double> x) =>
        {
            var err = function(x);
            return Math.Log(Math.Exp(-err) * (err + 1) / 2);
            // return Math.Exp(-err) * (err + 1) / 2;
        };
        GenerateParameterSample = i => Random.Shared.NextDouble() * 2 - 1;
        this.parametersVector = new ComplexObjectsFactory(variables).CreateVector(variables.Length);
    }
    /// <summary>
    /// Computes fisher information matrix using monte-carlo approximation.<br/>
    /// It generates <see cref="ExpectationsSampleCount"/> samples and returns
    /// computed information matrix as average of sample results.
    /// </summary>
    /// <returns></returns>
    public Matrix ComputeFisherInformationMatrix()
    {
        var matrix = DenseMatrix.Create(Dimensions, Dimensions, 0);
        Parallel.For(0, ExpectationsSampleCount, k =>
        {
            using var arr = ArrayPoolStorage.RentArray<double>(Dimensions);
            var variables = new RentedArrayDataAccess<double>(arr);
            for (int i = 0; i < variables.Length; i++)
            {
                variables[i] = GenerateParameterSample(i);
            }

            using var derivative = derivativeOfLikelihood(variables);
            lock (matrix)
                for (int i = 0; i < Dimensions; i++)
                {
                    var iDerivative = derivative[i];
                    for (int j = 0; j < Dimensions; j++)
                    {
                        var jDerivative = derivative[j];
                        matrix[i, j] += iDerivative * jDerivative;
                    }
                }
        });
        matrix.MapInplace(x => x / ExpectationsSampleCount);
        return matrix;
    }
    /// <summary>
    /// This method computes derivative of likelihood function,
    /// which is used for computing fisher information matrix
    /// </summary>
    RentedArray<double> derivativeOfLikelihood(IDataAccess<double> Variables)
    {
        var derivativeOfLikelihood = ArrayPoolStorage.RentArray<double>(Variables.Length);
        for (int i = 0; i < Variables.Length; i++)
        {
            Variables[i] += Theta;
            var b = likelihood(Variables);
            Variables[i] -= Theta;

            var c = likelihood(Variables);

            derivativeOfLikelihood[i] = (b - c) / Theta;
        };
        return derivativeOfLikelihood;
    }
    void ComputeChange(IDataAccess<double> change, double learningRate, double currentEvaluation, Matrix fisherInformationInverse)
    {
        ComputeGradient(change, currentEvaluation);
        var gradientVector = new ComplexObjectsFactory(change).CreateVector(change.Length);
        var result = learningRate*fisherInformationInverse*gradientVector;
        for(int i = 0;i<change.Length;i++)
            change[i] = result[i];
    }
    public override int Descent(int maxIterations)
    {
        var fisherInformationInverse = (Matrix)ComputeFisherInformationMatrix().Inverse();
        Logger?.LogLine("--------------Natural descent began");

        using RentedArrayDataAccess<double> change = new(ArrayPoolStorage.RentArray<double>(Dimensions));
        var iterations = 0;
        var descentRate = DescentRate;
        var beforeStep = Evaluate(Variables);
        while (iterations++ < maxIterations)
        {
            ComputeChange(change, descentRate, beforeStep,fisherInformationInverse);
            Step(change);
            var afterStep = Evaluate(Variables);
            var diff = Math.Abs(afterStep - beforeStep);
            Logger?.LogLine($"Error is {afterStep}");
            Logger?.LogLine($"Changed by {diff}");
            if (diff <= Theta) break;
            if (afterStep >= beforeStep || double.IsNaN(afterStep))
            {
                Logger?.LogLine($"Undo step. Decreasing descentRate.");
                UndoStep(change);
                descentRate *= DescentRateDecreaseRate;
            }
            else
            {
                beforeStep = afterStep;
            }
            Logger?.LogLine($"-------------");
        }
        Logger?.LogLine($"--------------Natural descent done in {iterations} iterations");

        return iterations;
    }
}
