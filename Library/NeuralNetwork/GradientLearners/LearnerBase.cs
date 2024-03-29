namespace GradientDescentSharp.NeuralNetwork;

/// <summary>
/// Learner interface
/// </summary>
public interface ILearner{
    /// <summary>
    /// Learning data
    /// </summary>
    LearningData LearningData{get;}
    /// <summary>
    /// Learns
    /// </summary>
    void Learn();
    /// <summary>
    /// Unlearns
    /// </summary>
    void Unlearn();
}

/// <summary>
/// Learns something, and can unlearn it too
/// </summary>
public abstract record LearnerBase(LearningData LearningData) : ILearner
{
    /// <summary>
    /// Learning layer
    /// </summary>
    protected ILayer layer => LearningData.layer;
    /// <summary>
    /// Gradient of basis
    /// </summary>
    protected FTensor biasesGradient => LearningData.biasesGradient;
    /// <summary>
    /// Input of layer
    /// </summary>
    protected FTensor layerInput => LearningData.layerInput;
    /// <summary>
    /// Learning rate
    /// </summary>
    protected float learningRate => LearningData.learningRate;
    ///<inheritdoc/>
    public abstract void Learn();
    ///<inheritdoc/>
    public abstract void Unlearn();
}

///<inheritdoc/>
public record LearningData(ILayer layer, FTensor biasesGradient, FTensor layerInput, float learningRate);