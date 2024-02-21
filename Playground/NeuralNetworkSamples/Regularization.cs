namespace Playground;
using MathNet.Numerics.LinearAlgebra.Single;

using GradientDescentSharp.NeuralNetwork;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

public partial class Examples
{
    public static void Regularization()
    {
        float[][] X = [
            [1,2],
            [3,4],
            [4,4],
            [6,2],
            [6,1],
            [8,3],
            [7,3]
        ];

        float[][] Y = [
            [255,255],
            [235,180],
            [200,150],
            [170,100],
            [220,180],
            [120,90],
            [0,0]
        ];

        var plt = new Plot();
        var data = X.Select(DenseVector.OfArray).Zip(Y.Select(DenseVector.OfArray)).ToArray();
        //draw dots
        foreach (var (x, y) in data)
        {
            plt.Add.Scatter(
                x[0], x[1],
                System.Drawing.Color.FromArgb((int)y[0], (int)y[1], 0).ToScatter()).MarkerSize = 15;
        }

        var defaultFactory = new NNComplexObjectsFactory();
        var layer1 = new Layer(defaultFactory, 2, 16, ActivationFunction.Relu(), Initializers.Guassian);
        var layer2 = new Layer(defaultFactory, 16, 4, ActivationFunction.Tanh(), Initializers.GlorotUniform);
        var layer3 = new Layer(defaultFactory, 4, 2, ActivationFunction.Relu(), Initializers.GlorotUniform);

        var nn = new ForwardNN(layer1, layer2, layer3)
        {
            LearningRate = 0.01f,
            LearnerFactory = d => new DefaultLearner(d, new L2Regularization(0.01f))
            // LearnerFactory = d => new DefaultLearner(d, null)
        };

        for (int epoch = 0; epoch < 1000; epoch++)
        {
            foreach (var (x, y) in data.OrderBy(x => Random.Shared.Next()))
            {
                var bw = nn.Backwards(x, y / 100);
                bw.Learn();
            }
        }

        //draw predictions
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                float x = i * 1.0f / 100 * 7 + 1;
                float y = j * 1.0f / 100 * 5;
                var color = nn.Forward(DenseVector.OfArray([x, y])) * 100;
                color.MapInplace(t => Math.Min(t, 255));
                color.MapInplace(t => Math.Max(t, 0));
                if (color.Any(float.IsNaN)) continue;
                plt.Add.Scatter(
                x, y,
                System.Drawing.Color.FromArgb((int)color[0], (int)color[1], 0).ToScatter()).MarkerSize = 5;
            }
        }

        plt.SaveJpeg("res.jpg", 1000, 1000);
    }
}