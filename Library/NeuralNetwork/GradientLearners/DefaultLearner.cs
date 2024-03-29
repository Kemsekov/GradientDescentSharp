using System.Runtime.CompilerServices;

namespace GradientDescentSharp.NeuralNetwork;

/// <summary>
/// Applies backpropagation to model weights in ordinary way
/// </summary>
public record DefaultLearner(LearningData LearningData, IRegularization? Regularization = null) : LearnerBase(LearningData)
{
    ///<inheritdoc/>
    public static Func<LearningData, ILearner> Factory(IRegularization? regularization = null)
    {
        return x => new DefaultLearner(x, regularization);
    }

    ///<inheritdoc/>
    public unsafe override void Learn()
    {

        var reg = Regularization ?? new NoRegularization();
        var bgSpan = biasesGradient.AsSpan();
        var liSpan = layerInput.AsSpan();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float weightUpdate(int j, int k, Span<float> bg, Span<float> li, float weight)
        {
            var weightGradient = bg[j] * li[k] + reg.WeightDerivative(weight);

            //by any reason you can get NaN while learning, in such case
            //we will just zero out weight update
            //and yes, this is faster than if block because we avoid branching
            //on cpu execution
            float isNan = float.IsNaN(weightGradient) ? 0 : 1;

            return weight - learningRate * isNan * weightGradient;
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe float biasUpdate(int j, System.Span<float> s, float x)
        {
            return x - learningRate * s[j];
        }
        layer.Weights.MapInplace(bgSpan, liSpan, weightUpdate);
        layer.Bias.VecMapInplace(bgSpan, biasUpdate);
    }

    /// <summary>
    /// Unlearns last learned weights and biases
    /// </summary>
    public override void Unlearn()
    {
        if (Regularization is not null)
        {
            throw new NotSupportedException("Unlearn supported only on learners without regularization");
        }
        var weightsGradient = (int j, int k) => biasesGradient.VecAt(j) * layerInput.VecAt(k);
        layer.Weights.MapInplace((j,k, x) => x + learningRate * weightsGradient(j,k));
        layer.Bias.VecMapInplace((j, x) => x + learningRate * biasesGradient.VecAt(j));
    }
}
