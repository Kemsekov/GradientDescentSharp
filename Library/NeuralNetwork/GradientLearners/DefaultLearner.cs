namespace GradientDescentSharp.NeuralNetwork;

/// <summary>
/// Applies backpropagation to model weights in ordinary way
/// </summary>
public record DefaultLearner(LearningData LearningData) : LearnerBase(LearningData)
{
    public static Func<LearningData, ILearner> Factory()
    {
        return x=>new DefaultLearner(x);
    }

    public override void Learn()
    {
        for (int k = 0; k < layerInput.Count; k++)
        {
            var kInput = layerInput[k];
            if (kInput == 0) continue;
            for (int j = 0; j < layer.Weights.RowCount; j++)
            {
                var weightGradient = biasesGradient[j] * kInput;
                layer.Weights[j, k] -= learningRate * weightGradient;
            }
        }
        layer.Bias.MapIndexedInplace((j, x) => x - learningRate * biasesGradient[j]);

    }
    /// <summary>
    /// Unlearns last learned weights and biases
    /// </summary>
    public override void Unlearn()
    {
        var weightsGradient = (int j, int k) => biasesGradient[j] * layerInput[k];
        layer.Weights.MapIndexedInplace((j, k, x) => x + learningRate * weightsGradient(j, k));
        layer.Bias.MapIndexedInplace((j, x) => x + learningRate * biasesGradient[j]);
    }
}
