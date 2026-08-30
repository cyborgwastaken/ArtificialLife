using NUnit.Framework;
using ArtificialLife;

public class NeuralNetworkTests
{
    [Test]
    public void ForwardPass_MatchesHandComputation()
    {
        // 2 -> 2 -> 1, all linear, weights from guide section 3.4.
        var net = new NeuralNetwork(new[] { 2, 2, 1 }, Activation.Linear, Activation.Linear);
        net.ReadParameters(new float[]
        {
            // Layer 1 weights (2 neurons x 2 inputs), row-major:
            0.5f, -0.5f,
            1.0f,  0.0f,
            // Layer 1 biases:
            0.0f, 1.0f,
            // Layer 2 weights (1 neuron x 2 inputs):
            2.0f, -1.0f,
            // Layer 2 bias:
            0.5f
        });

        float[] y = net.FeedForward(new float[] { 1f, 2f });

        Assert.AreEqual(-2.5f, y[0], 1e-5f);
    }

    [Test]
    public void RoundTrip_WriteThenRead_IsIdentity()
    {
        var rng = new Rng(1);
        var a = new NeuralNetwork(new[] { 8, 6, 3 });
        a.Randomize(rng);

        var genes = new float[a.ParameterCount];
        a.WriteParameters(genes);

        var b = new NeuralNetwork(new[] { 8, 6, 3 });
        b.ReadParameters(genes);

        float[] input = { 0.1f, -0.2f, 0.3f, 0f, 0.9f, -0.5f, 0.4f, 1f };
        Assert.AreEqual(a.FeedForward(input), b.FeedForward(input));
    }
}
