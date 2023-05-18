namespace GradientDescentSharp.NeuralNetwork;
public interface ILayer
{
    Matrix Weights{get;}
    Vector Bias{get;}
    IActivationFunction Activation{get;}
    Vector Forward(Vector input);
    void Learn(Vector biasesGradient, Vector layerInput, FloatType learningRate);
    public void Unlearn();
}
